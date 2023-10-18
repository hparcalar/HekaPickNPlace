using PickNPlace.Plc;
using PickNPlace.Plc.Data;
using PickNPlace.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PickNPlace.DataAccess;
using PickNPlace.Business;
using System.Timers;
using JotunWS;
using System.Data;

namespace PickNPlace
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // local variables
        private bool _isStarted = false;
        private bool _placeByRecipe = false;
        private PlcWorker _plc;
        private PlcDB _plcDB;
        private Task _flagListener;
        private bool _runFlagListener = false;
        private HkAutoPallet[] _palletList;
        private HkLogicWorker _logicWorker;
        private PlaceRequestDTO _activeRecipe = new PlaceRequestDTO();
        private PlaceRequestDTO _manualRecipe = new PlaceRequestDTO();
        private Timer _tmrError = new Timer(10000);
        private Timer _tmrResetInvoker = new Timer(750);
        private Timer _tmrStateUpdater = new Timer(1000);

        public MainWindow()
        {
            InitializeComponent();

            // make db migrations
            SchemaFactory.ApplyMigrations();

            _tmrResetInvoker.AutoReset = false;
            _tmrResetInvoker.Elapsed += _tmrResetInvoker_Elapsed;

            _tmrError.AutoReset = false;
            _tmrError.Elapsed += _tmrError_Elapsed;

            _tmrStateUpdater.AutoReset = true;
            _tmrStateUpdater.Elapsed += _tmrStateUpdater_Elapsed;
        }

        private void _tmrStateUpdater_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke((Action)delegate
            {
                this.BindLivePalletStates();
            });
        }

        private void _logicWorker_OnPalletIsFull(int palletNo)
        {
            this.Dispatcher.Invoke((Action)delegate
            {
                this.BindLivePalletStates();
            });
        }

        private void _logicWorker_OnSystemModeChanged(bool mode)
        {
            this.Dispatcher.Invoke((Action)delegate
            {
                _plcDB.System_Auto = mode;
                this.UpdateSystemStatus();
            });
        }

        private void _tmrError_Elapsed(object sender, ElapsedEventArgs e)
        {
           
        }

        private void _logicWorker_OnError(string message)
        {
            this.Dispatcher.Invoke((Action)delegate
            {
                lblError.Content = message;

                //_tmrError.Stop();

                //_tmrError.Enabled = true;
                //_tmrError.Start();
            });
        }

        private void _logicWorker_OnActivePalletChanged()
        {
            this.Dispatcher.Invoke((Action)delegate
            {
                this.BindLivePalletStates();
            });
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(InitLogicWorker);

            //_runFlagListener = true;
            //_flagListener = Task.Run(this.LoopFlagListen);
        }

        private void InitLogicWorker()
        {
            // init plc communication
            this._plc = PlcWorker.Instance();
            this._plc.OnPlcConnectionChanged += _plc_OnPlcConnectionChanged;
            this._plc.Start();

            // prepare datablocks
            this._plcDB = PlcDB.Instance();

            // init logic worker
            _logicWorker = HkLogicWorker.GetInstance();
            _logicWorker.OnActivePalletChanged += _logicWorker_OnActivePalletChanged;
            _logicWorker.OnError += _logicWorker_OnError;
            _logicWorker.OnSystemModeChanged += _logicWorker_OnSystemModeChanged;
            _logicWorker.OnPalletIsFull += _logicWorker_OnPalletIsFull;
            _logicWorker.OnPalletIsPlaced += _logicWorker_OnPalletIsPlaced;
            _logicWorker.OnRawPalletsAreFinished += _logicWorker_OnRawPalletsAreFinished;
            _logicWorker.OnCamSentRiskyPos += _logicWorker_OnCamSentRiskyPos;

            _tmrStateUpdater.Enabled = true;

            this.Dispatcher.Invoke((Action)delegate
            {
                this.CreateInitialData();
                //this.BindLivePalletStates();
            });
        }

        private void _logicWorker_OnCamSentRiskyPos()
        {
            this.Dispatcher.Invoke((Action)delegate
            {
                _plc.Set_SystemAuto(0);
                _plc.Set_Reset_Plc_Variables(1);
                _logicWorker.ResetFlags();
            });
        }

        private void _logicWorker_OnRawPalletsAreFinished()
        {
            this.Dispatcher.Invoke((Action)delegate
            {
                this._plc.Set_SystemAuto(0);
                this.BindLivePalletStates();
            });
        }

        private void _logicWorker_OnPalletIsPlaced(int palletNo)
        {
            this.Dispatcher.Invoke((Action)delegate
            {
                this.BindLivePalletStates();
                lblError.Content = "";
            });
        }

        private void CreateInitialData()
        {
            using (HekaDbContext db = SchemaFactory.CreateContext())
            {
                if (!db.PalletStateLive.Any())
                {
                    // create 6 pallet records
                    for (int i = 1; i <= 6; i++)
                    {
                        var dbPallet = new PalletStateLive
                        {
                            PalletNo = i,
                            IsPickable = (i == 1 || i == 2),
                            IsDropable = (i != 1 && i != 2),
                        };

                        db.PalletStateLive.Add(dbPallet);
                    }

                    db.SaveChanges();
                }
            }

            using (HekaDbContext db = SchemaFactory.CreateContext())
            {
                if (!db.PalletStateLive.Where(d => d.PalletNo == 7).Any())
                {
                    var dbPallet = new PalletStateLive
                    {
                        PalletNo = 7,
                        IsPickable = false,
                        IsDropable = true
                    };

                    db.PalletStateLive.Add(dbPallet);

                    db.SaveChanges();
                }
            }
        }

        private void BindLivePalletStates()
        {
            // get instant pallet data from database
            //using (HekaDbContext db = SchemaFactory.CreateContext())
            //{
            //    _palletList = db.PalletStateLive.Select(d => new PalletStateDTO
            //    {
            //        Id = d.Id,
            //        CompletedBatchCount = d.CompletedBatchCount,
            //        CurrentBatchIsCompleted = d.CurrentBatchIsCompleted,
            //        CurrentBatchIsStarted = d.CurrentBatchIsStarted,
            //        CurrentBatchNo = d.CurrentBatchNo,
            //        CurrentItemIsCompleted = d.CurrentItemIsCompleted,
            //        CurrentItemIsStarted = d.CurrentItemIsStarted,
            //        CurrentItemNo = d.CurrentItemNo,
            //        IsDropable = d.IsDropable,
            //        IsPickable = d.IsPickable,
            //        PalletNo = d.PalletNo,
            //        PalletRecipeId = d.PalletRecipeId,
            //        PlaceRequestId = d.PlaceRequestId,
            //    }).ToArray();
            //}

            try
            {
                if (_logicWorker != null)
                {
                    if (_palletList == null || _palletList.Length == 0)
                    {
                        _logicWorker.SetPalletAttributes(1, true, false, "");
                        _logicWorker.SetPalletAttributes(2, true, false, "");
                        _logicWorker.SetPalletAttributes(3, false, false, "");
                        _logicWorker.SetPalletAttributes(4, false, false, "");
                        _logicWorker.SetPalletAttributes(5, false, false, "");
                        _logicWorker.SetPalletAttributes(6, false, false, "");
                        _logicWorker.SetPalletAttributes(7, false, false, "");
                    }

                    _palletList = _logicWorker.GetPalletList().ToArray();
                }

                // update pallet visuals
                if (_palletList != null)
                {
                    foreach (var item in _palletList)
                    {
                        if (item.PalletNo == 1)
                        {
                            plt1.PickingText = item.IsRawMaterial == true ? "HAMMADDE" : "BOŞ PALET";
                            plt1.PickingColor = item.IsRawMaterial == true ? "#FFF2FBBB" : "#FF93C1F0";
                            plt1.IsPalletEnabled = item.IsEnabled;
                            plt1.RecipeName = item.IsRawMaterial ? item.RawMaterialCode : item.PlaceRecipeCode;
                            plt1.SackType = item.IsEnabled ? (item.SackType == 1 ? "40x60" : item.SackType == 2 ? "30x50" : item.SackType == 3 ? "50x70" : "") : "";
                            plt1.IsActivePallet = _plcDB.System_Auto && _logicWorker.CurrentRawPalletNo == 1;
                        }
                        else if (item.PalletNo == 2)
                        {
                            plt2.PickingText = item.IsRawMaterial == true ? "HAMMADDE" : "BOŞ PALET";
                            plt2.PickingColor = item.IsRawMaterial == true ? "#FFF2FBBB" : "#FF93C1F0";
                            plt2.IsPalletEnabled = item.IsEnabled;
                            plt2.RecipeName = item.IsRawMaterial ? item.RawMaterialCode : item.PlaceRecipeCode;
                            plt2.SackType = item.IsEnabled ? (item.SackType == 1 ? "40x60" : item.SackType == 2 ? "30x50" : item.SackType == 3 ? "50x70" : "") : "";
                            plt2.IsActivePallet = _plcDB.System_Auto && _logicWorker.CurrentRawPalletNo == 2;

                            var palletData2 = _logicWorker.GetPalletData(item.PalletNo);
                            plt2.EllapsedTime = palletData2 != null ? palletData2.EllapsedTime : null;
                            plt2.Pallet = palletData2;
                            plt2.BindState();
                        }
                        else if (item.PalletNo == 3)
                        {
                            plt3.IsPalletEnabled = item.IsEnabled;
                            plt3.IsActivePallet = _plcDB.System_Auto && _logicWorker.CurrentTargetPalletNo == 3;
                            plt3.Pallet = _logicWorker.GetPalletData(item.PalletNo);
                            plt3.BindState();
                            plt3.EllapsedTime = plt3.Pallet != null ? plt3.Pallet.EllapsedTime : null;
                        }
                        else if (item.PalletNo == 4)
                        {
                            plt4.IsPalletEnabled = item.IsEnabled;
                            plt4.IsActivePallet = _plcDB.System_Auto && _logicWorker.CurrentTargetPalletNo == 4;
                            plt4.Pallet = _logicWorker.GetPalletData(item.PalletNo);
                            plt4.BindState();
                            plt4.EllapsedTime = plt4.Pallet != null ? plt4.Pallet.EllapsedTime : null;
                        }
                        else if (item.PalletNo == 5)
                        {
                            plt5.IsPalletEnabled = item.IsEnabled;
                            plt5.IsActivePallet = _plcDB.System_Auto && _logicWorker.CurrentTargetPalletNo == 5;
                            plt5.Pallet = _logicWorker.GetPalletData(item.PalletNo);
                            plt5.BindState();
                            plt5.EllapsedTime = plt5.Pallet != null ? plt5.Pallet.EllapsedTime : null;
                        }
                        else if (item.PalletNo == 6)
                        {
                            plt6.IsPalletEnabled = item.IsEnabled;
                            plt6.IsActivePallet = _plcDB.System_Auto && _logicWorker.CurrentTargetPalletNo == 6;
                            plt6.Pallet = _logicWorker.GetPalletData(item.PalletNo);
                            plt6.BindState();
                            plt6.EllapsedTime = plt6.Pallet != null ? plt6.Pallet.EllapsedTime : null;
                        }
                        else if (item.PalletNo == 7)
                        {
                            plt7.IsPalletEnabled = item.IsEnabled;
                            plt7.IsActivePallet = _plcDB.System_Auto && _logicWorker.CurrentTargetPalletNo == 7;
                            plt7.Pallet = _logicWorker.GetPalletData(item.PalletNo);
                            plt7.BindState();
                            plt7.EllapsedTime = plt7.Pallet != null ? plt7.Pallet.EllapsedTime : null;
                        }
                    }
                }

            }
            catch (Exception)
            {

            }

        }

        private void plt_OnPickingStatusChanged(int palletNo)
        {
            this.Dispatcher.Invoke((Action)delegate
            {
                if (_palletList != null)
                {
                    var pallet = _palletList.FirstOrDefault(d => d.PalletNo == palletNo);
                    if (pallet != null)
                    {
                        if (pallet.PalletNo == 1 && pallet.IsRawMaterial)
                        {
                            MessageBox.Show("1. Palet hammadde olmak zorundadır.", "Uyarı", MessageBoxButton.OK);
                            return;
                        }

                        pallet.IsRawMaterial = !pallet.IsRawMaterial;
                        _logicWorker.SetPalletAttributes(palletNo, pallet.IsRawMaterial, pallet.IsEnabled, pallet.PlaceRecipeCode);

                        this.BindLivePalletStates();
                    }
                }
            });
        }

        private void plt_OnPalletEnabledChanged(int palletNo, bool enabled)
        {
            this.Dispatcher.Invoke((Action)delegate
            {
                if (_palletList != null)
                {
                    var pallet = _palletList.FirstOrDefault(d => d.PalletNo == palletNo);
                    if (pallet != null)
                    {
                        pallet.IsEnabled = !pallet.IsEnabled;

                        if (_placeByRecipe)
                        {
                            // auto assign active recipe to lately enabled pallet
                            if (_activeRecipe != null && !pallet.IsRawMaterial && pallet.IsEnabled)
                            {
                                pallet.PlaceRecipeCode = _activeRecipe.RequestNo;
                            }
                            else if (!pallet.IsEnabled && !pallet.IsRawMaterial)
                            {
                                pallet.PlaceRecipeCode = string.Empty;
                            }

                            _logicWorker.SetPalletAttributes(palletNo, pallet.IsRawMaterial, pallet.IsEnabled, pallet.PlaceRecipeCode);
                        }
                        else
                        {
                            if (_manualRecipe != null && !pallet.IsRawMaterial && pallet.IsEnabled)
                            {
                                _logicWorker.SetPalletAttributes(palletNo, pallet.IsRawMaterial, pallet.IsEnabled, _manualRecipe, string.Empty);
                            }
                            else if (!pallet.IsEnabled && !pallet.IsRawMaterial)
                            {
                                _logicWorker.SetPalletAttributes(palletNo, pallet.IsRawMaterial, false, null, string.Empty);
                            }
                            //else if (!pallet.IsEnabled && pallet.IsRawMaterial)
                            //{
                            //    _logicWorker.SetPalletAttributes(palletNo, pallet.IsRawMaterial, false, null, string.Empty);
                            //}
                        }

                        this.BindLivePalletStates();
                    }
                }
            });
        }

        private void plt_OnSelectRecipeSignal(int palletNo)
        {
            bool onlineEditing = false;
            var cPlt = _palletList.FirstOrDefault(d => d.PalletNo == palletNo);
            if (cPlt != null)
            {
                if (!cPlt.IsRawMaterial)
                {
                    onlineEditing = true;

                    var palletData = _logicWorker.GetPalletData(palletNo);
                    var reqData = _logicWorker.GetPalletRequest(palletNo);

                    OnlinePalletEdit wnd = new OnlinePalletEdit();
                    wnd.Pallet = palletData;
                    wnd.PlaceRequest = reqData;
                    wnd.ShowDialog();

                    this.BindLivePalletStates();
                }
            }

            if (onlineEditing)
                return;

            if (_activeRecipe != null && !string.IsNullOrEmpty(_activeRecipe.RequestNo)) // material selection with recipe barcode
            {
                if(_palletList != null)
                {
                    var plt = _palletList.FirstOrDefault(d => d.PalletNo == palletNo);
                    if (plt != null)
                    {
                        if (plt.IsRawMaterial)
                        {
                            MaterialList wnd = new MaterialList();
                            wnd.ItemCode = plt.RawMaterialCode;
                            wnd.SackType = plt.SackType;
                            wnd.RequestNo = _activeRecipe.RequestNo;
                            wnd.ShowDialog();

                            if (!string.IsNullOrEmpty(wnd.ItemCode) && wnd.SackType > 0)
                            {
                                plt.SackType = wnd.SackType;
                                plt.RawMaterialCode = wnd.ItemCode;

                                _logicWorker.SetPalletAttributes(palletNo, true, true, plt.RawMaterialCode);
                                _logicWorker.SetPalletSackType(palletNo, plt.SackType);

                                this.Dispatcher.Invoke((Action)delegate
                                {
                                    this.BindLivePalletStates();
                                });
                            }
                        }
                    }
                }
            }
            else // manual placement work order
            {
                if (_palletList != null)
                {
                    var rawPallets = _palletList.Where(d => d.IsRawMaterial).ToArray();

                    if (rawPallets == null || rawPallets.Length == 0)
                    {
                        MessageBox.Show("En az 1 adet hammadde paleti seçmelisiniz.", "Uyarı", MessageBoxButton.OK);
                        return;
                    }

                    ManualWorkOrder wnd = new ManualWorkOrder();
                    wnd.RawPallets = rawPallets;
                    wnd.WorkOrder = _manualRecipe;

                    wnd.ShowDialog();

                    if (wnd.SelectionOk)
                    {
                        if (_logicWorker != null)
                        {
                            _manualRecipe = wnd.WorkOrder;

                            // set raw material pallets at business logic
                            foreach (var rplt in wnd.RawPallets)
                            {
                                _logicWorker.SetPalletAttributes(rplt.PalletNo, true, true, wnd.WorkOrder, rplt.RawMaterialCode);
                                _logicWorker.SetPalletSackType(rplt.PalletNo, rplt.SackType);
                            }

                            // set manual recipe of active pallets
                            var activePallets = _palletList.Where(d => !d.IsRawMaterial).ToArray();
                            foreach (var plt in activePallets)
                            {
                                _logicWorker.SetPalletAttributes(plt.PalletNo, false, true, _manualRecipe, plt.RawMaterialCode);                                
                            }

                            this.BindLivePalletStates();
                            this.BindLivePalletStates();
                        }
                    }
                }
            }
        }

        private void _plc_OnPlcConnectionChanged(bool connected)
        {
            this.Dispatcher.BeginInvoke((Action)delegate
            {
                imgPlcOk.Source = new BitmapImage(new Uri(connected ? "Images/green_circle.png" : "Images/red_circle.png", UriKind.Relative));
                imgRobotOk.Source = new BitmapImage(new Uri(connected ? "Images/green_circle.png" : "Images/red_circle.png", UriKind.Relative));
            });
        }

        private void btnManagement_Click(object sender, RoutedEventArgs e)
        {
            ManagementWindow wnd = new ManagementWindow();
            wnd.Show();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _runFlagListener = false;

            if (_plc != null)
                this._plc.Stop();

            if (_logicWorker != null)
                this._logicWorker.Dispose();

            try
            {
                if (_flagListener != null)
                    this._flagListener.Dispose();
            }
            catch (Exception)
            {

            }
        }

        private void btnStartToggle_Click(object sender, RoutedEventArgs e)
        {
            //this.BindDefaults();

            var targetInfo = !_plcDB.System_Auto;

            if (targetInfo)
            {
                var rawPallets = _palletList.Any(d => d.IsRawMaterial && d.IsEnabled);
                if (!rawPallets)
                {
                    MessageBox.Show("Sistemi başlatabilmek için en az 1 hammadde paleti seçmelisiniz.", "Uyarı", MessageBoxButton.OK);
                    return;
                }
            }
            else
            {
                this._plc.Set_RobotHold(1);
            }

            this._plc.Set_SystemAuto((byte)(targetInfo ? 1 : 0));
            if (targetInfo)
            {
                _plc.Set_PlcEmgReset(1);
                _plc.Set_PlcEmergency(0);
                this._plc.Set_RobotHold(0);
                this._plc.Set_Robot_Start(0);
                this._plc.Set_Robot_Start(1);
            }

            //this.Dispatcher.Invoke((Action)delegate
            //{
            //    this.UpdateSystemStatus();
            //});
        }

        private void BindDefaults()
        {
            //using (HekaDbContext db = SchemaFactory.CreateContext())
            //{
            //    int tryValue = 0;

            //    var prmData = db.SysParam.FirstOrDefault(d => d.ParamCode == "ServoSpeed");
            //    if (prmData != null && Int32.TryParse(prmData.ParamValue, out tryValue))
            //    {
            //        this._plc.Set_ServoSpeed(Convert.ToInt32(prmData.ParamValue));
            //    }

            //    prmData = db.SysParam.FirstOrDefault(d => d.ParamCode == "ServoPosCam1");
            //    if (prmData != null && Int32.TryParse(prmData.ParamValue, out tryValue))
            //    {
            //        this._plc.Set_ServoPosCam1(Convert.ToInt32(prmData.ParamValue));
            //    }

            //    prmData = db.SysParam.FirstOrDefault(d => d.ParamCode == "ServoPosCam2");
            //    if (prmData != null && Int32.TryParse(prmData.ParamValue, out tryValue))
            //    {
            //        this._plc.Set_ServoPosCam2(Convert.ToInt32(prmData.ParamValue));
            //    }

            //    this._plc.Set_ServoStart(1);
            //}
        }

        private void UpdateSystemStatus()
        {
            if (_plcDB.System_Auto)
            {
                txtStart.Text = "SİSTEMİ DURAKLAT";
                imgStart.Source = new BitmapImage(new Uri("Images/stop.png", UriKind.Relative));
            }
            else
            {
                txtStart.Text = "SİSTEMİ BAŞLAT";
                imgStart.Source = new BitmapImage(new Uri("Images/start.png", UriKind.Relative));
            }
        }

        #region CONTINUOUS FLAG LISTENING
        private async Task LoopFlagListen()
        {
            while (_runFlagListener)
            {
                try
                {
                    await this.Dispatcher.BeginInvoke((Action)delegate
                    {
                        this.UpdateSystemStatus();
                    });
                }
                catch (Exception)
                {

                }

                await Task.Delay(250);
            }
        }

        #endregion

        private void btnShowPalletRecipes_Click(object sender, RoutedEventArgs e)
        {
            PalletRecipeWindow wnd = new PalletRecipeWindow();
            wnd.ShowDialog();
        }

        private void txtRecipeBarocde_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!string.IsNullOrEmpty(txtRecipeBarocde.Text))
                {
                    using (HekaDbContext db = SchemaFactory.CreateContext())
                    {
                        var dbRecipe = db.PlaceRequest.FirstOrDefault(d => d.RequestNo == txtRecipeBarocde.Text);
                        if (dbRecipe == null)
                        {
                            // try fetch data from web service
                            try
                            {
                                YASKAWA_RobotSoapClient wsClient = new YASKAWA_RobotSoapClient(YASKAWA_RobotSoapClient.EndpointConfiguration.YASKAWA_RobotSoap);
                                wsClient.OpenAsync().Wait();
                                var result = wsClient.GET_REQUEST_DETAILAsync("55555", txtRecipeBarocde.Text).Result;

                                if (result != null)
                                {
                                    //System.IO.Stream xmlStream = new System.IO.MemoryStream();
                                    var xmlStg = new System.Xml.XmlWriterSettings();
                                    xmlStg.ConformanceLevel = System.Xml.ConformanceLevel.Auto;
                                    //var xmlWriter = System.Xml.XmlWriter.Create(xmlStream, xmlStg);
                                    //result.WriteXml(xmlWriter);
                                    //xmlWriter.Flush();
                                    //xmlWriter.Close();

                                    var xmlFileWriter = System.Xml.XmlWriter.Create("test_int.xml", xmlStg);
                                    result.WriteXml(xmlFileWriter);
                                    xmlFileWriter.Flush();
                                    xmlFileWriter.Close();
                                }

                                wsClient.Close();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                            }

                            // try import from xml dataset
                            try
                            {
                                DataSet ds = new DataSet();
                                ds.ReadXml("test_int.xml", XmlReadMode.Fragment);

                                if (ds.Tables.Count > 0)
                                {
                                    dbRecipe = new PlaceRequest
                                    {
                                        BatchCount = 0,
                                        RequestNo = txtRecipeBarocde.Text,
                                    };
                                    db.PlaceRequest.Add(dbRecipe);

                                    List<string> itemCodes = new List<string>();
                                    List<RawMaterial> addedMats = new List<RawMaterial>();

                                    bool breakUpperLoop = false;

                                    foreach (DataTable table in ds.Tables)
                                    {
                                        if (breakUpperLoop)
                                            break;

                                        foreach (DataRow row in table.Rows)
                                        {
                                            string imCode = (string)row[4];

                                            if (itemCodes.Contains(imCode))
                                            {
                                                breakUpperLoop = true;
                                                break;
                                            }

                                            itemCodes.Add(imCode);

                                            // set recipe header information
                                            dbRecipe.RecipeCode = (string)row[2];
                                            dbRecipe.RecipeName = (string)row[3];

                                            // set raw material record
                                            var dbItem = db.RawMaterial.FirstOrDefault(d => d.ItemCode == imCode);
                                            if (dbItem == null)
                                            {
                                                dbItem = addedMats.FirstOrDefault(d => d.ItemCode == imCode);
                                            }

                                            if (dbItem == null)
                                            {
                                                dbItem = new RawMaterial
                                                {
                                                    ItemCode = imCode,
                                                    ItemName = (string)row[5],
                                                    ItemNetWeight = 0,
                                                };
                                                db.RawMaterial.Add(dbItem);
                                                addedMats.Add(dbItem);
                                            }

                                            // set recipe detail item
                                            var dbRecItem = new PlaceRequestItem
                                            {
                                                PiecesPerBatch = Convert.ToInt32(row[6]),
                                                PlaceRequest = dbRecipe,
                                                RawMaterial = dbItem,
                                            };
                                            db.PlaceRequestItem.Add(dbRecItem);
                                        }
                                    }

                                    db.SaveChanges();
                                }
                                else
                                    MessageBox.Show("Sonuç bulunamadı. Tables=0");
                            }
                            catch (Exception ex)
                            {

                            }

                            // delete the last imported data file
                            try
                            {
                                System.IO.File.Delete("test_int.xml");
                            }
                            catch (Exception)
                            {

                            }
                        }

                        if (dbRecipe != null)
                        {
                            foreach (var plt in _palletList)
                            {
                                if (!plt.IsRawMaterial)
                                {
                                    plt.PlaceRecipeCode = dbRecipe.RecipeCode;
                                    _logicWorker.SetPalletAttributes(plt.PalletNo, false, true, dbRecipe.RequestNo);
                                }
                            }

                            _activeRecipe.RequestNo = txtRecipeBarocde.Text;
                            _activeRecipe.RecipeName = dbRecipe.RecipeName;

                            txtRecipeBarocde.Text = "";

                            txtActiveRecipeCode.Content = _activeRecipe.RequestNo;
                            txtActiveRecipeName.Content = _activeRecipe.RecipeName;
                        }
                        else
                        {
                            foreach (var plt in _palletList)
                            {
                                if (!plt.IsRawMaterial)
                                {
                                    plt.PlaceRecipeCode = "";
                                    _logicWorker.SetPalletAttributes(plt.PalletNo, false, true, "");
                                }
                            }

                            MessageBox.Show("Okutulan barkod bilgisi ile eşleşen bir reçete bulunamadı.", "Uyarı", MessageBoxButton.OK);
                            txtRecipeBarocde.Text = "";

                            txtActiveRecipeCode.Content = "";
                            txtActiveRecipeName.Content = "";
                        }
                    }

                    this.Dispatcher.Invoke((Action)delegate
                    {
                        this.BindLivePalletStates();
                    });
                }
            }
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke((Action)delegate
            {
                lblError.Content = "";
            });

            _plc.Set_Reset_Plc_Variables(1);
            _logicWorker.ResetFlags();
            _plc.Set_PlcEmgReset(1);
            _plc.Set_PlcEmergency(0);

            this._plc.Set_Robot_Start(0);
            this._plc.Set_Robot_Start(1);
            _plc.Set_RobotHold(0);
            this._plc.Set_Robot_Start(0);
            this._plc.Set_Robot_Start(1);

            _tmrResetInvoker.Stop();
            _tmrResetInvoker.Enabled = true;
            _tmrResetInvoker.Start();
        }

        private void _tmrResetInvoker_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke((Action)delegate
            {
                _plc.Set_Reset_Plc_Variables(1);

                _logicWorker.ResetFlags();
                this._plc.Set_Robot_Start(0);
                this._plc.Set_Robot_Start(1);
                _plc.Set_RobotHold(0);
                this._plc.Set_Robot_Start(0);
                this._plc.Set_Robot_Start(1);
            });
        }

        private void btnPlaceRequests_Click(object sender, RoutedEventArgs e)
        {
            ProductRecipeWindow wnd = new ProductRecipeWindow();
            wnd.ShowDialog();
        }

        private void plt_OnlineEditRequested(int palletNo)
        {
            if (_logicWorker != null)
            {
                var palletData = _logicWorker.GetPalletData(palletNo);
                var reqData = _logicWorker.GetPalletRequest(palletNo);

                OnlinePalletEdit wnd = new OnlinePalletEdit();
                wnd.Pallet = palletData;
                wnd.PlaceRequest = reqData;
                wnd.ShowDialog();

                this.BindLivePalletStates();
            }
        }

        private void btnSelectRecipe_Click(object sender, RoutedEventArgs e)
        {
            ProductRecipeListWindow wnd = new ProductRecipeListWindow();
            wnd.ShowDialog();

            if (wnd.RecipeId > 0)
            {
                using (HekaDbContext db = SchemaFactory.CreateContext())
                {
                    var dbRecipe = db.PlaceRequest.FirstOrDefault(d => d.Id == wnd.RecipeId);
                    if (dbRecipe != null)
                    {
                        if (_palletList != null)
                        {
                            foreach (var plt in _palletList)
                            {
                                if (!plt.IsRawMaterial)
                                {
                                    plt.PlaceRecipeCode = dbRecipe.RecipeCode;
                                    _logicWorker.SetPalletAttributes(plt.PalletNo, false, true, dbRecipe.RequestNo);
                                }
                            }
                        }

                        if (_activeRecipe == null)
                            _activeRecipe = new PlaceRequestDTO();

                        _activeRecipe.RequestNo = dbRecipe.RequestNo;
                        _activeRecipe.RecipeName = dbRecipe.RecipeName;

                        txtRecipeBarocde.Text = "";

                        txtActiveRecipeCode.Content = _activeRecipe.RequestNo;
                        txtActiveRecipeName.Content = _activeRecipe.RecipeName;

                        _placeByRecipe = true;
                    }
                    else
                    {
                        if (_palletList != null)
                        {
                            foreach (var plt in _palletList)
                            {
                                if (!plt.IsRawMaterial)
                                {
                                    plt.PlaceRecipeCode = "";
                                    _logicWorker.SetPalletAttributes(plt.PalletNo, false, true, "");
                                }
                            }
                        }

                        MessageBox.Show("Okutulan barkod bilgisi ile eşleşen bir reçete bulunamadı.", "Uyarı", MessageBoxButton.OK);
                        txtRecipeBarocde.Text = "";

                        txtActiveRecipeCode.Content = "";
                        txtActiveRecipeName.Content = "";
                        _activeRecipe = null;
                        _placeByRecipe = false;
                    }
                }

                this.Dispatcher.Invoke((Action)delegate
                {
                    this.BindLivePalletStates();
                    this.BindLivePalletStates();
                });
            }
        }

        private void btnClearRecipe_Click(object sender, RoutedEventArgs e)
        {
            _activeRecipe = null;
            _placeByRecipe = false;
            txtActiveRecipeCode.Content = "";
            txtActiveRecipeName.Content = "";
            txtRecipeBarocde.Text = "";
        }

        private void btnPalletReset_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Tüm palet durumlarını sıfırlamak istediğinizden emin misiniz?", "Uyarı", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _manualRecipe = null;
                _activeRecipe = null;
                txtActiveRecipeCode.Content = "";
                txtActiveRecipeName.Content = "";

                _logicWorker.ClearPallets();

                this.Dispatcher.Invoke((Action)delegate
                {
                    this.BindLivePalletStates();
                });
            }
        }

        private void cmbRobotSpeed_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var speedIndex = cmbRobotSpeed.SelectedIndex;
            int speed = 100;

            switch (speedIndex)
            {
                case 0:
                    speed = 25;
                    break;
                case 1:
                    speed = 50;
                    break;
                case 2:
                    speed = 60;
                    break;
                case 3:
                    speed = 75;
                    break;
                case 4:
                    speed = 90;
                    break;
                case 5:
                    speed = 100;
                    break;
                default:
                    break;
            }

            try
            {
                _plc.Set_RobotSpeed(speed);
            }
            catch (Exception)
            {

            }
        }
    }
}
