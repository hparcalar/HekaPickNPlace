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
using PickNPlace.DTO;
using PickNPlace.DataAccess;

namespace PickNPlace
{
    /// <summary>
    /// Interaction logic for PalletRecipeListWindow.xaml
    /// </summary>
    public partial class PalletRecipeListWindow : Window
    {
        public PalletRecipeListWindow()
        {
            InitializeComponent();
        }

        public int PalletRecipeId { get; set; }

        PalletRecipeDTO[] _listData = new PalletRecipeDTO[0];

        private void BindList()
        {
            using (HekaDbContext db = SchemaFactory.CreateContext())
            {
                _listData = db.PalletRecipe.Select(d => new PalletRecipeDTO
                {
                    Id = d.Id,
                    Explanation = d.Explanation,
                    TotalFloors = d.TotalFloors,
                    PalletWidth = d.PalletWidth,
                    PalletLength = d.PalletLength,
                    RecipeCode = d.RecipeCode,
                }).OrderByDescending(d => d.Id).ToArray();
            }

            dgRecipe.ItemsSource = _listData;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.BindList();
        }

        private void btnSelectRecipe_Click(object sender, RoutedEventArgs e)
        {
            var selectedRecipe = dgRecipe.SelectedItem as PalletRecipeDTO;
            if (selectedRecipe != null)
            {
                this.PalletRecipeId = selectedRecipe.Id;
                this.Close();
            }
        }
    }
}
