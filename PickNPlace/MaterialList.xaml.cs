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
    /// Interaction logic for MaterialList.xaml
    /// </summary>
    public partial class MaterialList : Window
    {
        public MaterialList()
        {
            InitializeComponent();
        }

        // outputs
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public int SackType { get; set; }

        // inputs
        public string RequestNo { get; set; }

        private void btnSelectMaterial_Click(object sender, RoutedEventArgs e)
        {
            var selectedRecipe = dgRecipe.SelectedItem as PlaceRequestItemDTO;
            var selectedSackType = dgSackType.SelectedItem as SackTypeDTO;

            if (selectedRecipe != null && selectedSackType != null)
            {
                this.ItemCode = selectedRecipe.ItemCode;
                this.ItemName = selectedRecipe.ItemName;
                this.SackType = selectedSackType.SackType;

                this.Close();
            }
        }

        private void BindList()
        {
            var _listData = new PlaceRequestItemDTO[0];

            using (HekaDbContext db = SchemaFactory.CreateContext())
            {
                _listData = db.PlaceRequestItem.Where(d => d.PlaceRequest.RequestNo == RequestNo)
                    .Select(d => new PlaceRequestItemDTO
                {
                    Id = d.Id,
                    PiecesPerBatch = d.PiecesPerBatch,
                    RawMaterialId = d.RawMaterialId,
                    ItemCode = d.RawMaterial != null ? d.RawMaterial.ItemCode : "",
                    ItemName = d.RawMaterial != null ? d.RawMaterial.ItemName : "",
                    PlaceRequestId = d.PlaceRequestId,
                }).OrderByDescending(d => d.Id).ToArray();
            }

            dgRecipe.ItemsSource = _listData;

            dgSackType.ItemsSource = new SackTypeDTO[]
            {
                new SackTypeDTO{ SackType = 1, Explanation="40x60" },
                new SackTypeDTO{ SackType = 2, Explanation="30x50" },
                new SackTypeDTO{ SackType = 3, Explanation="50x70" },
            };

            // sign existing selections and show them to the user
            if (!string.IsNullOrEmpty(ItemCode))
            {
                var currentOne = _listData.FirstOrDefault(d => d.ItemCode == ItemCode);
                if (currentOne != null)
                {
                    dgRecipe.SelectedItem = currentOne;
                }
            }

            if (SackType > 0)
            {
                var currentOne = (dgSackType.ItemsSource as SackTypeDTO[]).FirstOrDefault(d => d.SackType == SackType);
                if (currentOne != null)
                {
                    dgSackType.SelectedItem = currentOne;
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.BindList();
        }
    }
}
