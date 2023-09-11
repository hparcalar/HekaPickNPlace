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
using PickNPlace.DataAccess;
using PickNPlace.DTO;

namespace PickNPlace
{
    /// <summary>
    /// Interaction logic for PalletRecipeWindow.xaml
    /// </summary>
    public partial class PalletRecipeWindow : Window
    {
        public PalletRecipeWindow()
        {
            InitializeComponent();
        }

        private int _recordId;
        private PalletRecipeDTO _recipe;

        private void BindModel()
        {
            this._recipe = new PalletRecipeDTO
            {
                Id = 0,
                Floors = new PalletRecipeFloorDTO[0],
                Explanation = "",
                PalletLength = 0,
                PalletWidth = 0,
                RecipeCode = "",
                TotalFloors = 0,
            };

            // get detailed information from database
            using (HekaDbContext db = SchemaFactory.CreateContext())
            {
                var dbObject = db.PalletRecipe.Where(d => d.Id == _recordId).FirstOrDefault();
                if (dbObject != null)
                {
                    this._recipe.Id = dbObject.Id;
                    this._recipe.Explanation = dbObject.Explanation;
                    this._recipe.PalletLength = dbObject.PalletLength;
                    this._recipe.PalletWidth = dbObject.PalletWidth;
                    this._recipe.RecipeCode = dbObject.RecipeCode;
                    this._recipe.TotalFloors = dbObject.TotalFloors;

                    // fetch floors
                    this._recipe.Floors = db.PalletRecipeFloor.Where(d => d.PalletRecipeId == _recordId)
                            .Select(d => new PalletRecipeFloorDTO
                            {
                                Id = d.Id,
                                PalletRecipeId = d.PalletRecipeId,
                                Cols = d.Cols,
                                FloorNumber = d.FloorNumber,
                                Rows = d.Rows,
                            }).OrderBy(d => d.FloorNumber).ToArray();

                    // fetch floor items (placement rules)
                    foreach (var floor in this._recipe.Floors)
                    {
                        floor.Items = db.PalletRecipeFloorItem.Where(d => d.PalletRecipeFloorId == floor.Id)
                                .Select(d => new PalletRecipeFloorItemDTO
                                {
                                    Id = d.Id,
                                    Col = d.Col,
                                    IsVertical = d.IsVertical,
                                    ItemOrder = d.ItemOrder,
                                    PalletRecipeFloorId = d.PalletRecipeFloorId,
                                    PalletRecipeId = d.PalletRecipeId,
                                    Row = d.Row,
                                }).OrderBy(d => d.ItemOrder).ToArray();
                    }
                }
                else // generate new recipe code
                {
                    var lastRecipe = db.PalletRecipe.OrderByDescending(d => d.RecipeCode).FirstOrDefault();
                    int nextRecipeNo = 1;

                    if (lastRecipe != null)
                    {
                        nextRecipeNo = Convert.ToInt32(lastRecipe.RecipeCode);
                        nextRecipeNo++;
                    }

                    _recipe.RecipeCode = string.Format("{0:000000}", nextRecipeNo);
                }
            }

            // write information to ui
            txtRecipeCode.Text = _recipe.RecipeCode;
            txtExplanation.Text = _recipe.Explanation;

            containerFloors.ItemsSource = _recipe.Floors;
        }

        private void btnSelectRecipe_Click(object sender, RoutedEventArgs e)
        {
            PalletRecipeListWindow wnd = new PalletRecipeListWindow();
            wnd.ShowDialog();

            if (wnd.PalletRecipeId > 0)
            {
                _recordId = wnd.PalletRecipeId;
                this.BindModel();
            }
        }

        private void btnNewRecipe_Click(object sender, RoutedEventArgs e)
        {
            _recordId = 0;
            this.BindModel();
        }

