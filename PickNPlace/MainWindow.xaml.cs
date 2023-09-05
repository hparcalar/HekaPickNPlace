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
        private PalletStateDTO[] _palletList;

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
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.CreateInitialData();
            this.BindLivePalletStates();

            _runFlagListener = true;
            _flagListener = Task.Run(this.LoopFlagListen);
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
            using (HekaDbContext db = SchemaFactory.CreateContext())
            {
                _palletList = db.PalletStateLive.Select(d => new PalletStateDTO
                {
                    Id = d.Id,
                    CompletedBatchCount = d.CompletedBatchCount,
                    CurrentBatchIsCompleted = d.CurrentBatchIsCompleted,
                    CurrentBatchIsStarted = d.CurrentBatchIsStarted,
                    CurrentBatchNo = d.CurrentBatchNo,
                    CurrentItemIsCompleted = d.CurrentItemIsCompleted,
                    CurrentItemIsStarted = d.CurrentItemIsStarted,
                    CurrentItemNo = d.CurrentItemNo,
                    IsDropable = d.IsDropable,
                    IsPickable = d.IsPickable,
                    PalletNo = d.PalletNo,
                    PalletRecipeId = d.PalletRecipeId,
                    PlaceRequestId = d.PlaceRequestId,
                }).ToArray();
            }

            // update pallet visuals
            foreach (var item in _palletList)
            {
                if (item.PalletNo == 1)
                {
                    plt1.PickingText = item.IsPickable == true ? "HAMMADDE" : "BOŞ PALET";
                    plt1.PickingColor = item.IsPickable == true ? "#FFEAE00D" : "#FF2BEA0D";
                }
                else if (item.PalletNo == 2)
                {
                    plt2.PickingText = item.IsPickable == true ? "HAMMADDE" : "BOŞ PALET";
                    plt2.PickingColor = item.IsPickable == true ? "#FFEAE00D" : "#FF2BEA0D";
                }
            }
        }

        private void plt_OnPickingStatusChanged(int palletNo)
        {
            var pallet = _palletList.FirstOrDefault(d => d.PalletNo == palletNo);
            if (pallet != null)
            {
                using (HekaDbContext db = SchemaFactory.CreateContext())
                {
                    var dbPallet = db.PalletStateLive.FirstOrDefault(d => d.Id == pallet.Id);
                    if (dbPallet != null)
                    {
                        dbPallet.IsPickable = !(dbPallet.IsPickable ?? false);
                        dbPallet.IsDropable = !dbPallet.IsPickable;

                        // check at least 1 pallet exists for picking or cancel toggle process
                        if (dbPallet.IsPickable == false)
                        {
                            if (!db.PalletStateLive.Any(d => d.Id != dbPallet.Id && d.IsPickable == true))
                            {
                                MessageBox.Show("En az 1 palet HAMMADDE olarak seçilmelidir.", "UYARI", MessageBoxButton.OK);
                                return;
                            }
                        }

                        db.SaveChanges();
                    }
                }

                this.BindLivePalletStates();
            }
        }

        private void _plc_OnPlcConnectionChanged(bool connected)
        {
            this.Dispatcher.BeginInvoke((Action)delegate
            {
                imgPlcOk.Source = new BitmapImage(new Uri(connected ? "/green_circle.png" : "/red_circle.png", UriKind.Relative));
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

            try
            {
                this._flagListener.Dispose();
            }
            catch (Exception)
            {

            }
        }

        private void btnStartToggle_Click(object sender, RoutedEventArgs e)
        {
            this.BindDefaults();

            _isStarted = !_isStarted;

            this._plc.Set_SystemAuto((byte)(_isStarted ? 1 : 0));

            this.UpdateSystemStatus();
        }

        private void BindDefaults()
        {
            using (HekaDbContext db = SchemaFactory.CreateContext())
            {
                int tryValue = 0;

                var prmData = db.SysParam.FirstOrDefault(d => d.ParamCode == "ServoSpeed");
                if (prmData != null && Int32.TryParse(prmData.ParamValue, out tryValue))
                {
                    this._plc.Set_ServoSpeed(Convert.ToInt32(prmData.ParamValue));
                }

                prmData = db.SysParam.FirstOrDefault(d => d.ParamCode == "ServoPosCam1");
                if (prmData != null && Int32.TryParse(prmData.ParamValue, out tryValue))
                {
                    this._plc.Set_ServoPosCam1(Convert.ToInt32(prmData.ParamValue));
                }

                prmData = db.SysParam.FirstOrDefault(d => d.ParamCode == "ServoPosCam2");
                if (prmData != null && Int32.TryParse(prmData.ParamValue, out tryValue))
                {
                    this._plc.Set_ServoPosCam2(Convert.ToInt32(prmData.ParamValue));
                }

                this._plc.Set_ServoStart(1);
            }
        }

        private void UpdateSystemStatus()
        {
            if (_plcDB.System_Auto)
            {
                txtStart.Text = "SİSTEMİ DURDUR";
                imgStart.Source = new BitmapImage(new Uri("/stop.png", UriKind.Relative));
            }
            else
            {
                txtStart.Text = "SİSTEMİ BAŞLAT";
                imgStart.Source = new BitmapImage(new Uri("/start.png", UriKind.Relative));
            }
        }

        #region CONTINUOUS FLAG LISTENING
        private async Task LoopFlagListen()
        {
            while (_runFlagListener)
            {
                try
                {
                    PlcDB _plcDb = PlcDB.Instance();
                    _isStarted = _plcDb.System_Auto;

                    await this.Dispatcher.BeginInvoke((Action)delegate
                    {
                        this.UpdateSystemStatus();
                    });
                }
                catch (Exception)
                {

                }

                await Task.Delay(100);
            }
        }
        #endregion

        
    }
}
