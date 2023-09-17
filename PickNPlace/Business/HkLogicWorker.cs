using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PickNPlace.Camera;
using PickNPlace.Plc;
using PickNPlace.Plc.Data;
using PickNPlace.DTO;
using PickNPlace.DataAccess;

namespace PickNPlace.Business
{
    public class HkLogicWorker : IDisposable
    {
        #region SINGLETON PATTERN
        public static HkLogicWorker _Instance;

        private HkLogicWorker() {
            _autoLogic = new HkAutoLogic();
            _plcWorker = PlcWorker.Instance();
            _palletList = new List<HkAutoPallet>();

            _plcDb = PlcDB.Instance();
            _plcDb.OnRobotPickedUp += _plcDb_OnRobotPickedUp;
            _plcDb.OnRobotPlacedDown += _plcDb_OnRobotPlacedDown;
            _plcDb.OnSystemAutoChanged += _plcDb_OnSystemAutoChanged;

            this.Start();
        }

        public static HkLogicWorker GetInstance()
        {
            if (_Instance == null)
                _Instance = new HkLogicWorker();

            return _Instance;
        }
        #endregion

        #region EVENTS & DELEGATES
        
        #endregion

        // environmental variables
        private HkAutoLogic _autoLogic;
        private PlcDB _plcDb;
        private PlcWorker _plcWorker;
        private IList<HkAutoPallet> _palletList;
        private Task _taskLoop;
        private bool _runLoop;

        // flag variables
        int _currentTargetPalletNo = 0;
        int _currentRawPalletNo = 1;
        bool _rawMaterialSelectionPalletOk = false;
        bool _targetSelectionPalletOk = false;
        bool _robotSentToPlaceDown = false;
        bool _robotPlacedDown = false;
        bool _robotPickedUp = false;
        bool _captureOk = false;

        public int CurrentRawPalletNo
        {
            get { return _currentRawPalletNo; }
        }

        public int CurrentTargetPalletNo
        {
            get
            {
                return _currentTargetPalletNo;
            }
        }

        public void ClearPallets()
        {
            _palletList.Clear();
        }
        public void SetPalletAttributes(int palletNo, bool isRawMaterial, bool isEnabled, string placeRequestOrRawCode)
        {
            var plt = _palletList.FirstOrDefault(d => d.PalletNo == palletNo);
            if (plt == null)
            {
                plt = new HkAutoPallet { PalletNo = palletNo, PalletWidth = 1000, PalletHeight = 1200 };
                _palletList.Add(plt);
            }

            plt.IsRawMaterial = isRawMaterial;
            plt.IsEnabled = isEnabled;

            // search for the place order request
            bool requestFound = false;
            if (!string.IsNullOrEmpty(placeRequestOrRawCode))
            {
                if (!isRawMaterial)
                {
                    plt.RawMaterialCode = "";

                    using (HekaDbContext db = SchemaFactory.CreateContext())
                    {
                        var dbReq = db.PlaceRequest.FirstOrDefault(d => d.RequestNo == placeRequestOrRawCode);
                        if (dbReq != null)
                        {
                            PlaceRequestDTO pReq = new PlaceRequestDTO
                            {
                                BatchCount = dbReq.BatchCount,
                                Id = dbReq.Id,
                                RecipeCode = dbReq.RecipeCode,
                                RecipeName = dbReq.RecipeName,
                                RequestNo = dbReq.RequestNo,
                            };

                            pReq.Items = db.PlaceRequestItem.Where(d => d.PlaceRequestId == dbReq.Id)
                                    .Select(d => new PlaceRequestItemDTO
                                    {
                                        Id = d.Id,
                                        ItemCode = d.RawMaterial != null ? d.RawMaterial.ItemCode : "",
                                        PiecesPerBatch = d.PiecesPerBatch,
                                        PlaceRequestId = d.PlaceRequestId,
                                        RawMaterialId = d.RawMaterialId,
                                        SackType = 0,
                                    }).ToArray();

                            _autoLogic.SetRequestForPallet(pReq, palletNo);

                            requestFound = true;
                        }
                    }
                }
                else
                {
                    plt.PlaceRecipeCode = "";
                    plt.RawMaterialCode = placeRequestOrRawCode;

                    requestFound = true;
                }
            }

            if (!requestFound)
            {
                plt.IsEnabled = false;
            }
        }

        public void SetPalletEnabled(int palletNo, bool isEnabled)
        {
            var plt = _palletList.FirstOrDefault(d => d.PalletNo == palletNo);
            if (plt != null)
            {
                plt.IsEnabled = isEnabled;
            }
        }

        public IList<HkAutoPallet> GetPalletList()
        {
            return _palletList;
        }