        private void btnDeleteRecipe_Click(object sender, RoutedEventArgs e)
        {
            if (_recordId > 0)
            {
                if (MessageBox.Show("Bu reçeteyi silmek istediğinizden emin misiniz?", "Uyarı", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (HekaDbContext db = SchemaFactory.CreateContext())
                        {
                            var dbObj = db.PalletRecipe.FirstOrDefault(d => d.Id == _recordId);
                            if (dbObj != null)
                            {
                                var floors = db.PalletRecipeFloor.Where(d => d.PalletRecipeId == _recordId).ToArray();
                                var flItems = db.PalletRecipeFloorItem.Where(d => d.PalletRecipeId == _recordId).ToArray();

                                foreach (var item in flItems)
                                {
                                    db.PalletRecipeFloorItem.Remove(item);
                                }

                                foreach (var floor in floors)
                                {
                                    db.PalletRecipeFloor.Remove(floor);
                                }

                                db.PalletRecipe.Remove(dbObj);

                                db.SaveChanges();
                            }
                        }

                        _recordId = 0;
                        this.BindModel();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Uyarı", MessageBoxButton.OK);
                    }
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.BindModel();
        }

        private void btnGoBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnSaveRecipe_Click(object sender, RoutedEventArgs e)
        {
            string errMsg = "";
            try
            {
                using (HekaDbContext db = SchemaFactory.CreateContext())
                {
                    // create if this is a new record
                    var dbObj = db.PalletRecipe.FirstOrDefault(d => d.Id == _recordId);
                    if (dbObj == null)
                    {
                        dbObj = new PalletRecipe
                        {
                            RecipeCode = txtRecipeCode.Text,
                        };
                        db.PalletRecipe.Add(dbObj);
                    }

                    // save header information
                    dbObj.Explanation = txtExplanation.Text;
                    dbObj.PalletLength = 100;
                    dbObj.PalletWidth = 120;
                    dbObj.TotalFloors = _recipe.Floors.Length;

                    // save floors
                    var oldFloors = db.PalletRecipeFloor.Where(d => d.PalletRecipeId == dbObj.Id).ToArray();
                    foreach (var floor in oldFloors)
                    {
                        var oldFloorItems = db.PalletRecipeFloorItem.Where(d => d.PalletRecipeFloorId == floor.Id).ToArray();
                        foreach (var item in oldFloorItems)
                        {
                            db.PalletRecipeFloorItem.Remove(item);
                        }

                        db.PalletRecipeFloor.Remove(floor);
                    }

                    foreach (var floor in _recipe.Floors)
                    {
                        var dbFloor = new PalletRecipeFloor
                        {
                            Cols = floor.Cols,
                            FloorNumber = floor.FloorNumber,
                            PalletRecipe = dbObj,
                            Rows = floor.Rows,
                        };

                        db.PalletRecipeFloor.Add(dbFloor);

                        foreach (var item in floor.Items)
                        {
                            var dbItem = new PalletRecipeFloorItem
                            {
                                PalletRecipeFloor = dbFloor,
                                PalletRecipe = dbObj,
                                Col = item.Col,
                                IsVertical = item.IsVertical,
                                ItemOrder = item.ItemOrder,
                                Row = item.Row,
                            };

                            db.PalletRecipeFloorItem.Add(dbItem);
                        }
                    }

                    db.SaveChanges();

                    _recordId = dbObj.Id;
                    this.BindModel();
                }
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;

                MessageBox.Show(errMsg, "Uyarı", MessageBoxButton.OK);
            }
        }

        private void btnNewFloor_Click(object sender, RoutedEventArgs e)
        {
            var list = _recipe.Floors.ToList();

            int nextFloorNo = 1;
            if (list.Any())
            {
                nextFloorNo = list.Max(m => m.FloorNumber) + 1;
            }

            list.Add(new PalletRecipeFloorDTO
            {
                Cols = 3,
                Rows = 2,
                FloorNumber = nextFloorNo,
                Items = new PalletRecipeFloorItemDTO[0],
            });

            _recipe.Floors = list.ToArray();
            containerFloors.ItemsSource = _recipe.Floors;
        }
    }
}
