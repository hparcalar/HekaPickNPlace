using PickNPlace.Plc;
using PickNPlace.Plc.Data;
using PickNPlace.DataAccess;
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
        }

        private void btnSendTargetPos_Click(object sender, RoutedEventArgs e)
        {
            this._plc.HoldReading = true;

            this._db.Servo_TargetPos = Convert.ToInt32(txtTargetPos.Text);
            this._plc.Set_ServoTargetPos = true;

            this._plc.HoldReading = false;
        }

        private void btnJogPlus_Click(object sender, RoutedEventArgs e)
        {
            this._plc.HoldReading = true;

            this._db.Servo_JogPlus = true;
            this._plc.Set_ServoJogPlus = true;

            this._plc.HoldReading = false;
        }

        private void btnJogMinus_Click(object sender, RoutedEventArgs e)
        {
            this._plc.HoldReading = true;

            this._db.Servo_JogMinus = true;
            this._plc.Set_ServoJogMinus = true;

            this._plc.HoldReading = false;
        }

        private void btnSetServoSpeed_Click(object sender, RoutedEventArgs e)
        {
            // send data to plc
            this._plc.HoldReading = true;

            this._db.Servo_Speed = Convert.ToInt32(txtServoSpeed.Text);
            this._plc.Set_ServoSpeed = true;

            this._plc.HoldReading = false;

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
            this._plc.HoldReading = true;

            this._db.Servo_PosCam1 = Convert.ToInt32(txtServoPosCam1.Text);
            this._plc.Set_ServoPosCam1 = true;

            this._db.Servo_PosCam2 = Convert.ToInt32(txtServoPosCam2.Text);
            this._plc.Set_ServoPosCam2 = true;

            this._plc.HoldReading = false;

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
    }
}
