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


namespace PickNPlace
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // local variables
        private bool _isStarted = false;
        private PlcWorker _plc;
        private PlcDB _plcDB;
        private Task _flagListener;
        private bool _runFlagListener = false;
        private HkAutoPallet[] _palletList;
        private HkLogicWorker _logicWorker;
        private PlaceRequestDTO _activeRecipe = new PlaceRequestDTO();
        private Timer _tmrError = new Timer(10000);

        public MainWindow()
        {
            InitializeComponent();

            // make db migrations
            SchemaFactory.ApplyMigrations();

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

            _tmrError.AutoReset = false;
            _tmrError.Elapsed += _tmrError_Elapsed;
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
            this.Dispatcher.Invoke((Action)delegate
            {
                lblError.Content = "";
            });
        }

        private void _logicWorker_OnError(string message)
        {
            this.Dispatcher.Invoke((Action)delegate
            {
                lblError.Content = message;

                _tmrError.Stop();

                _tmrError.Enabled = true;
                _tmrError.Start();
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
            this.CreateInitialData();
            this.BindLivePalletStates();

            //_runFlagListener = true;
            //_flagListener = Task.Run(this.LoopFlagListen);
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

            if (_palletList == null || _palletList.Length == 0)
            {
                _logicWorker.SetPalletAttributes(1, true, false, "");
                _logicWorker.SetPalletAttributes(2, true, false, "");
                _logicWorker.SetPalletAttributes(3, false, false, "");
                _logicWorker.SetPalletAttributes(4, false, false, "");
                _logicWorker.SetPalletAttributes(5, false, false, "");
                _logicWorker.SetPalletAttributes(6, false, false, "");
            }

            _palletList = _logicWorker.GetPalletList().ToArray();


            // update pallet visuals
            foreach (var item in _palletList)
            {
                if (item.PalletNo == 1)
                {
                    plt1.PickingText = item.IsRawMaterial == true ? "HAMMADDE" : "BOŞ PALET";
                    plt1.PickingColor = item.IsRawMaterial == true ? "#FFF2FBBB" : "#FF93C1F0";
                    plt1.IsPalletEnabled = item.IsEnabled;
                    plt1.RecipeName = item.IsRawMaterial ? item.RawMaterialCode : item.PlaceRecipeCode;
                    plt1.SackType = item.SackType == 1 ? "40x60" : item.SackType == 2 ? "30x50" : item.SackType == 3 ? "50x70" : "";
                    plt1.IsActivePallet = _plcDB.System_Auto && _logicWorker.CurrentRawPalletNo == 1;
                }
                else if (item.PalletNo == 2)
                {
                    plt2.PickingText = item.IsRawMaterial == true ? "HAMMADDE" : "BOŞ PALET";
                    plt2.PickingColor = item.IsRawMaterial == true ? "#FFF2FBBB" : "#FF93C1F0";
                    plt2.IsPalletEnabled = item.IsEnabled;
                    plt2.RecipeName = item.IsRawMaterial ? item.RawMaterialCode : item.PlaceRecipeCode;
                    plt2.SackType = item.SackType == 1 ? "40x60" : item.SackType == 2 ? "30x50" : item.SackType == 3 ? "50x70" : "";
                    plt2.IsActivePallet = _plcDB.System_Auto && _logicWorker.CurrentRawPalletNo == 2;
                }
                else if (item.PalletNo == 3)
                {
                    plt3.IsPalletEnabled = item.IsEnabled;
                    plt3.IsActivePallet = _plcDB.System_Auto && _logicWorker.CurrentTargetPalletNo == 3;
                }
                else if (item.PalletNo == 4)
                {
                    plt4.IsPalletEnabled = item.IsEnabled;
                    plt4.IsActivePallet = _plcDB.System_Auto && _logicWorker.CurrentTargetPalletNo == 4;
                }
                else if (item.PalletNo == 5)
                {
                    plt5.IsPalletEnabled = item.IsEnabled;
                    plt5.IsActivePallet = _plcDB.System_Auto && _logicWorker.CurrentTargetPalletNo == 5;
                }
                else if (item.PalletNo == 6)
                {
                    plt6.IsPalletEnabled = item.IsEnabled;
                    plt6.IsActivePallet = _plcDB.System_Auto && _logicWorker.CurrentTargetPalletNo == 6;
                }
            }
        }

        private void plt_OnPickingStatusChanged(int palletNo)
        {
            var pallet = _palletList.FirstOrDefault(d => d.PalletNo == palletNo);
            if (pallet != null)
            {
                pallet.IsRawMaterial = !pallet.IsRawMaterial;
                _logicWorker.SetPalletAttributes(palletNo, pallet.IsRawMaterial, pallet.IsEnabled, pallet.PlaceRecipeCode);

                this.Dispatcher.Invoke((Action)delegate
                {
                    this.BindLivePalletStates();
                });
            }
        }

        private void plt_OnPalletEnabledChanged(int palletNo, bool enabled)
        {
            var pallet = _palletList.FirstOrDefault(d => d.PalletNo == palletNo);
            if (pallet != null)
            {
                pallet.IsEnabled = !pallet.IsEnabled;

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

                this.Dispatcher.Invoke((Action)delegate
                {
                    this.BindLivePalletStates();
                });
            }
        }

        private void plt_OnSelectRecipeSignal(int palletNo)
        {
            if (string.IsNullOrEmpty(_activeRecipe.RequestNo))
            {
                MessageBox.Show("Lütfen önce reçete barkodunu okutunuz.", "Uyarı", MessageBoxButton.OK);
                return;
            }

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
                else
                {

                }
            }
        }

        private void _plc_OnPlcConnectionChanged(bool connected)
        {
            this.Dispatcher.BeginInvoke((Action)delegate
            {
                imgPlcOk.Source = new BitmapImage(new Uri(connected ? "Images/green_circle.png" : "Images/red_circle.png", UriKind.Relative));
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
            this._plc.Stop();
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

            this._plc.Set_SystemAuto((byte)(targetInfo ? 1 : 0));
            if (targetInfo)
            {
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
                txtStart.Text = "SİSTEMİ DURDUR";
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
            _plc.Set_Reset_Plc_Variables(1);

            //_plc.Set_CaptureOk(0);
            //_plc.Set_PlaceCalculationOk(0);
            //_plc.Set_RobotNextTargetOk(0);
            //_plc.Set_RobotPickingOk(0);
            //_plc.Set_RobotPlacingOk(0);
            //_plc.Set_SystemAuto(0);

            _logicWorker.ClearPallets();

            this.Dispatcher.Invoke((Action)delegate
            {
                this.BindLivePalletStates();
                this.BindLivePalletStates();
            });
        }

        private void btnPlaceRequests_Click(object sender, RoutedEventArgs e)
        {
            ProductRecipeWindow wnd = new ProductRecipeWindow();
            wnd.ShowDialog();
        }

        private void plt_OnlineEditRequested(int palletNo)
        {
            var palletData = _logicWorker.GetPalletData(palletNo);
            var reqData = _logicWorker.GetPalletRequest(palletNo);

            OnlinePalletEdit wnd = new OnlinePalletEdit();
            wnd.Pallet = palletData;
            wnd.PlaceRequest = reqData;
            wnd.ShowDialog();
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
                        foreach (var plt in _palletList)
                        {
                            if (!plt.IsRawMaterial)
                            {
                                plt.PlaceRecipeCode = dbRecipe.RecipeCode;
                                _logicWorker.SetPalletAttributes(plt.PalletNo, false, true, dbRecipe.RequestNo);
                            }
                        }

                        _activeRecipe.RequestNo = dbRecipe.RequestNo;
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
                    this.BindLivePalletStates();
                });
            }
        }
    }
}
