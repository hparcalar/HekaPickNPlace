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
    /// Interaction logic for StartConfirmation.xaml
    /// </summary>
    public partial class StartConfirmation : UserControl
    {
        public delegate void AcceptedEvent(bool accepted);
        public event AcceptedEvent OnIsAccepted;
        public StartConfirmation()
        {
            InitializeComponent();
            IsAccepted = false;
        }

        public bool IsAccepted { get; set; }

        private void btnYes_Click(object sender, RoutedEventArgs e)
        {
            IsAccepted = true;
            OnIsAccepted?.Invoke(true);
        }

        private void btnNo_Click(object sender, RoutedEventArgs e)
        {
            IsAccepted = false;
            OnIsAccepted?.Invoke(false);
        }
    }
}
