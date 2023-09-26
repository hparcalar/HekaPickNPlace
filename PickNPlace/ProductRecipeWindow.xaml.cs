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
    /// Interaction logic for ProductRecipeWindow.xaml
    /// </summary>
    public partial class ProductRecipeWindow : Window
    {
        public ProductRecipeWindow()
        {
            InitializeComponent();
        }

        private int _recordId = 0;
        private PlaceRequestDTO _recipe;

        private void BindModel()
        {
            this._recipe = new PlaceRequestDTO
            {
                Id = 0,
                BatchCount = 0,
                RecipeCode = "",
                RecipeName = "",
                RequestNo = "",
                Items = null,
            };

            // get detailed information from database
            using (HekaDbContext db = SchemaFactory.CreateContext())
            {
                var dbObject = db.PlaceRequest.Where(d => d.Id == _recordId).FirstOrDefault();
                if (dbObject != null)
                {
                    this._recipe.Id = dbObject.Id;
                    _recipe.RecipeCode = dbObject.RecipeCode;
                    _recipe.RequestNo = dbObject.RequestNo;
                    _recipe.RecipeName = dbObject.RecipeName;
                    _recipe.Items = db.PlaceRequestItem.Where(d => d.PlaceRequestId == _recordId)
                        .Select(d => new PlaceRequestItemDTO
                        {
                            ItemCode = d.RawMaterial != null ? d.RawMaterial.ItemCode : "",
                            ItemName = d.RawMaterial != null ? d.RawMaterial.ItemName : "",
                            PiecesPerBatch = d.PiecesPerBatch,
                            PlaceRequestId = d.PlaceRequestId,
                            RawMaterialId = d.RawMaterialId,
                            Id = d.Id,
                        }).ToArray();
                }
                else // generate new recipe code
                {
                    var lastRecipe = db.PlaceRequest.OrderByDescending(d => d.RequestNo).FirstOrDefault();
                    int nextRecipeNo = 1;

                    if (lastRecipe != null)
                    {
                        nextRecipeNo = Convert.ToInt32(lastRecipe.RequestNo);
                        nextRecipeNo++;
                    }

                    _recipe.RequestNo = string.Format("{0:000000}", nextRecipeNo);
                }
            }

            // write information to ui
            txtRecipeCode.Text = _recipe.RequestNo;
            txtExplanation.Text = _recipe.RecipeName;

            dgDetails.ItemsSource = _recipe.Items;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BindModel();
        }

        private void btnDeleteRecipe_Click(object sender, RoutedEventArgs e)
        {
            if (_recipe != null && _recipe.Id > 0)
            {
                if (MessageBox.Show("Bu reçeteyi silmek istediğinizden emin misiniz?","Uyarı", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    using (HekaDbContext db = SchemaFactory.CreateContext())
                    {
                        var dbObj = db.PlaceRequest.FirstOrDefault(d => d.Id == _recipe.Id);
                        if (dbObj != null)
                        {
                            var oldItems = db.PlaceRequestItem.Where(d => d.PlaceRequestId == dbObj.Id).ToArray();
                            foreach (var item in oldItems)
                            {
                                db.PlaceRequestItem.Remove(item);
                            }

                            db.PlaceRequest.Remove(dbObj);
                        }

                        db.SaveChanges();
                    }
                }
            }
        }

        private void btnGoBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnSelectRecipe_Click(object sender, RoutedEventArgs e)
        {
            ProductRecipeListWindow wnd = new ProductRecipeListWindow();
            wnd.ShowDialog();

            if (wnd.RecipeId > 0)
            {
                this._recordId = wnd.RecipeId;
                this.BindModel();
            }
        }

        private void btnNewRecipe_Click(object sender, RoutedEventArgs e)
        {
            _recordId = 0;
            this.BindModel();
        }

        private void btnSaveRecipe_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (HekaDbContext db = SchemaFactory.CreateContext())
                {
                    var dbObj = db.PlaceRequest.FirstOrDefault(d => d.Id == _recipe.Id);
                    if (dbObj == null)
                    {
                        dbObj = new PlaceRequest
                        {

                        };
                        db.PlaceRequest.Add(dbObj);
                    }

                    dbObj.RecipeCode = txtRecipeCode.Text;
                    dbObj.RequestNo = txtRecipeCode.Text;
                    dbObj.RecipeName = txtExplanation.Text;

                    // clear existing items
                    var oldItems = db.PlaceRequestItem.Where(d => d.PlaceRequestId == dbObj.Id).ToArray();
                    foreach (var item in oldItems)
                    {
                        db.PlaceRequestItem.Remove(item);
                    }

                    List<RawMaterial> _currentlyNewItems = new List<RawMaterial>();

                    // add new items
                    foreach (var item in _recipe.Items)
                    {
                        var properItem = db.RawMaterial.FirstOrDefault(d => d.ItemCode == item.ItemCode);
                        if (properItem == null)
                        {
                            properItem = _currentlyNewItems.FirstOrDefault(d => d.ItemCode == item.ItemCode);
                        }
                        if (properItem == null)
                        {
                            properItem = new RawMaterial
                            {
                                ItemCode = item.ItemCode,
                                ItemName = item.ItemName,
                            };
                            db.RawMaterial.Add(properItem);
                            _currentlyNewItems.Add(properItem);
                        }

                        var dbItem = new PlaceRequestItem
                        {
                            PlaceRequest = dbObj,
                            PiecesPerBatch = item.PiecesPerBatch,
                            RawMaterial = properItem,
                        };
                        db.PlaceRequestItem.Add(dbItem);
                    }

                    db.SaveChanges();
                    _recordId = dbObj.Id;
                }

                MessageBox.Show("KAYIT BAŞARILI", "Bilgilendirme", MessageBoxButton.OK);
            }
            catch (Exception)
            {

            }

            this.BindModel();
        }

        private void btnNewProduct_Click(object sender, RoutedEventArgs e)
        {
            var currentList = _recipe.Items != null ? _recipe.Items.ToList() : (new List<PlaceRequestItemDTO>());
            currentList.Add(new PlaceRequestItemDTO
            {
                ItemCode = "",
                ItemName = "",
                PiecesPerBatch = 1,
            });
            _recipe.Items = currentList.ToArray();
            dgDetails.ItemsSource = _recipe.Items;
        }
    }
}
