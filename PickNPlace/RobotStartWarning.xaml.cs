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
    /// Interaction logic for RobotStartWarning.xaml
    /// </summary>
    public partial class RobotStartWarning : Window
    {
        public RobotStartWarning()
        {
            InitializeComponent();

            IsAccepted = false;
        }

        public bool IsAccepted { get; set; }

        private void btnYes_Click(object sender, RoutedEventArgs e)
        {
            IsAccepted = true;
            this.Close();
        }

        private void btnNo_Click(object sender, RoutedEventArgs e)
        {
            IsAccepted = false;
            this.Close();
        }
    }
}
