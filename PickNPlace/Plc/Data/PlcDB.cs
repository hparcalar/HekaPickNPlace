using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PickNPlace.Plc.Data
{
    public class PlcDB
    {
        #region EVENTS & DELEGATES
        public delegate void RobotPickedUp();
        public event RobotPickedUp OnRobotPickedUp;

        public delegate void RobotPlacedDown();
        public event RobotPlacedDown OnRobotPlacedDown;

        public delegate void SystemAutoChanged(bool auto);
        public event SystemAutoChanged OnSystemAutoChanged;
        #endregion

        #region DATA ELEMENTS

        public bool Servo_Home { get; set; }
        public int Servo_TargetPos { get; set; }
        public bool Servo_Reset { get; set; }
        public bool Servo_StartFlag { get; set; }
        public bool Servo_JogPlus { get; set; }
        public bool Servo_JogMinus { get; set; }
        public int Servo_CurrentPos { get; set; }
        public int Servo_Speed { get; set; }
        public int Servo_PosCam1 { get; set; }
        public int Servo_PosCam2 { get; set; }

        private bool _SystemAuto;
        public bool System_Auto
        {
            get
            {
                return _SystemAuto;
            }
            set
            {
                var oldValue = _SystemAuto;
                _SystemAuto = value;

                if (value != oldValue)
                    OnSystemAutoChanged?.Invoke(value);
            }
        }

        private bool _RobotPickingOk;
        public bool RobotPickingOk
        {
            get
            {
                return _RobotPickingOk;
            }
            set
            {
                var oldValue = _RobotPickingOk;
                _RobotPickingOk = value;

                if (value && !oldValue)
                    OnRobotPickedUp?.Invoke();
            }
        }

        private bool _RobotPlacingOk;
        public bool RobotPlacingOk
        {
            get
            {
                return _RobotPlacingOk;
            }
            set
            {
                var oldValue = _RobotPlacingOk;
                _RobotPlacingOk = value;

                if (value && !oldValue)
                    OnRobotPlacedDown?.Invoke();
            }
        }

        #endregion

        private static PlcDB _instance;

        private PlcDB()
        {
            
        }

        public static PlcDB Instance()
        {
            if (_instance == null)
                _instance = new PlcDB();

            return _instance;
        }
    }
}
