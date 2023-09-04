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
    /// Interaction logic for MaterialPallet.xaml
    /// </summary>
    public partial class MaterialPallet : UserControl
    {
        public MaterialPallet()
        {
            InitializeComponent();
        }

        // events
        public delegate void PickingStatusChangeEvent(int palletNo);
        public event PickingStatusChangeEvent OnPickingStatusChanged;

        // public properties
        public int PalletNo
        {
            get { return (int)GetValue(PalletNoProperty); }
            set { SetValue(PalletNoProperty, value); }
        }
        public static readonly DependencyProperty PalletNoProperty =
            DependencyProperty.Register("PalletNo", typeof(int),
            typeof(MaterialPallet), new PropertyMetadata(0)
        );

        public string PickingText
        {
            get { return (string)GetValue(PickingTextProperty); }
            set { SetValue(PickingTextProperty, value); }
        }
        public static readonly DependencyProperty PickingTextProperty =
            DependencyProperty.Register("PickingText", typeof(string),
            typeof(MaterialPallet), new PropertyMetadata("")
        );

        public string PickingColor
        {
            get { return (string)GetValue(PickingColorProperty); }
            set { SetValue(PickingColorProperty, value); }
        }
        public static readonly DependencyProperty PickingColorProperty =
            DependencyProperty.Register("PickingColor", typeof(string),
            typeof(MaterialPallet), new PropertyMetadata("")
        );

        // handlers
        private void btnPickingStatus_Click(object sender, RoutedEventArgs e)
        {
            OnPickingStatusChanged?.Invoke(this.PalletNo);
        }
    }
}
