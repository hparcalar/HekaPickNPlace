using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PickNPlace.Plc.Data;
using Sharp7;

namespace PickNPlace.Plc
{
    public class PlcWorker
    {
        private static PlcWorker _instance;
        private PlcWorker()
        {
            _db = PlcDB.Instance();
            _plc = new S7Client();
        }

        public static PlcWorker Instance()
        {
            if (_instance == null)
                _instance = new PlcWorker();

            return _instance;
        }

        public delegate void PlcConnectionStatusEvent(bool connected);
        public event PlcConnectionStatusEvent OnPlcConnectionChanged;

        private Task _Listener;
        private bool _runListener;
        private PlcDB _db;
        private S7Client _plc;
        private bool _stateIsConnected = false;
        private const int DB_NUMBER = 2;

        bool _isSetRunning = false;
        bool _isReading = false;

        public void Start()
        {
            //if (!_runListener)
            //{
            //    _Listener = Task.Run(this._ListenLoop);
            //    _runListener = true;
            //}
        }

        public void Stop()
        {
            _runListener = false;

            try
            {
                if (this._Listener != null)
                    this._Listener.Dispose();
            }
            catch (Exception)
            {

            }

            try
            {
                if (this._plc != null && this._plc.Connected)
                    this._plc.Disconnect();
            }
            catch (Exception)
            {

            }
        }

        

        #region VARIABLES TO BE SENT TO THE PLC

        public bool Set_ServoHome(byte val)
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            bool result = false;
            try
            {
                byte[] data = new byte[1] { val };
                S7MultiVar Writer = new S7MultiVar(this._plc);
                Writer.Add(S7Consts.S7AreaDB, S7Consts.S7WLBit, DB_NUMBER, 0, 1, ref data);

                int writeResult = Writer.Write();
                result = writeResult == 0;
            }
            catch (Exception)
            {
                result = false;
            }
            _isSetRunning = false;
            return result;
        }

