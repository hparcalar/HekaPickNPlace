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
    /// Interaction logic for MaterialPalletDown.xaml
    /// </summary>
    public partial class MaterialPalletDown : UserControl
    {
        public delegate void PalletEnabledChanged(int palletNo, bool enabled);
        public event PalletEnabledChanged OnPalletEnabledChanged;

        public delegate void SelectRecipeSignal(int palletNo);
        public event SelectRecipeSignal OnSelectRecipeSignal;

        public MaterialPalletDown()
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
            typeof(MaterialPalletDown), new PropertyMetadata(0)
        );

        public string PickingText
        {
            get { return (string)GetValue(PickingTextProperty); }
            set { SetValue(PickingTextProperty, value);
                if (value == "HAMMADDE")
                    lblRecipeTitle.Content = "Stok";
                else
                    lblRecipeTitle.Content = "Reçete";
            }
        }
        public static readonly DependencyProperty PickingTextProperty =
            DependencyProperty.Register("PickingText", typeof(string),
            typeof(MaterialPalletDown), new PropertyMetadata("")
        );

        public string RecipeName
        {
            get { return (string)GetValue(RecipeNameProperty); }
            set { SetValue(RecipeNameProperty, value);
                txtMatName.Content = value;
            }
        }
        public static readonly DependencyProperty RecipeNameProperty =
            DependencyProperty.Register("RecipeName", typeof(string),
            typeof(MaterialPalletDown), new PropertyMetadata("")
        );

        public bool IsActivePallet
        {
            get { return (bool)GetValue(IsActivePalletProperty); }
            set
            {
                SetValue(IsActivePalletProperty, value);
                pnlIsActive.Visibility = value ? Visibility.Visible : Visibility.Hidden;
            }
        }
        public static readonly DependencyProperty IsActivePalletProperty =
            DependencyProperty.Register("IsActivePallet", typeof(bool),
            typeof(MaterialPallet), new PropertyMetadata(false)
        );

        public string SackType
        {
            get { return (string)GetValue(SackTypeProperty); }
            set { SetValue(SackTypeProperty, value);
                txtSackType.Content = value;
            }
        }
        public static readonly DependencyProperty SackTypeProperty =
            DependencyProperty.Register("SackType", typeof(string),
            typeof(MaterialPalletDown), new PropertyMetadata("")
        );

        public string PickingColor
        {
            get { return (string)GetValue(PickingColorProperty); }
            set { SetValue(PickingColorProperty, value); }
        }
        public static readonly DependencyProperty PickingColorProperty =
            DependencyProperty.Register("PickingColor", typeof(string),
            typeof(MaterialPalletDown), new PropertyMetadata("")
        );

        // handlers
        private void btnPickingStatus_Click(object sender, RoutedEventArgs e)
        {
            OnPickingStatusChanged?.Invoke(this.PalletNo);
        }

        public bool IsPalletEnabled
        {
            get { return (bool)GetValue(IsPalletEnabledProperty); }
            set { SetValue(IsPalletEnabledProperty, value);
                btnPalletEnable.Content = value ? "AKTİF" : "PASİF";
                btnPalletEnable.Background = value ? Brushes.LimeGreen : Brushes.Red;
            }
        }
        public static readonly DependencyProperty IsPalletEnabledProperty =
            DependencyProperty.Register("IsPalletEnabled", typeof(bool),
            typeof(MaterialPalletDown), new PropertyMetadata(false)
        );

        public string EnabledColor
        {
            get
            {
                return IsPalletEnabled ? "LimeGreen" : "Red";
            }
        }

        public string EnabledText
        {
            get
            {
                return IsPalletEnabled ? "AKTİF" : "PASİF";
            }
        }

        private void btnPalletEnable_Click(object sender, RoutedEventArgs e)
        {
            IsPalletEnabled = !IsPalletEnabled;
            OnPalletEnabledChanged?.Invoke(this.PalletNo, IsPalletEnabled);
        }

        public bool IsRawMaterial
        {
            get
            {
                return PickingText == "HAMMADDE";
            }
        }

        public string RecipeText
        {
            get
            {
                return IsRawMaterial ? "Stok" : "Reçete";
            }
        }

        private void btnSelectRecipe_Click(object sender, RoutedEventArgs e)
        {
            OnSelectRecipeSignal?.Invoke(PalletNo);
        }
    }
}
