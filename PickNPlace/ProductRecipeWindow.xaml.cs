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

        private int _recordId;
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

            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void btnDeleteRecipe_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnGoBack_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSelectRecipe_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnNewRecipe_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSaveRecipe_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
