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
            _autoLogic.OnEmptyPalletIsFull += _autoLogic_OnEmptyPalletIsFull;

            _plcWorker = PlcWorker.Instance();
            _palletList = new List<HkAutoPallet>();

            _plcDb = PlcDB.Instance();
            //_plcDb.OnRobotPickedUp += _plcDb_OnRobotPickedUp;
            //_plcDb.OnRobotPlacedDown += _plcDb_OnRobotPlacedDown;
            //_plcDb.OnSystemAutoChanged += _plcDb_OnSystemAutoChanged;

            this.Start();
        }

        private void _autoLogic_OnEmptyPalletIsFull(int palletNo)
        {
            SetPalletDisabled(palletNo);
            OnPalletIsFull?.Invoke(palletNo);
        }

        public void SetPalletDisabled(int palletNo)
        {
            var plt = _palletList.FirstOrDefault(d => d.PalletNo == palletNo);
            if (plt != null)
            {
                plt.IsEnabled = false;
                _autoLogic.ClearPallet(palletNo);
            }
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

        public delegate void PalletIsFull(int palletNo);
        public event PalletIsFull OnPalletIsFull;

        public delegate void PalletIsPlaced(int palletNo);
        public event PalletIsPlaced OnPalletIsPlaced;

        public delegate void RawPalletsAreFinished();
        public event RawPalletsAreFinished OnRawPalletsAreFinished;

        public delegate void CamSentRiskyPos();
        public event CamSentRiskyPos OnCamSentRiskyPos;

        public delegate void PalletSensorStateChanged(int palletNo, bool state);
        public event PalletSensorStateChanged OnPalletSensorChanged;

        public delegate void PalletPlaceLog(int palletNo, bool isPlaced);
        public event PalletPlaceLog OnPalletPlaceLog;

        public delegate void RadarStatus(bool status);
        public event RadarStatus OnRadarStatusChanged;
        #endregion

        // environmental variables
        private HkAutoLogic _autoLogic;
        private PlcDB _plcDb;
        private PlcWorker _plcWorker;
        private IList<HkAutoPallet> _palletList;
        private Task _taskLoop;
        private bool _oldSystemMode = false;
        private bool _oldPendantMode = true;
        private bool _oldEmgMode = false;
        private bool _runLoop;
        private TimeSpan _ellapsedCycle;

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

        bool _oldSnsPlt_1 = false;
        bool _oldSnsPlt_2 = false;
        bool _oldSnsPlt_3 = false;
        bool _oldSnsPlt_4 = false;
        bool _oldSnsPlt_5 = false;
        bool _oldSnsPlt_6 = false;
        bool _oldSnsPlt_7 = false;

        bool _pltSns_1 = false;
        bool _pltSns_2 = false;
        bool _pltSns_3 = false;
        bool _pltSns_4 = false;
        bool _pltSns_5 = false;
        bool _pltSns_6 = false;
        bool _pltSns_7 = false;

        bool _pltLevel_1 = false;
        bool _pltLevel_2 = false;
        bool _pltLevel_3 = false;
        bool _pltLevel_4 = false;
        bool _pltLevel_5 = false;
        bool _pltLevel_6 = false;
        bool _pltLevel_7 = false;

        bool _oldRadarStatus = false;

        public HkAutoPallet GetPalletData(int palletNo)
        {
            //return _palletList.FirstOrDefault(d => d.PalletNo == palletNo);
            return _autoLogic.GetPalletData(palletNo);
        }

        public HkAutoPallet GetPalletDataForStore(int palletNo)
        {
            return _palletList.FirstOrDefault(d => d.PalletNo == palletNo);
        }

        public bool IsPalletLevelFull(int palletNo)
        {
            bool retVal = false;

            switch (palletNo)
            {
                case 1:
                    return _pltLevel_1;
                case 2:
                    return _pltLevel_2;
                case 3:
                    return _pltLevel_3;
                case 4:
                    return _pltLevel_4;
                case 5:
                    return _pltLevel_5;
                case 6:
                    return _pltLevel_6;
                case 7:
                    return _pltLevel_7;
                default:
                    break;
            }

            return retVal;
        }

        public bool IsPalletFull(int palletNo)
        {
            bool retVal = false;

            switch (palletNo)
            {
                case 1:
                    return _pltSns_1;
                case 2:
                    return _pltSns_2;
                case 3:
                    return _pltSns_3;
                case 4:
                    return _pltSns_4;
                case 5:
                    return _pltSns_5;
                case 6:
                    return _pltSns_6;
                case 7:
                    return _pltSns_7;
                default:
                    break;
            }

            return retVal;
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

            if (!plt.IsEnabled)
            {
                plt.Floors = null;
                plt.RawMaterialCode = string.Empty;
                plt.PlaceRecipeCode = string.Empty;
                _autoLogic.ClearPallet(palletNo);
            }
        }

        public void SetPalletAttributes(int palletNo, bool isRawMaterial, bool isEnabled, PlaceRequestDTO pRequest, string rawMaterialCode)
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
            if (!isRawMaterial)
            {
                plt.RawMaterialCode = "";

                if (pRequest != null)
                {
                    _autoLogic.SetRequestForPallet(pRequest, palletNo);
                    requestFound = true;
                }
            }
            else
            {
                plt.PlaceRecipeCode = "";
                plt.RawMaterialCode = rawMaterialCode;

                requestFound = true;
            }

            if (!requestFound)
            {
                plt.IsEnabled = false;
            }

            if (!plt.IsEnabled)
            {
                plt.Floors = null;
                plt.RawMaterialCode = string.Empty;
                plt.PlaceRecipeCode = string.Empty;
                _autoLogic.ClearPallet(palletNo);
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

        public void ResetFlags()
        {
            _currentRawPalletNo = 1;
            _currentTargetPalletNo = 7;
            _rawMaterialSelectionPalletOk = false;
            _targetSelectionPalletOk = false;
            _captureOk = false;
            _robotTargetCoordsReady = false;
            _robotSentToPlaceDown = false;
            _robotPickedUp = false;
        }

        // main logic loop
        private async Task LoopFunc()
        {
            bool wrResult = false;

            while (_runLoop)
            {
                try
                {
                    // handle emergency
                    var emgExists = _plcWorker.Get_PlcEmergency();
                    if (!emgExists && _oldEmgMode)
                        OnError?.Invoke("");
                    if (emgExists && !_oldEmgMode)
                    {
                        _plcWorker.Set_SystemAuto(0);
                        _plcWorker.Set_RobotHold(1);
                        OnError?.Invoke("SİSTEM ACİL DURUMUNA GEÇTİ.");

                        if (_robotSentToPlaceDown)
                        {
                            OnPalletPlaceLog?.Invoke(_currentTargetPalletNo, false);
                        }
                    }
                    _oldEmgMode = emgExists;

                    #region check someone inside
                    var someoneInside = _plcWorker.Get_PC_SomeoneInside();
                    if (someoneInside)
                    {
                        _plcWorker.Set_SystemAuto(0);
                        _plcWorker.Set_RobotHold(1);
                        OnError.Invoke("İÇERİDE HAREKET TESPİT EDİLDİĞİNDEN SİSTEM BAŞLATILMADI.");

                        _plcWorker.Set_PC_SomeoneInside(0);
                    }
                    #endregion

                    // handle system working mode
                    var systemAuto = _plcWorker.Get_SystemAuto();
                    _plcDb.System_Auto = systemAuto;
                    if (_oldSystemMode != systemAuto)
                        OnSystemModeChanged?.Invoke(systemAuto);
                    _oldSystemMode = systemAuto;

                    // handle robot pendant mode
                    var pendantRemoteMode = _plcWorker.Get_RobotRemoteMode();
                    if (pendantRemoteMode != _oldPendantMode && pendantRemoteMode)
                        OnPalletIsPlaced?.Invoke(1);
                    else if (pendantRemoteMode != _oldPendantMode && !pendantRemoteMode)
                        OnError.Invoke("ROBOT KUMANDASINI REMOTE ÇALIŞMA MODUNA ALINIZ.");
                    _oldPendantMode = pendantRemoteMode;

                    #region check pallet sensors
                    if (systemAuto && !emgExists)
                    {
                        // pallet-2
                        var pltState = _plcWorker.Get_PltSns_2();
                        _pltSns_2 = pltState;
                        if (!_oldSnsPlt_2 && pltState)
                            OnPalletSensorChanged.Invoke(2, true);
                        else if (_oldSnsPlt_2 && !pltState)
                            OnPalletSensorChanged.Invoke(2, false);
                        _oldSnsPlt_2 = pltState;
                        _pltLevel_2 = _plcWorker.Get_PltLevelSns_2();

                        // pallet-3
                        pltState = _plcWorker.Get_PltSns_3();
                        _pltSns_3 = pltState;
                        if (!_oldSnsPlt_3 && pltState)
                            OnPalletSensorChanged.Invoke(3, true);
                        else if (_oldSnsPlt_3 && !pltState)
                            OnPalletSensorChanged.Invoke(3, false);
                        _oldSnsPlt_3 = pltState;
                        _pltLevel_3 = _plcWorker.Get_PltLevelSns_3();

                        // pallet-4
                        pltState = _plcWorker.Get_PltSns_4();
                        _pltSns_4 = pltState;
                        if (!_oldSnsPlt_4 && pltState)
                            OnPalletSensorChanged.Invoke(4, true);
                        else if (_oldSnsPlt_4 && !pltState)
                            OnPalletSensorChanged.Invoke(4, false);
                        _oldSnsPlt_4 = pltState;
                        _pltLevel_4 = _plcWorker.Get_PltLevelSns_4();

                        // pallet-5
                        pltState = _plcWorker.Get_PltSns_5();
                        _pltSns_5 = pltState;
                        if (!_oldSnsPlt_5 && pltState)
                            OnPalletSensorChanged.Invoke(5, true);
                        else if (_oldSnsPlt_5 && !pltState)
                            OnPalletSensorChanged.Invoke(5, false);
                        _oldSnsPlt_5 = pltState;
                        _pltLevel_5 = _plcWorker.Get_PltLevelSns_5();

                        // pallet-6
                        pltState = _plcWorker.Get_PltSns_6();
                        _pltSns_6 = pltState;
                        if (!_oldSnsPlt_6 && pltState)
                            OnPalletSensorChanged.Invoke(6, true);
                        else if (_oldSnsPlt_6 && !pltState)
                            OnPalletSensorChanged.Invoke(6, false);
                        _oldSnsPlt_6 = pltState;
                        _pltLevel_6 = _plcWorker.Get_PltLevelSns_6();

                        // pallet-7
                        pltState = _plcWorker.Get_PltSns_7();
                        _pltSns_7 = pltState;
                        if (!_oldSnsPlt_7 && pltState)
                            OnPalletSensorChanged.Invoke(7, true);
                        else if (_oldSnsPlt_7 && !pltState)
                            OnPalletSensorChanged.Invoke(7, false);
                        _oldSnsPlt_7 = pltState;
                        _pltLevel_7 = _plcWorker.Get_PltLevelSns_7();
                    }
                    #endregion

                    #region update radar sensor status
                    var radarStatus = _plcWorker.Get_PC_RadarStatus();
                    if (radarStatus != _oldRadarStatus)
                    {
                        OnRadarStatusChanged?.Invoke(radarStatus);
                    }
                    _oldRadarStatus = radarStatus;
                    #endregion

                    if (systemAuto)
                    {
                        _ellapsedCycle = DateTime.Now.TimeOfDay;
                        await PrepareRawMaterial();

                        // select next pallet to place
                        if (_currentTargetPalletNo <= 0 || _currentTargetPalletNo > 7)
                        {
                            _currentTargetPalletNo = 7;
                            _targetSelectionPalletOk = false;
                        }

                        if (!_targetSelectionPalletOk)
                            CheckNextTargetPallet();
                        
                        // son çuvalın dağıtımında robotun durmasını ve hata reseti engellediği için kaldırıldı
                        //if (!_targetSelectionPalletOk)
                        //{
                        //    _plcWorker.Set_SystemAuto(0);
                        //    _plcWorker.Set_RobotHold(1);
                        //    OnError?.Invoke("DAĞITILACAK YENİ HAMMADDEYİ BELİRLEYİN.");
                        //}

                        if (_targetSelectionPalletOk)
                        {
                            if (!_robotTargetCoordsReady)
                            {
                                var sendResult = SendRobotToPlaceDown();
                                if (sendResult)
                                    _robotTargetCoordsReady = true;
                            }

                            var isRiskyPos = _plcWorker.Get_RobotRiskyPos();
                            if (isRiskyPos)
                            {
                                _plcWorker.Set_RobotHold(1);
                                _plcWorker.Set_RobotRiskyPos(0);
                                OnCamSentRiskyPos?.Invoke();
                                OnError?.Invoke("KAMERA RİSKLİ BİR POZİSYON GÖNDERDİ.");
                            }

                            var robotPickingOk = _plcWorker.Get_RobotPickingOk();
                            if (robotPickingOk && _robotTargetCoordsReady && _captureOk && !_robotSentToPlaceDown)
                            {
                                wrResult = _plcWorker.Set_RobotNextTargetOk(1);
                                if (wrResult)
                                {
                                    var zeroResult = _plcWorker.Set_CaptureOk(0);
                                    if (zeroResult)
                                    {
                                        wrResult = this._plcWorker.Set_PlaceCalculationOk(1);
                                        if (wrResult)
                                        {
                                            _robotSentToPlaceDown = true;
                                        }

                                        //zeroResult = _plcWorker.Set_RobotPickingOk(0);
                                        //if (zeroResult)
                                        //{
                                        //    wrResult = this._plcWorker.Set_PlaceCalculationOk(1);
                                        //    if (wrResult)
                                        //    {
                                        //        _robotSentToPlaceDown = true;
                                        //    }
                                        //}
                                    }
                                }
                            }
                        }

                        // wait for placed down signal from robot and then reset all flags to // pick & place // again
                        var robotPlacingOk = _plcWorker.Get_RobotPlacingOk();
                        if (_robotSentToPlaceDown && robotPlacingOk)
                        {
                            wrResult = _plcWorker.Set_RobotPlacingOk(0);
                            if (wrResult)
                            {
                                wrResult = _plcWorker.Set_RobotPickingOk(0);
                                if (wrResult)
                                {
                                    var placingLogicOk = _autoLogic.SignWaitingPlacementIsMade(_currentTargetPalletNo);
                                    if (placingLogicOk)
                                    {
                                        OnPalletIsPlaced?.Invoke(_currentTargetPalletNo);
                                        OnPalletPlaceLog?.Invoke(_currentTargetPalletNo, true);
                                    }

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
                            }
                        }

                        var passedTime = DateTime.Now.TimeOfDay - _ellapsedCycle;
                        _autoLogic.AddEllapsedTime(_currentTargetPalletNo, passedTime);
                        _autoLogic.AddEllapsedTime(_currentTargetPalletNo, TimeSpan.FromMilliseconds(200));
                    }
                }
                catch (Exception)
                {

                }

                await Task.Delay(200);
            }
        }

        private async Task PrepareRawMaterial()
        {
            bool wrResult = false;

            try
            {
                if (!_palletList.Any(d => d.IsRawMaterial && d.IsEnabled))
                {
                    _currentRawPalletNo = 1;
                    _rawMaterialSelectionPalletOk = false;
                    _captureOk = false;
                }

                // select proper raw material pallet
                if (!_rawMaterialSelectionPalletOk)
                    await CheckRawPallets();

                // trigger camera and pick up
                if (_rawMaterialSelectionPalletOk && !_captureOk)
                {
                    _plcWorker.Set_RawPalletNo(_currentRawPalletNo);

                    var trgOk = TriggerCamera(_currentRawPalletNo.ToString());
                    if (trgOk)
                    {
                        wrResult = this._plcWorker.Set_CaptureOk(1);
                        if (wrResult)
                            _captureOk = true;
                    }
                }
            }
            catch (Exception)
            {

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
                {
                    OnRawPalletsAreFinished?.Invoke();
                    OnError?.Invoke("KAMERA ÇUVAL TESPİT EDEMEDİ.");
                }
            }
            catch (Exception)
            {

            }
        }

        private void CheckNextTargetPallet()
        {
            _targetSelectionPalletOk = false;

            

            try
            {
                var _oldTargetPallet = _currentTargetPalletNo;

                if (_currentTargetPalletNo <= 0)
                    _currentTargetPalletNo = 7;

                var currentRawPallet = _palletList.FirstOrDefault(d => d.PalletNo == _currentRawPalletNo);
                var currentRawMaterial = currentRawPallet != null ? currentRawPallet.RawMaterialCode : "";

                if (!string.IsNullOrEmpty(currentRawMaterial))
                {
                    var nextPallet = _palletList
                            .Where(d => !d.IsRawMaterial && d.IsEnabled && d.PalletNo <= _currentTargetPalletNo)
                            .OrderByDescending(d => d.PalletNo).FirstOrDefault();

                    while (nextPallet == null || !_autoLogic.CheckIsProperItem(nextPallet.PalletNo, currentRawMaterial) || !nextPallet.IsEnabled)
                    {
                        _currentTargetPalletNo--;

                        if (_currentTargetPalletNo <= 0)
                        {
                            _currentTargetPalletNo = 7;
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
                        this._plcWorker.Set_RobotZ_ForTarget((currentFloor * 11 * 10 * -1) + 80);
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
            try
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
            catch (Exception)
            {

            }
           
        }

        private bool TriggerCamera(string programId)
        {
            bool result = false;

            bool wrResult = false;

            try
            {
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
            }
            catch (Exception)
            {

            }

           

            return result;
        }

    }
}
