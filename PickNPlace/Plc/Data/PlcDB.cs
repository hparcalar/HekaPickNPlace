using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PickNPlace.Plc.Data
{
    public class PlcDB
    {
        #region DATA ELEMENTS

        public bool Servo_Home { get; set; }
        public int Servo_TargetPos { get; set; }
        public bool Servo_Reset { get; set; }
        public bool Servo_StartFlag { get; set; }
        public bool Servo_JogPlus { get; set; }
        public bool Servo_JogMinus { get; set; }
        public int Servo_CurrentPos { get; set; }


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
