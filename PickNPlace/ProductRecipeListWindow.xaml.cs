using PickNPlace.DataAccess;
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

namespace PickNPlace
{
    /// <summary>
    /// Interaction logic for ProductRecipeListWindow.xaml
    /// </summary>
    public partial class ProductRecipeListWindow : Window
    {
        public ProductRecipeListWindow()
        {
            InitializeComponent();
        }

        public int RecipeId { get; set; }

        PlaceRequestDTO[] _listData = new PlaceRequestDTO[0];

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.BindList();
        }

        private void BindList()
        {
            using (HekaDbContext db = SchemaFactory.CreateContext())
            {
                _listData = db.PlaceRequest.Select(d => new PlaceRequestDTO
                {
                    Id = d.Id,
                    BatchCount = d.BatchCount,
                    RecipeCode = d.RecipeCode,
                    RecipeName = d.RecipeName,
                    RequestNo = d.RequestNo,
                }).OrderByDescending(d => d.Id).ToArray();
            }

            dgRecipe.ItemsSource = _listData;
        }

        private void btnSelectRecipe_Click(object sender, RoutedEventArgs e)
        {
            var selectedRecipe = dgRecipe.SelectedItem as PlaceRequestDTO;
            if (selectedRecipe != null)
            {
                this.RecipeId = selectedRecipe.Id;
                this.Close();
            }
        }

        private void dgRecipe_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var recipe = dgRecipe.SelectedItem as PlaceRequestDTO;
            if (recipe != null)
            {
                using (HekaDbContext db = SchemaFactory.CreateContext())
                {
                    var items = db.PlaceRequestItem.Where(d => d.PlaceRequestId == recipe.Id)
                        .Select(d => new PlaceRequestItemDTO
                        {
                            Id = d.Id,
                            ItemCode = d.RawMaterial != null ? d.RawMaterial.ItemCode : "",
                            ItemName = d.RawMaterial != null ? d.RawMaterial.ItemName : "",
                            PiecesPerBatch = d.PiecesPerBatch,
                            PlaceRequestId = d.PlaceRequestId,
                            RawMaterialId = d.RawMaterialId,
                        }).ToArray();

                    dgItems.ItemsSource = items;
                }
            }
            else
                dgItems.ItemsSource = null;
        }
    }
}
