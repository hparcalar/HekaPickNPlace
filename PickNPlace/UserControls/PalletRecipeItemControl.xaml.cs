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
using System.Windows.Navigation;
using System.Windows.Shapes;
using PickNPlace.DTO;

namespace PickNPlace.UserControls
{
    /// <summary>
    /// Interaction logic for PalletRecipeItemControl.xaml
    /// </summary>
    public partial class PalletRecipeItemControl : UserControl
    {
        public PalletRecipeItemControl()
        {
            InitializeComponent();
        }

        public PalletRecipeFloorDTO Floor
        {
            get { return (PalletRecipeFloorDTO)GetValue(FloorProperty); }
            set { SetValue(FloorProperty, value); }
        }
        public static readonly DependencyProperty FloorProperty =
            DependencyProperty.Register("Floor", typeof(PalletRecipeFloorDTO),
            typeof(PalletRecipeItemControl), new PropertyMetadata(default(PalletRecipeFloorDTO))
        );

        private void btnNewProductRow1_Click(object sender, RoutedEventArgs e)
        {
            if (Floor != null)
            {
                if (Floor.Items == null)
                {
                    Floor.Items = new PalletRecipeFloorItemDTO[0];
                }

                // calculate next item placement order
                var maxOrderNumber = 1;

                if (Floor.Items.Any())
                {
                    maxOrderNumber = Floor.Items.Max(d => d.ItemOrder) ?? 0;
                    maxOrderNumber++;
                }

                // calculate next column number of current row
                var nextColNumber = 1;
                if (Floor.Items.Where(d => d.Row == 1).Any())
                {
                    nextColNumber = Floor.Items.Where(d => d.Row == 1).Max(d => d.Col) ?? 0;
                    nextColNumber++;
                }

                var list = Floor.Items.ToList();
                list.Add(new PalletRecipeFloorItemDTO
                {
                    ItemOrder = maxOrderNumber,
                    Col = nextColNumber,
                    IsVertical = true,
                    Row = 1,
                });

                Floor.Items = list.ToArray();

                this.BindItems();
            }
        }

        private void BindItems()
        {
            if (Floor != null && Floor.Items != null)
            {
                containerPalletUp.ItemsSource = Floor.Items.Where(d => d.Row == 2).ToArray();
                containerPalletDown.ItemsSource = Floor.Items.Where(d => d.Row == 1).ToArray();
            }
        }

        private void btnNewProductRow2_Click(object sender, RoutedEventArgs e)
        {
            if (Floor != null)
            {
                if (Floor.Items == null)
                {
                    Floor.Items = new PalletRecipeFloorItemDTO[0];
                }

                // calculate next item placement order
                var maxOrderNumber = 1;

                if (Floor.Items.Any())
                {
                    maxOrderNumber = Floor.Items.Max(d => d.ItemOrder) ?? 0;
                    maxOrderNumber++;
                }

                // calculate next column number of current row
                var nextColNumber = 1;
                if (Floor.Items.Where(d => d.Row == 2).Any())
                {
                    nextColNumber = Floor.Items.Where(d => d.Row == 2).Max(d => d.Col) ?? 0;
                    nextColNumber++;
                }

                var list = Floor.Items.ToList();
                list.Add(new PalletRecipeFloorItemDTO
                {
                    ItemOrder = maxOrderNumber,
                    Col = nextColNumber,
                    IsVertical = false,
                    Row = 2,
                });

                Floor.Items = list.ToArray();

                this.BindItems();
            }
        }

        private void btnCellRow1_Click(object sender, RoutedEventArgs e)
        {
            int colNo = (int)((e.Source as Button).Tag);
            var targetObj = Floor.Items.FirstOrDefault(d => d.Row == 1 && d.Col == colNo);
            if (targetObj != null)
            {
                targetObj.IsVertical = !targetObj.IsVertical;
            }

            var imageObj = (e.Source as Button).Content as Image;
            if (imageObj != null)
            {
                imageObj.Source = new BitmapImage(new Uri(targetObj.IsVertical ? "/UserControls/sack-jotun-horz.png" : "/UserControls/sack-jotun-vertical.png", UriKind.Relative));
            }
        }

        private void btnCellRow2_Click(object sender, RoutedEventArgs e)
        {
            int colNo = (int)((e.Source as Button).Tag);
            var targetObj = Floor.Items.FirstOrDefault(d => d.Row == 2 && d.Col == colNo);
            if (targetObj != null)
            {
                targetObj.IsVertical = !targetObj.IsVertical;
            }

            var imageObj = (e.Source as Button).Content as Image;
            if (imageObj != null)
            {
                imageObj.Source = new BitmapImage(new Uri(targetObj.IsVertical ? "/UserControls/sack-jotun-horz.png" : "/UserControls/sack-jotun-vertical.png", UriKind.Relative));
            }
        }

        private void PalletFloorWnd_Loaded(object sender, RoutedEventArgs e)
        {
            this.BindItems();
        }

        private void btnCellRow1_Initialized(object sender, EventArgs e)
        {
            var flItems = Floor.Items.Where(d => d.Row == 1).Count();
            var btn = (sender as Button);
            btn.Width = pnlPalletDown.RenderSize.Width / flItems;
        }

        private void btnCellRow2_Initialized(object sender, EventArgs e)
        {
            var flItems = Floor.Items.Where(d => d.Row == 2).Count();
            var btn = (sender as Button);
            btn.Width = pnlPalletUp.RenderSize.Width / flItems;
        }
    }
}