        public void SetPalletSackType(int palletNo, int sackType)
        {
            var plt = _palletList.FirstOrDefault(d => d.PalletNo == palletNo);
            if (plt != null)
            {
                plt.SackType = sackType;
            }
        }

        private void Start()
        {
            using (HekaDbContext db = SchemaFactory.CreateContext())
            {
                int tryValue = 0;

                var prmData = db.SysParam.FirstOrDefault(d => d.ParamCode == "ServoSpeed");
                if (prmData != null && Int32.TryParse(prmData.ParamValue, out tryValue))
                {
                    this._plcWorker.Set_ServoSpeed(Convert.ToInt32(prmData.ParamValue));
                }

                prmData = db.SysParam.FirstOrDefault(d => d.ParamCode == "ServoPosCam1");
                if (prmData != null && Int32.TryParse(prmData.ParamValue, out tryValue))
                {
                    this._plcWorker.Set_ServoPosCam1(Convert.ToInt32(prmData.ParamValue));
                }

                prmData = db.SysParam.FirstOrDefault(d => d.ParamCode == "ServoPosCam2");
                if (prmData != null && Int32.TryParse(prmData.ParamValue, out tryValue))
                {
                    this._plcWorker.Set_ServoPosCam2(Convert.ToInt32(prmData.ParamValue));
                }

                this._plcWorker.Set_ServoStart(1);
            }

            _runLoop = true;
            _taskLoop = Task.Run(LoopFunc);
        }

        private void Stop()
        {
            _runLoop = false;

            try
            {
                _taskLoop.Dispose();
            }
            catch (Exception)
            {

            }
        }

        public void Dispose()
        {
            this.Stop();
        }

        private async Task LoopFunc()
        {
            while (_runLoop)
            {
                if (_plcDb.System_Auto)
                {
                    // select proper raw material pallet
                    if (!_rawMaterialSelectionPalletOk)
                        CheckRawPallets();

                    // trigger camera and pick up
                    if (_rawMaterialSelectionPalletOk && !_captureOk)
                    {
                        var trgOk = TriggerCamera(_currentRawPalletNo.ToString());
                        if (trgOk)
                            _captureOk = true;
                    }

                    // select next pallet to place
                    if (_captureOk && _robotPickedUp)
                    {
                        if (!_targetSelectionPalletOk)
                            CheckNextTargetPallet();

                        if (_targetSelectionPalletOk)
                        {
                            if (!_robotSentToPlaceDown)
                            {
                                _robotPlacedDown = false;
                                var sendResult = SendRobotToPlaceDown();
                                if (sendResult)
                                    _robotSentToPlaceDown = true;
                            }
                        }
                    }

                    // wait for placed down signal from robot and then reset all flags to // pick & place // again
                    if (_robotSentToPlaceDown && _robotPlacedDown)
                    {
                        _autoLogic.SignWaitingPlacementIsMade(_currentTargetPalletNo);

                        _captureOk = false;
                        _robotPickedUp = false;
                        _targetSelectionPalletOk = false;
                        _robotSentToPlaceDown = false;
                        _rawMaterialSelectionPalletOk = false;
                    }
                }

                await Task.Delay(100);
            }
        }

        private void CheckRawPallets()
        {
            _rawMaterialSelectionPalletOk = false;

            try
            {
                // first try on current one
                var plt = _palletList.FirstOrDefault(d => d.PalletNo == _currentRawPalletNo);
                if (plt != null && plt.IsRawMaterial)
                {
                    foreach (var pallet in _palletList)
                    {
                        var isProperItem = _autoLogic.CheckIsProperItem(pallet.PalletNo, plt.RawMaterialCode);
                        if (isProperItem)
                        {
                            _rawMaterialSelectionPalletOk = true;
                            break;
                        }
                    }
                }

                // second try on other
                if (!_rawMaterialSelectionPalletOk)
                {
                    var nextRawPalletNo = _currentRawPalletNo == 1 ? 2 : 1;
                    plt = _palletList.FirstOrDefault(d => d.PalletNo == nextRawPalletNo);
                    if (plt != null && plt.IsRawMaterial)
                    {
                        foreach (var pallet in _palletList)
                        {
                            var isProperItem = _autoLogic.CheckIsProperItem(pallet.PalletNo, plt.RawMaterialCode);
                            if (isProperItem)
                            {
                                _rawMaterialSelectionPalletOk = true;
                                _currentRawPalletNo = nextRawPalletNo;
                                break;
                            }
                        }
                    }
                }

                // drive the servo to proper camera
                if (_rawMaterialSelectionPalletOk)
                {
                    this.SwitchCamera(_currentRawPalletNo);
                }
                else
                {
                    // light up red buzzer
                }
            }
            catch (Exception)
            {

            }
        }

