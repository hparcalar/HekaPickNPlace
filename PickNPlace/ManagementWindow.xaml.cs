using PickNPlace.Plc;
using PickNPlace.Plc.Data;
using PickNPlace.DataAccess;
using PickNPlace.Camera;
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
using System.Windows.Shapes;
using PickNPlace.Business;

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
                this._plc.Set_PlaceCalculationOk(0);

                string posRaw = camera.GetVisionTargets(txtCameraProgramId.Text);
                txtCameraVisionResult.Text = posRaw;

                int[] posData = new int[0];

                try
                {
                    posData = (posRaw.Split(',')).Skip(5).Take(6).Select(d => Convert.ToInt32(Convert.ToSingle(d))).ToArray();
                }
                catch (Exception)
                {

                }

                if (posData.Length > 0)
                {
                    this._plc.Set_RobotX(posData[0]);
                    this._plc.Set_RobotY(posData[1]);
                    this._plc.Set_RobotZ(posData[2]);
                    this._plc.Set_RobotRX(posData[3]);
                    this._plc.Set_RobotRY(posData[4]);
                    this._plc.Set_RobotRZ(posData[5]);

                    this._plc.Set_CaptureOk(1);
                }
            }
        }

        // temporary placing variables
        private int _currentFloor = 1;
        private int _currentItemNo = 0;

        private PalletRecipeDTO _palletRecipe = new PalletRecipeDTO
        {
            Explanation = "Test",
            PalletWidth = 120, // x
            PalletLength = 100, // y
            RecipeCode = "0001",
            TotalFloors = 3,
            Floors = new PalletRecipeFloorDTO[]
            {
                // 1. floor
                new PalletRecipeFloorDTO
                {
                    FloorNumber = 1,
                    Rows = 2,
                    Cols = 3,
                    Items = new PalletRecipeFloorItemDTO[]
                    {
                        // 1. item
                        new PalletRecipeFloorItemDTO
                        {
                            ItemOrder = 1,
                            Row = 1,
                            Col = 1,
                            IsVertical = true,
                        },
                        // 2. item
                        new PalletRecipeFloorItemDTO
                        {
                            ItemOrder = 2,
                            IsVertical = true,
                            Row = 1,
                            Col = 2,
                        },
                        // 3. item
                        new PalletRecipeFloorItemDTO
                        {
                            ItemOrder = 3,
                            IsVertical = false,
                            Row = 2,
                            Col = 1,
                        },
                        // 4. item
                        new PalletRecipeFloorItemDTO
                        {
                            ItemOrder = 4,
                            IsVertical = false,
                            Row = 2,
                            Col = 2,
                        },
                        // 5.item
                        new PalletRecipeFloorItemDTO
                        {
                            ItemOrder = 5,
                            IsVertical = false,
                            Row=2,
                            Col=3,
                        }
                    },
                },
                // 2. floor
                new PalletRecipeFloorDTO
                {
                    FloorNumber = 2,
                    Rows = 2,
                    Cols = 3,
                    Items = new PalletRecipeFloorItemDTO[]
                    {
                        // 6. item
                        new PalletRecipeFloorItemDTO
                        {
                            ItemOrder = 6,
                            IsVertical = false,
                            Row = 1,
                            Col = 1,
                        },
                        // 7. item
                        new PalletRecipeFloorItemDTO
                        {
                            ItemOrder = 7,
                            IsVertical = false,
                            Row = 1,
                            Col = 2,
                        },
                        // 8. item
                        new PalletRecipeFloorItemDTO
                        {
                            ItemOrder = 8,
                            IsVertical = false,
                            Row = 1,
                            Col = 3,
                        },
                        // 9. item
                        new PalletRecipeFloorItemDTO
                        {
                            ItemOrder = 9,
                            IsVertical = true,
                            Row = 2,
                            Col = 1,
                        },
                        // 10. item
                        new PalletRecipeFloorItemDTO
                        {
                            ItemOrder = 10,
                            IsVertical = true,
                            Row = 2,
                            Col = 2,
                        }
                    }
                }
            },
        };

        private RobotPositionDTO CalculateCurrentItemDropPosition()
        {
            RobotPositionDTO pos = null;

            try
            {
                var floor = _palletRecipe.Floors.FirstOrDefault(d => d.FloorNumber == _currentFloor);
                if (floor != null)
                {
                    var item = floor.Items.FirstOrDefault(d => d.ItemOrder == _currentItemNo);
                    if (item != null)
                    {
                        var totalItemsOfCurrentRow = floor.Items.Where(d => d.Row == item.Row).Count();
                        var totalRowsOfCurrentFloor = floor.Rows ?? 1;

                        pos = new RobotPositionDTO
                        {
                            X = ((_palletRecipe.PalletWidth / totalItemsOfCurrentRow) * ((item.Col ?? 1) - 1)) + (_palletRecipe.PalletWidth / totalItemsOfCurrentRow / 2),
                            Y =  ( (_palletRecipe.PalletLength / totalRowsOfCurrentFloor) * ((item.Row ?? 1) - 1) ) + (_palletRecipe.PalletLength / totalRowsOfCurrentFloor / 2),
                            Z = _currentFloor * 15,
                            RX = 0,
                            RY = 0,
                            RZ = item.IsVertical ? 178 : -102,
                        };

                        pos.X *= 10;
                        pos.Y *= 10;
                        pos.Z *= 10;
                    }
                }
            }
            catch (Exception)
            {
                pos = null;
            }

            return pos;
        }

        private void btnPlaceAnItem_Click(object sender, RoutedEventArgs e)
        {
            this._plc.Set_CaptureOk(0);

            _currentItemNo++;
            if (_currentItemNo == 6)
            {
                _currentFloor = 2;
            }

            var pos = CalculateCurrentItemDropPosition();
            if (pos != null)
            {
                this._plc.Set_RobotX(pos.X);
                this._plc.Set_RobotY(pos.Y);
                this._plc.Set_RobotZ(pos.Z);
                this._plc.Set_RobotRX(pos.RX);
                this._plc.Set_RobotRY(pos.RY);
                this._plc.Set_RobotRZ(pos.RZ);

                this._plc.Set_EmptyPalletNo(3);
                this._plc.Set_PlaceCalculationOk(1);
            }
            else
            {
                _currentFloor = 1;
                _currentItemNo = 0;
            }
        }


        PlaceRequestDTO _placeReq = new PlaceRequestDTO
        {
            BatchCount = 10,
            Items = new PlaceRequestItemDTO[]
            {
                new PlaceRequestItemDTO { ItemCode = "37623SB", PiecesPerBatch = 25, SackType = 1, },
                new PlaceRequestItemDTO { ItemCode = "00727SB", PiecesPerBatch = 8, SackType = 2, },
                new PlaceRequestItemDTO { ItemCode = "37326SB", PiecesPerBatch = 4, SackType = 3 },
            },
        };

        HkAutoLogic logic = null;
        int _pickingItemOrder = 1;
        int _nextItemIndex = 0;
        private void btnAutoCalcTest_Click(object sender, RoutedEventArgs e)
        {
            // first recipe setup for logic test
            if (logic == null)
            {
                logic = new HkAutoLogic();

                // set order recipe for the pallet no. 3
                logic.SetRequestForPallet(_placeReq, 3);
            }

            PlaceRequestItemDTO nextItem = null;
            foreach (var pack in _placeReq.Items)
            {
                if (pack.PiecesPerBatch > _pickingItemOrder)
                {
                    nextItem = pack;
                    break;
                }
                else
                {
                    _nextItemIndex = 1;
                }
            }

            nextItem = _placeReq.Items[_nextItemIndex];

            if (nextItem != null)
            {
                // try place an item from a raw pallet
                int placeResult = logic.PlaceAnItem(3, nextItem.ItemCode, nextItem.SackType ?? 0);

                if (placeResult == 0) // successfull
                {
                    var posItem = logic.GetWaitingItem(3);
                    var currentFloor = logic.GetCurrentFloor(3);

                    this._plc.Set_RobotX(posItem.PlacedX);
                    this._plc.Set_RobotY(posItem.PlacedY);
                    this._plc.Set_RobotZ(currentFloor * 9 * 10 * -1);
                    this._plc.Set_RobotRX(0);
                    this._plc.Set_RobotRY(0);

                    this._plc.Set_RobotRZ(posItem.IsRotated ? -179 : -89);

                    this._plc.Set_EmptyPalletNo(3);
                    this._plc.Set_PlaceCalculationOk(1);

                    logic.SignWaitingPlacementIsMade(3);

                    // move cursor to next item when event will be triggered again
                    _pickingItemOrder++;
                }
                else // an error occured
                {

                }
            }

            var pltData = logic.GetPalletData(3);
            if (pltData != null)
            {
                int xx = 5;
            }
        }
    }
}
