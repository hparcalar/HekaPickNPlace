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

        public bool HoldReading { get; set; }

        public bool Set_ServoHome { get; set; }
        public bool Set_ServoTargetPos { get; set; }
        public bool Set_ServoReset { get; set; }
        public bool Set_ServoStartFlag { get; set; }
        public bool Set_ServoJogPlus { get; set; }
        public bool Set_ServoJogMinus { get; set; }

        public void Start()
        {
            if (!_runListener)
            {
                _Listener = Task.Run(this._ListenLoop);
                _runListener = true;
            }
        }

        public void Stop()
        {
            _runListener = false;

            try
            {
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

        private async Task _ListenLoop()
        {
            while (_runListener)
            {
                try
                {
                    bool isConnected = this._plc.Connected;
                    if (!isConnected)
                    {
                        int conResult = this._plc.ConnectTo("192.168.0.3", 0, 0);
                        isConnected = conResult == 0;

                        if (isConnected && !_stateIsConnected)
                        {
                            _stateIsConnected = true;
                            this.OnPlcConnectionChanged?.Invoke(true);
                        }
                        else if (!isConnected && _stateIsConnected)
                        {
                            _stateIsConnected = false;
                            this.OnPlcConnectionChanged?.Invoke(false);
                        }
                    }

                    if (isConnected)
                    {
                        S7MultiVar Reader = new S7MultiVar(this._plc);
                        S7MultiVar Writer = new S7MultiVar(this._plc);

                        byte[] DB_HMI = new byte[1024];
                        byte[] DB_HMI_WR = new byte[1024];
                        int DBNumber_HMI = 2;

                        Reader.Add(S7Consts.S7AreaDB, S7Consts.S7WLByte, DBNumber_HMI, 0, 8, ref DB_HMI);
                        Writer.Add(S7Consts.S7AreaDB, S7Consts.S7WLByte, DBNumber_HMI, 0, 6, ref DB_HMI_WR);

                        // write datablock
                        bool doWrite = false;

                        if (Set_ServoHome)
                        {
                            S7.SetBitAt(ref DB_HMI_WR, 0, 0, _db.Servo_Home);
                            this.Set_ServoHome = false;

                            doWrite = true;
                        }

                        if (Set_ServoReset) {
                            S7.SetBitAt(ref DB_HMI_WR, 4, 1, _db.Servo_Reset);
                            this.Set_ServoReset = false;

                            doWrite = true;
                        }

                        if (Set_ServoTargetPos)
                        {
                            S7.SetIntAt(DB_HMI_WR, 2, (short)_db.Servo_TargetPos);
                            this.Set_ServoTargetPos = false;

                            doWrite = true;
                        }

                        if (Set_ServoStartFlag)
                        {
                            S7.SetBitAt(ref DB_HMI_WR, 4, 0, _db.Servo_StartFlag);
                            this.Set_ServoStartFlag = false;

                            doWrite = true;
                        }

                        if (Set_ServoJogPlus)
                        {
                            S7.SetBitAt(ref DB_HMI_WR, 4, 2, _db.Servo_JogPlus);
                            this.Set_ServoJogPlus = false;

                            doWrite = true;
                        }

                        if (Set_ServoJogMinus)
                        {
                            S7.SetBitAt(ref DB_HMI_WR, 4, 3, _db.Servo_JogMinus);
                            this.Set_ServoJogMinus = false;

                            doWrite = true;
                        }

                        if (doWrite)
                        {
                            Writer.Write();
                        }

                        int Result = Reader.Read();

                        // read datablock
                        if (!HoldReading)
                        {
                            _db.Servo_CurrentPos = S7.GetIntAt(DB_HMI, 6);
                            _db.Servo_Home = S7.GetBitAt(DB_HMI, 0, 0);
                            _db.Servo_Reset = S7.GetBitAt(DB_HMI, 4, 1);
                            _db.Servo_TargetPos = S7.GetIntAt(DB_HMI, 2);
                            _db.Servo_StartFlag = S7.GetBitAt(DB_HMI, 4, 0);
                            _db.Servo_JogPlus = S7.GetBitAt(DB_HMI, 4, 2);
                            _db.Servo_JogMinus = S7.GetBitAt(DB_HMI, 4, 3);
                        }

                        //this._plc.Disconnect();
                    }
                }
                catch (Exception)
                {
                    if (!this._plc.Connected && _stateIsConnected)
                        this.OnPlcConnectionChanged?.Invoke(false);
                }

                await Task.Delay(50);
            }
        }
    }
}
