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

namespace PickNPlace.UserControls
{
    /// <summary>
    /// Interaction logic for EmptyPallet.xaml
    /// </summary>
    public partial class EmptyPallet : UserControl
    {
        public EmptyPallet()
        {
            InitializeComponent();
        }

        public int PalletNo
        {
            get { return (int)GetValue(PalletNoProperty); }
            set { SetValue(PalletNoProperty, value); }
        }
        public static readonly DependencyProperty PalletNoProperty =
            DependencyProperty.Register("PalletNo", typeof(int),
            typeof(EmptyPallet), new PropertyMetadata(0)
        );
    }
}
