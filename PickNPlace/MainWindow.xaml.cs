using PickNPlace.Plc;
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
using PickNPlace.DataAccess;

namespace PickNPlace
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // make db migrations
            SchemaFactory.ApplyMigrations();

            // init plc communication
            this._plc = PlcWorker.Instance();
            this._plc.OnPlcConnectionChanged += _plc_OnPlcConnectionChanged;
            this._plc.Start();
        }

        private void _plc_OnPlcConnectionChanged(bool connected)
        {
            this.Dispatcher.BeginInvoke((Action)delegate
            {
                imgPlcOk.Source = new BitmapImage(new Uri(connected ? "/green_circle.png" : "/red_circle.png", UriKind.Relative));
            });
        }

        private PlcWorker _plc;

        private void btnManagement_Click(object sender, RoutedEventArgs e)
        {
            ManagementWindow wnd = new ManagementWindow();
            wnd.ShowDialog();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this._plc.Stop();
        }
    }
}
