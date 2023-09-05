using PickNPlace.Plc;
using PickNPlace.Plc.Data;
using PickNPlace.DataAccess;
using PickNPlace.Camera;
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
using System.Windows.Shapes;

namespace PickNPlace
{
    /// <summary>
    /// Interaction logic for ManagementWindow.xaml
    /// </summary>
    public partial class ManagementWindow : Window
    {
        public ManagementWindow()
        {
            InitializeComponent();

            this._db = PlcDB.Instance();
            this._plc = PlcWorker.Instance();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.BindDefaults();
        }

        private PlcWorker _plc;
        private PlcDB _db;

        private void BindDefaults()
        {
            using (HekaDbContext db = SchemaFactory.CreateContext())
            {
                int tryValue = 0;

                var prmData = db.SysParam.FirstOrDefault(d => d.ParamCode == "ServoSpeed");
                if (prmData != null && Int32.TryParse(prmData.ParamValue, out tryValue))
                {
                    txtServoSpeed.Text = prmData.ParamValue;
                }

                prmData = db.SysParam.FirstOrDefault(d => d.ParamCode == "ServoPosCam1");
                if (prmData != null && Int32.TryParse(prmData.ParamValue, out tryValue))
                {
                    txtServoPosCam1.Text = prmData.ParamValue;
                }

                prmData = db.SysParam.FirstOrDefault(d => d.ParamCode == "ServoPosCam2");
                if (prmData != null && Int32.TryParse(prmData.ParamValue, out tryValue))
                {
                    txtServoPosCam2.Text = prmData.ParamValue;
                }
            }

            txtTargetPos.Text = this._db.Servo_CurrentPos.ToString();
        }

        private void btnSendTargetPos_Click(object sender, RoutedEventArgs e)
        {
            this._plc.Set_ServoTargetPos(Convert.ToInt32(txtTargetPos.Text));
        }

        private void btnSetServoSpeed_Click(object sender, RoutedEventArgs e)
        {
            // send data to plc
            this._plc.Set_ServoSpeed(Convert.ToInt32(txtServoSpeed.Text));

            // save to database
            using (HekaDbContext db = SchemaFactory.CreateContext())
            {
                var exPrm = db.SysParam.FirstOrDefault(d => d.ParamCode == "ServoSpeed");
                if (exPrm == null)
                {
                    exPrm = new SysParam();
                    exPrm.ParamCode = "ServoSpeed";
                    db.SysParam.Add(exPrm);
                }

                exPrm.ParamValue = txtServoSpeed.Text;

                db.SaveChanges();
            }
        }

        private void btnSaveServoPositions_Click(object sender, RoutedEventArgs e)
        {
            // send data to plc
            this._plc.Set_ServoPosCam1(Convert.ToInt32(txtServoPosCam1.Text));
            this._plc.Set_ServoPosCam2(Convert.ToInt32(txtServoPosCam2.Text));

            // save to database
            using (HekaDbContext db = SchemaFactory.CreateContext())
            {
                // set pos 1
                var exPrm = db.SysParam.FirstOrDefault(d => d.ParamCode == "ServoPosCam1");
                if (exPrm == null)
                {
                    exPrm = new SysParam();
                    exPrm.ParamCode = "ServoPosCam1";
                    db.SysParam.Add(exPrm);
                }

                exPrm.ParamValue = txtServoPosCam1.Text;

                // set pos 2
                var exPrm2 = db.SysParam.FirstOrDefault(d => d.ParamCode == "ServoPosCam2");
                if (exPrm2 == null)
                {
                    exPrm2 = new SysParam();
                    exPrm2.ParamCode = "ServoPosCam2";
                    db.SysParam.Add(exPrm2);
                }

                exPrm2.ParamValue = txtServoPosCam2.Text;

                db.SaveChanges();
            }
        }

        private void btnBackTome_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnJogPlus_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            this._plc.Set_ServoTargetPos(Convert.ToInt32(txtTargetPos.Text) + 10);
            this._plc.Set_ServoJogPlus(1);
        }

        private void btnJogPlus_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            this._plc.Set_ServoJogPlus(0);
        }

        private void btnJogMinus_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            this._plc.Set_ServoTargetPos(Convert.ToInt32(txtTargetPos.Text) - 10);
            this._plc.Set_ServoJogMinus(1);
        }

        private void btnJogMinus_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            this._plc.Set_ServoJogMinus(0);
        }

        private void btnGotoPos1_Click(object sender, RoutedEventArgs e)
        {
            this._plc.Set_ServoTargetPos(_db.Servo_PosCam1);
        }

        private void btnGotoPos2_Click(object sender, RoutedEventArgs e)
        {
            this._plc.Set_ServoTargetPos(_db.Servo_PosCam2);
        }

        private void btnTriggerCamera_Click(object sender, RoutedEventArgs e)
        {
            MechWorker camera = new MechWorker();
            if (camera.TriggerCamera(txtCameraProgramId.Text))
            {
                txtCameraVisionResult.Text = camera.GetVisionTargets(txtCameraProgramId.Text);
            }
        }
    }
}
