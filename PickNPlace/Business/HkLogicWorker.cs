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
            //_plcDb.OnRobotPickedUp += _plcDb_OnRobotPickedUp;
            //_plcDb.OnRobotPlacedDown += _plcDb_OnRobotPlacedDown;
            //_plcDb.OnSystemAutoChanged += _plcDb_OnSystemAutoChanged;

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
        public delegate void ActivePalletChanged();
        public event ActivePalletChanged OnActivePalletChanged;

        public delegate void AnErrorOccured(string message);
        public event AnErrorOccured OnError;

        public delegate void SystemModeChanged(bool mode);
        public event SystemModeChanged OnSystemModeChanged;
        #endregion

        // environmental variables
        private HkAutoLogic _autoLogic;
        private PlcDB _plcDb;
        private PlcWorker _plcWorker;
        private IList<HkAutoPallet> _palletList;
        private Task _taskLoop;
        private bool _oldSystemMode = false;
        private bool _runLoop;

        // flag variables
        int _currentTargetPalletNo = 0;
        int _currentRawPalletNo = 1;
        bool _rawMaterialSelectionPalletOk = false;
        bool _targetSelectionPalletOk = false;
        bool _robotSentToPlaceDown = false;
        bool _robotPlacedDown = false;
        bool _robotTargetCoordsReady = false;
        bool _robotPickedUp = false;
        bool _makeSwitchCamera = false;
        bool _captureOk = false;

        public HkAutoPallet GetPalletData(int palletNo)
        {
            return _autoLogic.GetPalletData(palletNo);
        }

        public PlaceRequestDTO GetPalletRequest(int palletNo)
        {
            return _autoLogic.GetRequestForPallet(palletNo);
        }

        public bool Sim_PlaceItem(int palletNo, string itemCode, int sackType = 3)
        {
            var placingResult = _autoLogic.PlaceAnItem(palletNo, itemCode, sackType);
            if (placingResult == 0)
                _autoLogic.SignWaitingPlacementIsMade(palletNo);

            return placingResult == 0;
        }

        public bool Sim_RemoveLastItem(int palletNo)
        {
            return _autoLogic.RemoveLastPlacedItem(palletNo);
        }

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
            _autoLogic.ClearPallets();

            _rawMaterialSelectionPalletOk = false;
            _targetSelectionPalletOk = false;
            _robotSentToPlaceDown = false;
            _robotPlacedDown = false;
            _robotTargetCoordsReady = false;
            _robotPickedUp = false;
            _makeSwitchCamera = false;
            _captureOk = false;

            _currentRawPalletNo = 1;
            _currentTargetPalletNo = 0;
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
                    this._plcDb.Servo_PosCam1 = Convert.ToInt32(prmData.ParamValue);
                }

                prmData = db.SysParam.FirstOrDefault(d => d.ParamCode == "ServoPosCam2");
                if (prmData != null && Int32.TryParse(prmData.ParamValue, out tryValue))
                {
                    this._plcWorker.Set_ServoPosCam2(Convert.ToInt32(prmData.ParamValue));
                    this._plcDb.Servo_PosCam2 = Convert.ToInt32(prmData.ParamValue);
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
            bool wrResult = false;

            while (_runLoop)
            {
                var systemAuto = _plcWorker.Get_SystemAuto();
                _plcDb.System_Auto = systemAuto;
                if (_oldSystemMode != systemAuto)
                    OnSystemModeChanged?.Invoke(systemAuto);
                _oldSystemMode = systemAuto;

                if (systemAuto)
                {
                    await PrepareRawMaterial();

                    // select next pallet to place
                    
                    if (!_targetSelectionPalletOk)
                        CheckNextTargetPallet();

                    if (_targetSelectionPalletOk)
                    {
                        if (!_robotTargetCoordsReady)
                        {
                            var sendResult = SendRobotToPlaceDown();
                            if (sendResult)
                                _robotTargetCoordsReady = true;
                            //else
                            //    _plcWorker.ReConnect();
                        }

                        var robotPickingOk = _plcWorker.Get_RobotPickingOk();
                        if (robotPickingOk && _robotTargetCoordsReady && _captureOk)
                        {
                            wrResult = _plcWorker.Set_RobotNextTargetOk(1);
                            if (wrResult)
                            {
                                var zeroResult = _plcWorker.Set_CaptureOk(0);
                                if (zeroResult)
                                {
                                    zeroResult = _plcWorker.Set_RobotPickingOk(0);
                                    if (zeroResult)
                                    {
                                        wrResult = this._plcWorker.Set_PlaceCalculationOk(1);
                                        if (wrResult)
                                        {
                                            _robotSentToPlaceDown = true;
                                        }
                                        //else
                                        //    _plcWorker.ReConnect();
                                    }
                                    //else
                                    //    _plcWorker.ReConnect();
                                }
                                //else
                                //    _plcWorker.ReConnect();
                            }
                            //else
                            //    _plcWorker.ReConnect();
                        }
                    }


                    // wait for placed down signal from robot and then reset all flags to // pick & place // again
                    var robotPlacingOk = _plcWorker.Get_RobotPlacingOk();
                    if (_robotSentToPlaceDown && robotPlacingOk)
                    {
                        wrResult = _plcWorker.Set_RobotPlacingOk(0);
                        if (wrResult)
                        {
                            _autoLogic.SignWaitingPlacementIsMade(_currentTargetPalletNo);
                            _plcWorker.Set_PlaceCalculationOk(0);
                            _plcWorker.Set_CaptureOk(0);
                            _plcWorker.Set_RobotNextTargetOk(0);

                            _robotTargetCoordsReady = false;
                            _captureOk = false;
                            _robotPickedUp = false;
                            _targetSelectionPalletOk = false;
                            _robotSentToPlaceDown = false;
                            _rawMaterialSelectionPalletOk = false;

                            await PrepareRawMaterial();
                        }
                        //else
                        //    _plcWorker.ReConnect();
                    }
                }

                await Task.Delay(200);
            }
        }

        private async Task PrepareRawMaterial()
        {
            bool wrResult = false;

            // select proper raw material pallet
            if (!_rawMaterialSelectionPalletOk)
                await CheckRawPallets();

            // trigger camera and pick up
            if (_rawMaterialSelectionPalletOk && !_captureOk)
            {
                var trgOk = TriggerCamera(_currentRawPalletNo.ToString());
                if (trgOk)
                {
                    wrResult = this._plcWorker.Set_CaptureOk(1);
                    if (wrResult)
                        _captureOk = true;
                }
            }
        }

        private async Task CheckRawPallets()
        {
            _rawMaterialSelectionPalletOk = false;

            var tmpOk = false;

            try
            {
                var oldRawPalletNo = _currentRawPalletNo;

                if (_palletList.Any(d => (d.PalletNo == 1 || d.PalletNo == 2) && d.IsEnabled && d.IsRawMaterial && !string.IsNullOrEmpty(d.RawMaterialCode))
                    //&& (
                    //    _palletList.Any(d => d.PalletNo == 2 && (d.IsEnabled == false || d.IsRawMaterial == false)) ||
                    //    _palletList.Any(d => d.PalletNo == 2 && d.IsEnabled && d.IsRawMaterial && !string.IsNullOrEmpty(d.RawMaterialCode))
                    //   )
                 )
                {
                    if (_makeSwitchCamera)
                    {
                        var currentPallet = _palletList.FirstOrDefault(d => d.PalletNo == _currentRawPalletNo);
                        if (currentPallet != null)
                        {
                            currentPallet.IsEnabled = false;
                        }

                        _makeSwitchCamera = false;
                    }

                    // first try on current one
                    var plt = _palletList.FirstOrDefault(d => d.PalletNo == _currentRawPalletNo);
                    if (plt != null && plt.IsRawMaterial && plt.IsEnabled)
                    {
                        foreach (var pallet in _palletList)
                        {
                            if (pallet.IsEnabled && !pallet.IsRawMaterial)
                            {
                                var isProperItem = _autoLogic.CheckIsProperItem(pallet.PalletNo, plt.RawMaterialCode);
                                if (isProperItem)
                                {
                                    tmpOk = true;
                                    break;
                                }
                            }
                        }
                    }

                    // second try on other
                    if (!tmpOk && !_rawMaterialSelectionPalletOk)
                    {
                        var nextRawPalletNo = _currentRawPalletNo == 1 ? 2 : 1;
                        plt = _palletList.FirstOrDefault(d => d.PalletNo == nextRawPalletNo);
                        if (plt != null && plt.IsRawMaterial && plt.IsEnabled)
                        {
                            foreach (var pallet in _palletList)
                            {
                                if (pallet.IsEnabled && !pallet.IsRawMaterial)
                                {
                                    var isProperItem = _autoLogic.CheckIsProperItem(pallet.PalletNo, plt.RawMaterialCode);
                                    if (isProperItem)
                                    {
                                        tmpOk = true;
                                        _currentRawPalletNo = nextRawPalletNo;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    // drive the servo to proper camera
                    if (tmpOk)
                    {
                        await this.SwitchCamera(_currentRawPalletNo);

                        if (_currentRawPalletNo != oldRawPalletNo)
                        {
                            OnActivePalletChanged?.Invoke();
                        }

                        _rawMaterialSelectionPalletOk = true;
                    }
                    else
                    {
                        // light up red buzzer
                    }
                }
                else
                    OnError?.Invoke("LÜTFEN HAMMADDE PALETİ VE ÜRÜNÜNÜ SEÇİNİZ.");
            }
            catch (Exception)
            {

            }
        }

        private void CheckNextTargetPallet()
        {
            _targetSelectionPalletOk = false;

            var _oldTargetPallet = _currentTargetPalletNo;

            if (_currentTargetPalletNo <= 0)
                _currentTargetPalletNo = 6;

            var currentRawPallet = _palletList.FirstOrDefault(d => d.PalletNo == _currentRawPalletNo);
            var currentRawMaterial = currentRawPallet != null ? currentRawPallet.RawMaterialCode : "";

            try
            {
                if (!string.IsNullOrEmpty(currentRawMaterial))
                {
                    var nextPallet = _palletList
                            .Where(d => !d.IsRawMaterial && d.IsEnabled && d.PalletNo <= _currentTargetPalletNo)
                            .OrderByDescending(d => d.PalletNo).FirstOrDefault();
                    //var nextPallet = _palletList
                    //    .Where(d => !d.IsRawMaterial && d.IsEnabled && d.PalletNo < _currentTargetPalletNo)
                    //    .OrderByDescending(d => d.PalletNo).FirstOrDefault();

                    if (nextPallet != null)
                    {

                        while (!_autoLogic.CheckIsProperItem(nextPallet.PalletNo, currentRawMaterial) || !nextPallet.IsEnabled)
                        {
                            _currentTargetPalletNo--;

                            if (_currentTargetPalletNo <= 0)
                            {
                                _currentTargetPalletNo = 0;
                                break;
                            }

                            nextPallet = _palletList
                                .Where(d => !d.IsRawMaterial && d.IsEnabled && d.PalletNo <= _currentTargetPalletNo)
                                .OrderByDescending(d => d.PalletNo).FirstOrDefault();
                        }

                        if (_autoLogic.CheckIsProperItem(nextPallet.PalletNo, currentRawMaterial) && nextPallet.IsEnabled)
                        {
                            _currentTargetPalletNo = nextPallet.PalletNo;
                            this._plcWorker.Set_EmptyPalletNo(_currentTargetPalletNo);
                            _targetSelectionPalletOk = true;
                        }
                        else
                        {
                            // light up red buzzer
                            OnError?.Invoke("SIRADAKİ YERLEŞTİRİLECEK PALET İÇİN UYGUN BİR HEDEF BULUNAMADI.");
                        }
                    }
                }

                if (_oldTargetPallet != _currentTargetPalletNo)
                    OnActivePalletChanged?.Invoke();
            }
            catch (Exception)
            {

            }
        }

        private bool SendRobotToPlaceDown()
        {
            try
            {
                var rawMatPallet = _palletList.FirstOrDefault(d => d.PalletNo == _currentRawPalletNo && d.IsRawMaterial && d.IsEnabled);

                if (rawMatPallet != null)
                {
                    // try place an item from a raw pallet
                    int placeResult = _autoLogic.PlaceAnItem(_currentTargetPalletNo, rawMatPallet.RawMaterialCode, rawMatPallet.SackType);

                    if (placeResult == 0) // successfull
                    {
                        var posItem = _autoLogic.GetWaitingItem(_currentTargetPalletNo);
                        var currentFloor = _autoLogic.GetCurrentFloor(_currentTargetPalletNo);

                        this._plcWorker.Set_EmptyPalletNo(_currentTargetPalletNo);

                        this._plcWorker.Set_RobotX_ForTarget(posItem.PlacedX - 50);
                        this._plcWorker.Set_RobotY_ForTarget(posItem.PlacedY - 50);
                        this._plcWorker.Set_RobotZ_ForTarget((currentFloor * 9 * 10 * -1) + 80);
                        this._plcWorker.Set_RobotRX_ForTarget(0);
                        this._plcWorker.Set_RobotRY_ForTarget(0);

                        this._plcWorker.Set_RobotRZ_ForTarget(posItem.IsRotated ? -179 : -89);

                        return true;
                    }
                    else // an error occured
                    {
                        OnError?.Invoke("SIRADAKİ YERLEŞTİRME POZİSYONU HESAPLANAMADI. LÜTFEN YENİDEN REÇETE SEÇİNİZ.");
                    }
                }
               
            }
            catch (Exception)
            {

            }

            return false;
        }

        private async Task SwitchCamera(int rawPalletNo)
        {
            if (rawPalletNo == 1)
            {
                this._plcWorker.Set_ServoTargetPos(_plcDb.Servo_PosCam1);
            }
            else if (rawPalletNo == 2)
            {
                this._plcWorker.Set_ServoTargetPos(_plcDb.Servo_PosCam2);
            }

            while (!_plcWorker.Get_ServoIsArrived())
                await Task.Delay(500);
        }

        private bool TriggerCamera(string programId)
        {
            bool result = false;

            bool wrResult = false;

            MechWorker camera = new MechWorker();
            if (camera.TriggerCamera(programId))
            {
                wrResult = this._plcWorker.Set_PlaceCalculationOk(0);
                if (wrResult)
                {
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
                        wrResult = this._plcWorker.Set_RobotRZ(posData[5]);

                        if (wrResult)
                            result = true;
                    }
                    else
                    {
                        _makeSwitchCamera = true;
                        _rawMaterialSelectionPalletOk = false;
                        OnError?.Invoke("KAMERA ÇUVAL TESPİT EDEMEDİ.");
                    }
                }
            }
            else
                OnError?.Invoke("KAMERAYA TETİK VERİLEMEDİ. HABERLEŞME BAĞLANTISINI KONTROL EDİNİZ.");

            return result;
        }

    }
}
