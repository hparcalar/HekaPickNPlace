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
    /// Interaction logic for PalletRecipeWindow.xaml
    /// </summary>
    public partial class PalletRecipeWindow : Window
    {
        public PalletRecipeWindow()
        {
            InitializeComponent();
        }

        public int[] Floors
        {
            get { return (int[])GetValue(FloorsProperty); }
            set { SetValue(FloorsProperty, value); }
        }
        public static readonly DependencyProperty FloorsProperty =
            DependencyProperty.Register("Floors", typeof(int[]),
            typeof(PalletRecipeWindow), new PropertyMetadata(new int[0])
        );

        private void btnSelectRecipe_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnNewRecipe_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnDeleteRecipe_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Floors = new int[] { 1, 2, 3, 4 };
        }
    }
}
