using PickNPlace.Plc;
using PickNPlace.Plc.Data;
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

        private PlcWorker _plc;
        private PlcDB _db;

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

        }

        private void btnSaveServoPositions_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnBackTome_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