        private void CheckNextTargetPallet()
        {
            _targetSelectionPalletOk = false;

            if (_currentTargetPalletNo == 0)
                _currentTargetPalletNo = 6;

            var currentRawPallet = _palletList.FirstOrDefault(d => d.PalletNo == _currentRawPalletNo);
            var currentRawMaterial = currentRawPallet != null ? currentRawPallet.RawMaterialCode : "";

            if (!string.IsNullOrEmpty(currentRawMaterial))
            {
                var nextPallet = _palletList
                    .Where(d => !d.IsRawMaterial && d.IsEnabled && d.PalletNo < _currentTargetPalletNo)
                    .OrderByDescending(d => d.PalletNo).FirstOrDefault();

                while (!_autoLogic.CheckIsProperItem(nextPallet.PalletNo, currentRawMaterial))
                {
                    _currentTargetPalletNo--;

                    nextPallet = _palletList
                        .Where(d => !d.IsRawMaterial && d.IsEnabled && d.PalletNo < _currentTargetPalletNo)
                        .OrderByDescending(d => d.PalletNo).FirstOrDefault();
                }

                if (_autoLogic.CheckIsProperItem(nextPallet.PalletNo, currentRawMaterial))
                {
                    _targetSelectionPalletOk = true;
                }
                else
                {
                    // light up red buzzer
                }
            }
        }

        private bool SendRobotToPlaceDown()
        {
            try
            {
                var rawMatPallet = _palletList.FirstOrDefault(d => d.PalletNo == _currentRawPalletNo && d.IsRawMaterial && d.IsEnabled);

                // try place an item from a raw pallet
                int placeResult = _autoLogic.PlaceAnItem(_currentTargetPalletNo, rawMatPallet.RawMaterialCode, rawMatPallet.SackType);

                if (placeResult == 0) // successfull
                {
                    var posItem = _autoLogic.GetWaitingItem(_currentTargetPalletNo);
                    var currentFloor = _autoLogic.GetCurrentFloor(_currentTargetPalletNo);

                    this._plcWorker.Set_EmptyPalletNo(_currentTargetPalletNo);

                    this._plcWorker.Set_RobotX(posItem.PlacedX);
                    this._plcWorker.Set_RobotY(posItem.PlacedY);
                    this._plcWorker.Set_RobotZ(currentFloor * 9 * 10 * -1);
                    this._plcWorker.Set_RobotRX(0);
                    this._plcWorker.Set_RobotRY(0);

                    this._plcWorker.Set_RobotRZ(posItem.IsRotated ? -179 : -89);
                    this._plcWorker.Set_PlaceCalculationOk(1);

                    return true;
                }
                else // an error occured
                {

                }
            }
            catch (Exception)
            {

            }

            return false;
        }

        private void _plcDb_OnRobotPlacedDown()
        {
            _robotPlacedDown = true;
            _plcWorker.Set_RobotPlacingOk(0);
        }

        private void _plcDb_OnRobotPickedUp()
        {
            _robotPickedUp = true;
            _plcWorker.Set_RobotPickingOk(0);
        }

        private void _plcDb_OnSystemAutoChanged(bool auto)
        {
            
        }

        private void SwitchCamera(int rawPalletNo)
        {
            if (rawPalletNo == 1)
            {
                this._plcWorker.Set_ServoTargetPos(_plcDb.Servo_PosCam1);
            }
            else if (rawPalletNo == 2)
            {
                this._plcWorker.Set_ServoTargetPos(_plcDb.Servo_PosCam2);
            }
        }

        private bool TriggerCamera(string programId)
        {
            bool result = false;

            MechWorker camera = new MechWorker();
            if (camera.TriggerCamera(programId))
            {
                this._plcWorker.Set_PlaceCalculationOk(0);

                string posRaw = camera.GetVisionTargets(programId);

                int[] posData = new int[0];

                try
                {
                    posData = (posRaw.Split(',')).Skip(5).Take(6).Select(d => Convert.ToInt32(Convert.ToSingle(d))).ToArray();
                }
                catch (Exception)
                {

                }

                if (posData.Length > 0)
                {
                    this._plcWorker.Set_RobotX(posData[0]);
                    this._plcWorker.Set_RobotY(posData[1]);
                    this._plcWorker.Set_RobotZ(posData[2]);
                    this._plcWorker.Set_RobotRX(posData[3]);
                    this._plcWorker.Set_RobotRY(posData[4]);
                    this._plcWorker.Set_RobotRZ(posData[5]);

                    this._plcWorker.Set_CaptureOk(1);

                    result = true;
                }
            }

            return result;
        }

    }
}