        public bool Set_RobotRestart(byte val)
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            bool result = false;
            try
            {
                byte[] data = new byte[1] { val };
                S7MultiVar Writer = new S7MultiVar(this._plc);
                Writer.Add(S7Consts.S7AreaDB, S7Consts.S7WLBit, DB_NUMBER, 55 * 8 + 6, 1, ref data);

                int writeResult = Writer.Write();
                result = writeResult == 0;
            }
            catch (Exception)
            {
                result = false;
            }
            _isSetRunning = false;
            return result;
        }

        public bool Set_SystemAuto(byte val)
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            bool result = false;
            try
            {
                byte[] data = new byte[1] { val };
                S7MultiVar Writer = new S7MultiVar(this._plc);
                Writer.Add(S7Consts.S7AreaDB, S7Consts.S7WLBit, DB_NUMBER, 14 * 8, 1, ref data);

                int writeResult = Writer.Write();
                result = writeResult == 0;
            }
            catch (Exception)
            {
                result = false;
            }

            _isSetRunning = false;

            return result;
        }

        public bool Set_ServoReset(byte val)
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            bool result = false;
            try
            {
                byte[] data = new byte[1] { val };
                S7MultiVar Writer = new S7MultiVar(this._plc);
                Writer.Add(S7Consts.S7AreaDB, S7Consts.S7WLBit, DB_NUMBER, (4 * 8) + 1, 1, ref data);

                int writeResult = Writer.Write();
                result = writeResult == 0;
            }
            catch (Exception)
            {
                result = false;
            }

            _isSetRunning = false;

            return result;
        }

        public bool Set_ServoTargetPos(int val)
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            bool result = false;
            try
            {
                byte[] data = new byte[2];
                S7.SetIntAt(data, 0, (short)val);

                S7MultiVar Writer = new S7MultiVar(this._plc);
                Writer.Add(S7Consts.S7AreaDB, S7Consts.S7WLInt, DB_NUMBER, 2, 1, ref data);

                int writeResult = Writer.Write();
                result = writeResult == 0;
            }
            catch (Exception)
            {
                result = false;
            }

            _isSetRunning = false;

            return result;
        }

        public bool Set_ServoStart(byte val)
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            bool result = false;
            try
            {
                byte[] data = new byte[1] { val };
                S7MultiVar Writer = new S7MultiVar(this._plc);
                Writer.Add(S7Consts.S7AreaDB, S7Consts.S7WLBit, DB_NUMBER, (4 * 8), 1, ref data);

                int writeResult = Writer.Write();
                result = writeResult == 0;
            }
            catch (Exception)
            {
                result = false;
            }

            _isSetRunning = false;

            return result;
        }

        public bool Set_ServoJogPlus(byte val)
        {
            bool result = false;
            try
            {
                byte[] data = new byte[1] { val };
                S7MultiVar Writer = new S7MultiVar(this._plc);
                Writer.Add(S7Consts.S7AreaDB, S7Consts.S7WLBit, DB_NUMBER, (4 * 8) + 2, 1, ref data);

                int writeResult = Writer.Write();
                result = writeResult == 0;
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }

        public bool Set_ServoJogMinus(byte val)
        {
            bool result = false;
            try
            {
                byte[] data = new byte[1] { val };
                S7MultiVar Writer = new S7MultiVar(this._plc);
                Writer.Add(S7Consts.S7AreaDB, S7Consts.S7WLBit, DB_NUMBER, (4 * 8) + 3, 1, ref data);

                int writeResult = Writer.Write();
                result = writeResult == 0;
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }

        public bool Set_ServoSpeed(int val)
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            bool result = false;
            try
            {
                byte[] data = new byte[2];
                S7.SetIntAt(data, 0, (short)val);

                S7MultiVar Writer = new S7MultiVar(this._plc);
                Writer.Add(S7Consts.S7AreaDB, S7Consts.S7WLInt, DB_NUMBER, 8, 1, ref data);

                int writeResult = Writer.Write();
                result = writeResult == 0;
            }
            catch (Exception)
            {
                result = false;
            }
            _isSetRunning = false;
            return result;
        }

        public bool Set_ServoPosCam1(int val)
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            bool result = false;
            try
            {
                byte[] data = new byte[2];
                S7.SetIntAt(data, 0, (short)val);

                S7MultiVar Writer = new S7MultiVar(this._plc);
                Writer.Add(S7Consts.S7AreaDB, S7Consts.S7WLInt, DB_NUMBER, 10, 1, ref data);

                int writeResult = Writer.Write();
                result = writeResult == 0;
            }
            catch (Exception)
            {
                result = false;
            }

            _isSetRunning = false;

            return result;
        }

        public bool Set_ServoPosCam2(int val)
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            bool result = false;
            try
            {
                byte[] data = new byte[2];
                S7.SetIntAt(data, 0, (short)val);

                S7MultiVar Writer = new S7MultiVar(this._plc);
                Writer.Add(S7Consts.S7AreaDB, S7Consts.S7WLInt, DB_NUMBER, 12, 1, ref data);

                int writeResult = Writer.Write();
                result = writeResult == 0;
            }
            catch (Exception)
            {
                result = false;
            }

            _isSetRunning = false;

            return result;
        }

        public bool Set_RobotX(int val)
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            bool result = false;
            try
            {
                byte[] data = new byte[2];
                S7.SetIntAt(data, 0, (short)val);

                S7MultiVar Writer = new S7MultiVar(this._plc);
                Writer.Add(S7Consts.S7AreaDB, S7Consts.S7WLInt, DB_NUMBER, 16, 1, ref data);

                int writeResult = Writer.Write();
                result = writeResult == 0;
            }
            catch (Exception)
            {
                result = false;
            }

            _isSetRunning = false;

            return result;
        }

        public bool Set_RobotY(int val)
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            bool result = false;
            try
            {
                byte[] data = new byte[2];
                S7.SetIntAt(data, 0, (short)val);

                S7MultiVar Writer = new S7MultiVar(this._plc);
                Writer.Add(S7Consts.S7AreaDB, S7Consts.S7WLInt, DB_NUMBER, 18, 1, ref data);

                int writeResult = Writer.Write();
                result = writeResult == 0;
            }
            catch (Exception)
            {
                result = false;
            }

            _isSetRunning = false;

            return result;
        }

        public bool Set_RobotZ(int val)
        {

            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            bool result = false;
            try
            {
                byte[] data = new byte[2];
                S7.SetIntAt(data, 0, (short)val);

                S7MultiVar Writer = new S7MultiVar(this._plc);
                Writer.Add(S7Consts.S7AreaDB, S7Consts.S7WLInt, DB_NUMBER, 20, 1, ref data);

                int writeResult = Writer.Write();
                result = writeResult == 0;
            }
            catch (Exception)
            {
                result = false;
            }

            _isSetRunning = false;


            return result;
        }

        public bool Set_RobotRX(int val)
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            bool result = false;
            try
            {
                byte[] data = new byte[2];
                S7.SetIntAt(data, 0, (short)val);

                S7MultiVar Writer = new S7MultiVar(this._plc);
                Writer.Add(S7Consts.S7AreaDB, S7Consts.S7WLInt, DB_NUMBER, 22, 1, ref data);

                int writeResult = Writer.Write();
                result = writeResult == 0;
            }
            catch (Exception)
            {
                result = false;
            }

            _isSetRunning = false;

            return result;
        }

        public bool Set_RobotRY(int val)
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            bool result = false;
            try
            {
                byte[] data = new byte[2];
                S7.SetIntAt(data, 0, (short)val);

                S7MultiVar Writer = new S7MultiVar(this._plc);
                Writer.Add(S7Consts.S7AreaDB, S7Consts.S7WLInt, DB_NUMBER, 24, 1, ref data);

                int writeResult = Writer.Write();
                result = writeResult == 0;
            }
            catch (Exception)
            {
                result = false;
            }

            _isSetRunning = false;

            return result;
        }

        public bool Set_RobotRZ(int val)
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            bool result = false;
            try
            {
                byte[] data = new byte[2];
                S7.SetIntAt(data, 0, (short)val);

                S7MultiVar Writer = new S7MultiVar(this._plc);
                Writer.Add(S7Consts.S7AreaDB, S7Consts.S7WLInt, DB_NUMBER, 26, 1, ref data);

                int writeResult = Writer.Write();
                result = writeResult == 0;
            }
            catch (Exception)
            {
                result = false;
            }

            _isSetRunning = false;

            return result;
        }

        public bool Set_CaptureOk(byte val)
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            bool result = false;
            try
            {
                byte[] data = new byte[1] { val };
                S7MultiVar Writer = new S7MultiVar(this._plc);
                Writer.Add(S7Consts.S7AreaDB, S7Consts.S7WLBit, DB_NUMBER, 28 * 8, 1, ref data);

                int writeResult = Writer.Write();
                result = writeResult == 0;
            }
            catch (Exception)
            {
                result = false;
            }

            _isSetRunning = false;

            return result;
        }

        public bool Set_PlaceCalculationOk(byte val)
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            bool result = false;
            try
            {
                byte[] data = new byte[1] { val };
                S7MultiVar Writer = new S7MultiVar(this._plc);
                Writer.Add(S7Consts.S7AreaDB, S7Consts.S7WLBit, DB_NUMBER, 28 * 8 + 1, 1, ref data);

                int writeResult = Writer.Write();
                result = writeResult == 0;
            }
            catch (Exception)
            {
                result = false;
            }

            _isSetRunning = false;

            return result;
        }

        public bool Set_RobotPickingOk(byte val)
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            bool result = false;
            try
            {
                byte[] data = new byte[1] { val };
                S7MultiVar Writer = new S7MultiVar(this._plc);
                Writer.Add(S7Consts.S7AreaDB, S7Consts.S7WLBit, DB_NUMBER, 32 * 8 + 0, 1, ref data);

                int writeResult = Writer.Write();
                result = writeResult == 0;
            }
            catch (Exception)
            {
                result = false;
            }

            _isSetRunning = false;

            return result;
        }

        public bool Set_RobotPlacingOk(byte val)
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            bool result = false;
            try
            {
                byte[] data = new byte[1] { val };
                S7MultiVar Writer = new S7MultiVar(this._plc);
                Writer.Add(S7Consts.S7AreaDB, S7Consts.S7WLBit, DB_NUMBER, 32 * 8 + 1, 1, ref data);

                int writeResult = Writer.Write();
                result = writeResult == 0;
            }
            catch (Exception)
            {
                result = false;
            }

            _isSetRunning = false;

            return result;
        }

        public bool Set_RobotNextTargetOk(byte val)
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            bool result = false;
            try
            {
                byte[] data = new byte[1] { val };
                S7MultiVar Writer = new S7MultiVar(this._plc);
                Writer.Add(S7Consts.S7AreaDB, S7Consts.S7WLBit, DB_NUMBER, 32 * 8 + 2, 1, ref data);

                int writeResult = Writer.Write();
                result = writeResult == 0;
            }
            catch (Exception)
            {
                result = false;
            }

            _isSetRunning = false;

            return result;
        }

        public bool Set_EmptyPalletNo(int val)
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            bool result = false;
            try
            {
                byte[] data = new byte[2];
                S7.SetIntAt(data, 0, (short)val);

                S7MultiVar Writer = new S7MultiVar(this._plc);
                Writer.Add(S7Consts.S7AreaDB, S7Consts.S7WLInt, DB_NUMBER, 30, 1, ref data);

                int writeResult = Writer.Write();
                result = writeResult == 0;
            }
            catch (Exception)
            {
                result = false;
            }

            _isSetRunning = false;

            return result;
        }

        public bool Set_RawPalletNo(int val)
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            bool result = false;
            try
            {
                byte[] data = new byte[2];
                S7.SetIntAt(data, 0, (short)val);

                S7MultiVar Writer = new S7MultiVar(this._plc);
                Writer.Add(S7Consts.S7AreaDB, S7Consts.S7WLInt, DB_NUMBER, 52, 1, ref data);

                int writeResult = Writer.Write();
                result = writeResult == 0;
            }
            catch (Exception)
            {
                result = false;
            }

            _isSetRunning = false;

            return result;
        }

        public bool Set_RobotSpeed(int val)
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            bool result = false;
            try
            {
                byte[] data = new byte[2];
                S7.SetIntAt(data, 0, (short)val);

                S7MultiVar Writer = new S7MultiVar(this._plc);
                Writer.Add(S7Consts.S7AreaDB, S7Consts.S7WLInt, DB_NUMBER, 50, 1, ref data);

                int writeResult = Writer.Write();
                result = writeResult == 0;
            }
            catch (Exception)
            {
                result = false;
            }

            _isSetRunning = false;

            return result;
        }

        #region SETTING TARGET PALLET COORDS TO ROBOT VIA PLC
        public bool Set_RobotX_ForTarget(int val)
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            bool result = false;
            try
            {
                byte[] data = new byte[2];
                S7.SetIntAt(data, 0, (short)val);

                S7MultiVar Writer = new S7MultiVar(this._plc);
                Writer.Add(S7Consts.S7AreaDB, S7Consts.S7WLInt, DB_NUMBER, 36, 1, ref data);

                int writeResult = Writer.Write();
                result = writeResult == 0;
            }
            catch (Exception)
            {
                result = false;
            }

            _isSetRunning = false;

            return result;
        }

        public bool Set_RobotY_ForTarget(int val)
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            bool result = false;
            try
            {
                byte[] data = new byte[2];
                S7.SetIntAt(data, 0, (short)val);

                S7MultiVar Writer = new S7MultiVar(this._plc);
                Writer.Add(S7Consts.S7AreaDB, S7Consts.S7WLInt, DB_NUMBER, 38, 1, ref data);

                int writeResult = Writer.Write();
                result = writeResult == 0;
            }
            catch (Exception)
            {
                result = false;
            }

            _isSetRunning = false;

            return result;
        }

        public bool Set_RobotZ_ForTarget(int val)
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            bool result = false;
            try
            {
                byte[] data = new byte[2];
                S7.SetIntAt(data, 0, (short)val);

                S7MultiVar Writer = new S7MultiVar(this._plc);
                Writer.Add(S7Consts.S7AreaDB, S7Consts.S7WLInt, DB_NUMBER, 40, 1, ref data);

                int writeResult = Writer.Write();
                result = writeResult == 0;
            }
            catch (Exception)
            {
                result = false;
            }

            _isSetRunning = false;

            return result;
        }

        public bool Set_RobotRX_ForTarget(int val)
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            bool result = false;
            try
            {
                byte[] data = new byte[2];
                S7.SetIntAt(data, 0, (short)val);

                S7MultiVar Writer = new S7MultiVar(this._plc);
                Writer.Add(S7Consts.S7AreaDB, S7Consts.S7WLInt, DB_NUMBER, 42, 1, ref data);

                int writeResult = Writer.Write();
                result = writeResult == 0;
            }
            catch (Exception)
            {
                result = false;
            }

            _isSetRunning = false;

            return result;
        }

        public bool Set_RobotRY_ForTarget(int val)
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            bool result = false;
            try
            {
                byte[] data = new byte[2];
                S7.SetIntAt(data, 0, (short)val);

                S7MultiVar Writer = new S7MultiVar(this._plc);
                Writer.Add(S7Consts.S7AreaDB, S7Consts.S7WLInt, DB_NUMBER, 44, 1, ref data);

                int writeResult = Writer.Write();
                result = writeResult == 0;
            }
            catch (Exception)
            {
                result = false;
            }

            _isSetRunning = false;

            return result;
        }

        public bool Set_RobotRZ_ForTarget(int val)
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            bool result = false;
            try
            {
                byte[] data = new byte[2];
                S7.SetIntAt(data, 0, (short)val);

                S7MultiVar Writer = new S7MultiVar(this._plc);
                Writer.Add(S7Consts.S7AreaDB, S7Consts.S7WLInt, DB_NUMBER, 46, 1, ref data);

                int writeResult = Writer.Write();
                result = writeResult == 0;
            }
            catch (Exception)
            {
                result = false;
            }

            _isSetRunning = false;

            return result;
        }
        #endregion

        public bool Set_Reset_Plc_Variables(byte val)
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            bool result = false;
            try
            {
                byte[] data = new byte[1] { val };
                S7MultiVar Writer = new S7MultiVar(this._plc);
                Writer.Add(S7Consts.S7AreaDB, S7Consts.S7WLBit, DB_NUMBER, 48 * 8 + 2, 1, ref data);

                int writeResult = Writer.Write();
                result = writeResult == 0;
            }
            catch (Exception)
            {
                result = false;
            }

            _isSetRunning = false;

            return result;
        }

        public bool Set_Robot_Start(byte val)
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            bool result = false;
            try
            {
                byte[] data = new byte[1] { val };
                S7MultiVar Writer = new S7MultiVar(this._plc);
                Writer.Add(S7Consts.S7AreaDB, S7Consts.S7WLBit, DB_NUMBER, 48 * 8, 1, ref data);

                int writeResult = Writer.Write();
                result = writeResult == 0;
            }
            catch (Exception)
            {
                result = false;
            }

            _isSetRunning = false;

            return result;
        }

        public bool Set_RobotRiskyPos(byte val)
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            bool result = false;
            try
            {
                byte[] data = new byte[1] { val };
                S7MultiVar Writer = new S7MultiVar(this._plc);
                Writer.Add(S7Consts.S7AreaDB, S7Consts.S7WLBit, DB_NUMBER, 48 * 8 + 5, 1, ref data);

                int writeResult = Writer.Write();
                result = writeResult == 0;
            }
            catch (Exception)
            {
                result = false;
            }

            _isSetRunning = false;

            return result;
        }

        public bool Set_RobotHold(byte val)
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            bool result = false;
            try
            {
                byte[] data = new byte[1] { val };
                S7MultiVar Writer = new S7MultiVar(this._plc);
                Writer.Add(S7Consts.S7AreaDB, S7Consts.S7WLBit, DB_NUMBER, 48 * 8 + 6, 1, ref data);

                int writeResult = Writer.Write();
                result = writeResult == 0;
            }
            catch (Exception)
            {
                result = false;
            }

            _isSetRunning = false;

            return result;
        }

        public bool Set_PlcEmergency(byte val)
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            bool result = false;
            try
            {
                byte[] data = new byte[1] { val };
                S7MultiVar Writer = new S7MultiVar(this._plc);
                Writer.Add(S7Consts.S7AreaDB, S7Consts.S7WLBit, DB_NUMBER, 48 * 8 + 1, 1, ref data);

                int writeResult = Writer.Write();
                result = writeResult == 0;
            }
            catch (Exception)
            {
                result = false;
            }

            _isSetRunning = false;

            return result;
        }
        public bool Set_PlcEmgReset(byte val)
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            bool result = false;
            try
            {
                byte[] data = new byte[1] { val };
                S7MultiVar Writer = new S7MultiVar(this._plc);
                Writer.Add(S7Consts.S7AreaDB, S7Consts.S7WLBit, DB_NUMBER, 49 * 8 + 0, 1, ref data);

                int writeResult = Writer.Write();
                result = writeResult == 0;
            }
            catch (Exception)
            {
                result = false;
            }

            _isSetRunning = false;

            return result;
        }

        public bool Set_PC_SomeoneInside(byte val)
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            bool result = false;
            try
            {
                byte[] data = new byte[1] { val };
                S7MultiVar Writer = new S7MultiVar(this._plc);
                Writer.Add(S7Consts.S7AreaDB, S7Consts.S7WLBit, DB_NUMBER, 55 * 8 + 7, 1, ref data);

                int writeResult = Writer.Write();
                result = writeResult == 0;
            }
            catch (Exception)
            {
                result = false;
            }

            _isSetRunning = false;

            return result;
        }

        public bool Set_PC_ManVacuum(byte val)
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            bool result = false;
            try
            {
                byte[] data = new byte[1] { val };
                S7MultiVar Writer = new S7MultiVar(this._plc);
                Writer.Add(S7Consts.S7AreaDB, S7Consts.S7WLBit, DB_NUMBER, 56 * 8 + 1, 1, ref data);

                int writeResult = Writer.Write();
                result = writeResult == 0;
            }
            catch (Exception)
            {
                result = false;
            }

            _isSetRunning = false;

            return result;
        }

        private void ValidateConnection()
        {
            bool plcAlive = false;
            int tryCount = 0;

            bool oldConnection = _plc != null && _plc.Connected;

            while (!plcAlive)
            {
                try
                {
                    byte[] data = new byte[1] { 0 };
                    S7MultiVar Reader = new S7MultiVar(this._plc);
                    Reader.Add(S7Consts.S7AreaDB, S7Consts.S7WLBit, DB_NUMBER, 48 * 8 + 3, 1, ref data);
                    Reader.Read();

                    if (data[0] == 1)
                        plcAlive = true;
                }
                catch (Exception)
                {

                }

                if (!plcAlive)
                {
                    try
                    {
                        _plc.Disconnect();
                    }
                    catch (Exception)
                    {

                    }

                    try
                    {
                        this._plc.ConnectTo("192.168.0.3", 0, 1);
                    }
                    catch (Exception)
                    {

                    }
                }

                tryCount++;

                if (tryCount > 3)
                    break;
            }

            try
            {
                if (!oldConnection && plcAlive)
                    this.OnPlcConnectionChanged?.Invoke(true);
                else if (oldConnection && !plcAlive)
                    this.OnPlcConnectionChanged?.Invoke(false);
            }
            catch (Exception)
            {

            }
           
        }

        #endregion

        #region GETTERS

        public bool Get_ServoIsArrived()
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            byte[] data = new byte[1] { 0 };

            try
            {
                S7MultiVar Reader = new S7MultiVar(this._plc);
                Reader.Add(S7Consts.S7AreaDB, S7Consts.S7WLBit, DB_NUMBER, 48 * 8 + 4, 1, ref data);
                Reader.Read();
            }
            catch (Exception)
            {

            }

            _isSetRunning = false;
            return data[0] == 1;
        }

        public int Get_ServoCurrentPos()
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            try
            {
                byte[] data = new byte[2];
                S7MultiVar Reader = new S7MultiVar(this._plc);
                Reader.Add(S7Consts.S7AreaDB, S7Consts.S7WLInt, DB_NUMBER, 6, 1, ref data);
                Reader.Read();

                int res = S7.GetIntAt(data, 0);

                _isSetRunning = false;
                return res;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public bool Get_RobotPickingOk()
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            byte[] data = new byte[1] { 0 };
            try
            {
                S7MultiVar Reader = new S7MultiVar(this._plc);
                Reader.Add(S7Consts.S7AreaDB, S7Consts.S7WLBit, DB_NUMBER, 32 * 8 + 0, 1, ref data);
                Reader.Read();
            }
            catch (Exception)
            {

            }

            _isSetRunning = false;
            return data[0] == 1;
        }

        public bool Get_RobotPlacingOk()
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            byte[] data = new byte[1] { 0 };

            try
            {
                S7MultiVar Reader = new S7MultiVar(this._plc);
                Reader.Add(S7Consts.S7AreaDB, S7Consts.S7WLBit, DB_NUMBER, 32 * 8 + 1, 1, ref data);
                Reader.Read();
            }
            catch (Exception)
            {

            }

            _isSetRunning = false;
            return data[0] == 1;
        }

        public bool Get_RobotRiskyPos()
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            byte[] data = new byte[1] { 0 };

            try
            {
                S7MultiVar Reader = new S7MultiVar(this._plc);
                Reader.Add(S7Consts.S7AreaDB, S7Consts.S7WLBit, DB_NUMBER, 48 * 8 + 5, 1, ref data);
                Reader.Read();
            }
            catch (Exception)
            {

            }

            _isSetRunning = false;
            return data[0] == 1;
        }

        public bool Get_RobotRemoteMode()
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            byte[] data = new byte[1] { 0 };

            try
            {
                S7MultiVar Reader = new S7MultiVar(this._plc);
                Reader.Add(S7Consts.S7AreaDB, S7Consts.S7WLBit, DB_NUMBER, 48 * 8 + 7, 1, ref data);
                Reader.Read();
            }
            catch (Exception)
            {

            }

            _isSetRunning = false;
            return data[0] == 1;
        }

        public bool Get_PlcEmergency()
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            byte[] data = new byte[1] { 0 };

            try
            {
                S7MultiVar Reader = new S7MultiVar(this._plc);
                Reader.Add(S7Consts.S7AreaDB, S7Consts.S7WLBit, DB_NUMBER, 48 * 8 + 1, 1, ref data);
                Reader.Read();
            }
            catch (Exception)
            {

            }

            _isSetRunning = false;
            return data[0] == 1;
        }

        public bool Get_SystemAuto()
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            byte[] data = new byte[1] { 0 };
            try
            {
                S7MultiVar Reader = new S7MultiVar(this._plc);
                Reader.Add(S7Consts.S7AreaDB, S7Consts.S7WLBit, DB_NUMBER, 14 * 8, 1, ref data);
                Reader.Read();
            }
            catch (Exception)
            {

            }

            _isSetRunning = false;
            return data[0] == 1;
        }

        public bool Get_PltSns_1()
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            byte[] data = new byte[1] { 0 };
            try
            {
                S7MultiVar Reader = new S7MultiVar(this._plc);
                Reader.Add(S7Consts.S7AreaDB, S7Consts.S7WLBit, DB_NUMBER, 54 * 8 + 0, 1, ref data);
                Reader.Read();
            }
            catch (Exception)
            {

            }

            _isSetRunning = false;
            return data[0] == 1;
        }

        public bool Get_PltSns_2()
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            byte[] data = new byte[1] { 0 };
            try
            {
                S7MultiVar Reader = new S7MultiVar(this._plc);
                Reader.Add(S7Consts.S7AreaDB, S7Consts.S7WLBit, DB_NUMBER, 54 * 8 + 1, 1, ref data);
                Reader.Read();
            }
            catch (Exception)
            {

            }

            _isSetRunning = false;
            return data[0] == 1;
        }

        public bool Get_PltSns_3()
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            byte[] data = new byte[1] { 0 };
            try
            {
                S7MultiVar Reader = new S7MultiVar(this._plc);
                Reader.Add(S7Consts.S7AreaDB, S7Consts.S7WLBit, DB_NUMBER, 54 * 8 + 2, 1, ref data);
                Reader.Read();
            }
            catch (Exception)
            {

            }

            _isSetRunning = false;
            return data[0] == 1;
        }

        public bool Get_PltSns_4()
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            byte[] data = new byte[1] { 0 };
            try
            {
                S7MultiVar Reader = new S7MultiVar(this._plc);
                Reader.Add(S7Consts.S7AreaDB, S7Consts.S7WLBit, DB_NUMBER, 54 * 8 + 3, 1, ref data);
                Reader.Read();
            }
            catch (Exception)
            {

            }

            _isSetRunning = false;
            return data[0] == 1;
        }

        public bool Get_PltSns_5()
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            byte[] data = new byte[1] { 0 };
            try
            {
                S7MultiVar Reader = new S7MultiVar(this._plc);
                Reader.Add(S7Consts.S7AreaDB, S7Consts.S7WLBit, DB_NUMBER, 54 * 8 + 4, 1, ref data);
                Reader.Read();
            }
            catch (Exception)
            {

            }

            _isSetRunning = false;
            return data[0] == 1;
        }

        public bool Get_PltSns_6()
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            byte[] data = new byte[1] { 0 };
            try
            {
                S7MultiVar Reader = new S7MultiVar(this._plc);
                Reader.Add(S7Consts.S7AreaDB, S7Consts.S7WLBit, DB_NUMBER, 54 * 8 + 5, 1, ref data);
                Reader.Read();
            }
            catch (Exception)
            {

            }

            _isSetRunning = false;
            return data[0] == 1;
        }

        public bool Get_PltSns_7()
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            byte[] data = new byte[1] { 0 };
            try
            {
                S7MultiVar Reader = new S7MultiVar(this._plc);
                Reader.Add(S7Consts.S7AreaDB, S7Consts.S7WLBit, DB_NUMBER, 54 * 8 + 6, 1, ref data);
                Reader.Read();
            }
            catch (Exception)
            {

            }

            _isSetRunning = false;
            return data[0] == 1;
        }

        // level sensors for the empty pallets
        public bool Get_PltLevelSns_1()
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            byte[] data = new byte[1] { 0 };
            try
            {
                S7MultiVar Reader = new S7MultiVar(this._plc);
                Reader.Add(S7Consts.S7AreaDB, S7Consts.S7WLBit, DB_NUMBER, 54 * 8 + 7, 1, ref data);
                Reader.Read();
            }
            catch (Exception)
            {

            }

            _isSetRunning = false;
            return data[0] == 1;
        }

        public bool Get_PltLevelSns_2()
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            byte[] data = new byte[1] { 0 };
            try
            {
                S7MultiVar Reader = new S7MultiVar(this._plc);
                Reader.Add(S7Consts.S7AreaDB, S7Consts.S7WLBit, DB_NUMBER, 55 * 8 + 0, 1, ref data);
                Reader.Read();
            }
            catch (Exception)
            {

            }

            _isSetRunning = false;
            return data[0] == 1;
        }

        public bool Get_PltLevelSns_3()
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            byte[] data = new byte[1] { 0 };
            try
            {
                S7MultiVar Reader = new S7MultiVar(this._plc);
                Reader.Add(S7Consts.S7AreaDB, S7Consts.S7WLBit, DB_NUMBER, 55 * 8 + 1, 1, ref data);
                Reader.Read();
            }
            catch (Exception)
            {

            }

            _isSetRunning = false;
            return data[0] == 1;
        }

        public bool Get_PltLevelSns_4()
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            byte[] data = new byte[1] { 0 };
            try
            {
                S7MultiVar Reader = new S7MultiVar(this._plc);
                Reader.Add(S7Consts.S7AreaDB, S7Consts.S7WLBit, DB_NUMBER, 55 * 8 + 2, 1, ref data);
                Reader.Read();
            }
            catch (Exception)
            {

            }

            _isSetRunning = false;
            return data[0] == 1;
        }

        public bool Get_PltLevelSns_5()
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            byte[] data = new byte[1] { 0 };
            try
            {
                S7MultiVar Reader = new S7MultiVar(this._plc);
                Reader.Add(S7Consts.S7AreaDB, S7Consts.S7WLBit, DB_NUMBER, 55 * 8 + 3, 1, ref data);
                Reader.Read();
            }
            catch (Exception)
            {

            }

            _isSetRunning = false;
            return data[0] == 1;
        }

        public bool Get_PltLevelSns_6()
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            byte[] data = new byte[1] { 0 };
            try
            {
                S7MultiVar Reader = new S7MultiVar(this._plc);
                Reader.Add(S7Consts.S7AreaDB, S7Consts.S7WLBit, DB_NUMBER, 55 * 8 + 4, 1, ref data);
                Reader.Read();
            }
            catch (Exception)
            {

            }

            _isSetRunning = false;
            return data[0] == 1;
        }

        public bool Get_PltLevelSns_7()
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            byte[] data = new byte[1] { 0 };
            try
            {
                S7MultiVar Reader = new S7MultiVar(this._plc);
                Reader.Add(S7Consts.S7AreaDB, S7Consts.S7WLBit, DB_NUMBER, 55 * 8 + 5, 1, ref data);
                Reader.Read();
            }
            catch (Exception)
            {

            }

            _isSetRunning = false;
            return data[0] == 1;
        }

        public bool Get_RobotRestart()
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            byte[] data = new byte[1] { 0 };
            try
            {
                S7MultiVar Reader = new S7MultiVar(this._plc);
                Reader.Add(S7Consts.S7AreaDB, S7Consts.S7WLBit, DB_NUMBER, 55 * 8 + 6, 1, ref data);
                Reader.Read();
            }
            catch (Exception)
            {

            }

            _isSetRunning = false;
            return data[0] == 1;
        }

        public bool Get_PC_SomeoneInside()
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            byte[] data = new byte[1] { 0 };
            try
            {
                S7MultiVar Reader = new S7MultiVar(this._plc);
                Reader.Add(S7Consts.S7AreaDB, S7Consts.S7WLBit, DB_NUMBER, 55 * 8 + 7, 1, ref data);
                Reader.Read();
            }
            catch (Exception)
            {

            }

            _isSetRunning = false;
            return data[0] == 1;
        }

        public bool Get_PC_RadarStatus()
        {
            while (_isSetRunning)
                ;
            _isSetRunning = true;
            ValidateConnection();

            byte[] data = new byte[1] { 0 };
            try
            {
                S7MultiVar Reader = new S7MultiVar(this._plc);
                Reader.Add(S7Consts.S7AreaDB, S7Consts.S7WLBit, DB_NUMBER, 56 * 8 + 0, 1, ref data);
                Reader.Read();
            }
            catch (Exception)
            {

            }

            _isSetRunning = false;
            return data[0] == 1;
        }
        #endregion

        private async Task _ListenLoop()
        {
            //while (_runListener)
            //{
            //    try
            //    {
            //        if (_isSetRunning)
            //            continue;

            //        ValidateConnection();

            //        _isReading = true;
            //        bool isConnected = this._plc.Connected;
            //        if (!isConnected)
            //        {
            //            int conResult = this._plc.ConnectTo("192.168.0.3", 0, 1);
            //            isConnected = conResult == 0;

            //            if (isConnected && !_stateIsConnected)
            //            {
            //                _stateIsConnected = true;
            //                this.OnPlcConnectionChanged?.Invoke(true);
            //            }
            //            else if (!isConnected && _stateIsConnected)
            //            {
            //                _stateIsConnected = false;
            //                this.OnPlcConnectionChanged?.Invoke(false);
            //            }
            //        }

            //        if (isConnected)
            //        {
            //            S7MultiVar Reader = new S7MultiVar(this._plc);

            //            byte[] DB_HMI = new byte[1024];

            //            Reader.Add(S7Consts.S7AreaDB, S7Consts.S7WLByte, DB_NUMBER, 0, 49, ref DB_HMI);

            //            int Result = Reader.Read();

            //            _db.System_Auto = S7.GetBitAt(DB_HMI, 14, 0);

            //            byte[] test = new byte[8];
            //            int rRes = _plc.ReadArea(S7Consts.S7AreaDB, DB_NUMBER, 14 * 8, 1, 1, test);

            //            _db.Servo_CurrentPos = S7.GetIntAt(DB_HMI, 6);
            //            _db.Servo_Home = S7.GetBitAt(DB_HMI, 0, 0);
            //            _db.Servo_Reset = S7.GetBitAt(DB_HMI, 4, 1);
            //            _db.Servo_TargetPos = S7.GetIntAt(DB_HMI, 2);
            //            _db.Servo_StartFlag = S7.GetBitAt(DB_HMI, 4, 0);
            //            _db.Servo_JogPlus = S7.GetBitAt(DB_HMI, 4, 2);
            //            _db.Servo_JogMinus = S7.GetBitAt(DB_HMI, 4, 3);
            //            _db.Servo_Speed = S7.GetIntAt(DB_HMI, 8);
            //            _db.Servo_PosCam1 = S7.GetIntAt(DB_HMI, 10);
            //            _db.Servo_PosCam2 = S7.GetIntAt(DB_HMI, 12);
            //            _db.RobotPickingOk = S7.GetBitAt(DB_HMI, 32, 0);
            //            _db.RobotPlacingOk = S7.GetBitAt(DB_HMI, 32, 1);
            //            _db.Servo_IsArrived = S7.GetBitAt(DB_HMI, 48, 4);

            //            //this._plc.Disconnect();
            //        }
            //    }
            //    catch (Exception)
            //    {
            //        if (!this._plc.Connected && _stateIsConnected)
            //            this.OnPlcConnectionChanged?.Invoke(false);
            //    }

            //    _isReading = false;

            //    await Task.Delay(200);
            //}
        }
    }
}
