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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PickNPlace.UserControls
{
    /// <summary>
    /// Interaction logic for EmptyPalletDown.xaml
    /// </summary>
    public partial class EmptyPalletDown : UserControl
    {

        public delegate void PalletEnabledChanged(int palletNo, bool enabled);
        public event PalletEnabledChanged OnPalletEnabledChanged;

        public delegate void SelectRecipeSignal(int palletNo);
        public event SelectRecipeSignal OnSelectRecipeSignal;

        public delegate void OnlineEditSignal(int palletNo);
        public event OnlineEditSignal OnlineEditRequested;
        public EmptyPalletDown()
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
            typeof(EmptyPalletDown), new PropertyMetadata(0)
        );

        public bool IsPalletEnabled
        {
            get { return (bool)GetValue(IsPalletEnabledProperty); }
            set { SetValue(IsPalletEnabledProperty, value);
                btnPalletEnable.Content = value ? "AKTİF" : "PASİF";
                btnPalletEnable.Background = value ? new SolidColorBrush(Color.FromRgb(44, 240, 5)) : new SolidColorBrush(Color.FromRgb(176, 17, 5));
                btnPalletEnable.Foreground = new SolidColorBrush(IsPalletEnabled ? Color.FromRgb(0, 0, 0) : Color.FromRgb(255, 255, 255));
            }
        }
        public static readonly DependencyProperty IsPalletEnabledProperty =
            DependencyProperty.Register("IsPalletEnabled", typeof(bool),
            typeof(EmptyPalletDown), new PropertyMetadata(false)
        );

        public TimeSpan? EllapsedTime
        {
            get { return (TimeSpan?)GetValue(EllapsedTimeProperty); }
            set
            {
                SetValue(EllapsedTimeProperty, value);
                if (value != null && value != TimeSpan.Zero)
                {
                    lblEllapsedTime.Content = "Geçen süre: " + string.Format("{0:mm\\:ss}", value);
                }
                else
                {
                    lblEllapsedTime.Content = string.Empty;
                }
            }
        }
        public static readonly DependencyProperty EllapsedTimeProperty =
            DependencyProperty.Register("EllapsedTime", typeof(TimeSpan?),
            typeof(EmptyPalletDown), new PropertyMetadata(TimeSpan.Zero)
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
            typeof(EmptyPalletDown), new PropertyMetadata(false)
        );

        public string EnabledColor
        {
            get
            {
                return IsPalletEnabled ? "#2cf005" : "#b01105";
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

        private void btnSelectRecipe_Click(object sender, RoutedEventArgs e)
        {
            OnSelectRecipeSignal?.Invoke(PalletNo);
        }

        private void btnOnlineEdit_Click(object sender, RoutedEventArgs e)
        {
            OnlineEditRequested?.Invoke(PalletNo);
        }

        public HkAutoPallet Pallet { get; set; }

        public void BindState()
        {
            HkAutoFloor[] floorsData = null;
            if (Pallet != null && Pallet.Floors != null)
            {
                List<HkAutoFloor> _tempData = new List<HkAutoFloor>();

                foreach (var flr in Pallet.Floors)
                {
                    _tempData.Add(new HkAutoFloor
                    {
                        FloorNo = flr.FloorNo,
                        Items = flr.Items.Where(d => d.IsPlaced == true)
                        .Select(d => new HkAutoItem
                        {
                            IsPlaced = d.IsPlaced,
                            IsRotated = d.IsRotated,
                            ItemCode = d.ItemCode,
                            ItemHeight = d.ItemHeight,
                            ItemOrder = d.ItemOrder,
                            ItemWidth = d.ItemWidth,
                            PlacedX = d.PlacedX,
                            PlacedY = d.PlacedY,
                            SackType = d.SackType,
                        })
                        .ToArray()
                    });
                }

                floorsData = _tempData.ToArray();
            }

            containerFloors.ItemsSource = null;
            containerFloors.ItemsSource = floorsData != null ? floorsData.OrderByDescending(d => d.FloorNo).ToArray() : null;
        }
    }
}
