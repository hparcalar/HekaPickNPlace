﻿using PickNPlace.Business;
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
using System.Windows.Shapes;

namespace PickNPlace
{
    /// <summary>
    /// Interaction logic for OnlinePalletEdit.xaml
    /// </summary>
    public partial class OnlinePalletEdit : Window
    {
        public OnlinePalletEdit()
        {
            InitializeComponent();
        }

        public HkAutoPallet Pallet { get; set; }
        public PlaceRequestDTO PlaceRequest { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.BindModel();
        }

        private void BindModel()
        {
            if (PlaceRequest != null && PlaceRequest.Items != null)
                dgItems.ItemsSource = PlaceRequest.Items;

            containerFloors.ItemsSource = null;
            containerFloors.ItemsSource = Pallet != null ? Pallet.Floors : null;
        }

        private void btnBackward_Click(object sender, RoutedEventArgs e)
        {
            var _logic = HkLogicWorker.GetInstance();
            if (_logic.Sim_RemoveLastItem(Pallet.PalletNo))
            {
                this.Pallet = _logic.GetPalletData(Pallet.PalletNo);
                this.BindModel();
            }
            else
            {
                MessageBox.Show("Son çuval dizilimden çıkartılamadı.", "Uyarı", MessageBoxButton.OK);
            }
        }

        private void btnForward_Click(object sender, RoutedEventArgs e)
        {
            var sltItem = dgItems.SelectedItem as PlaceRequestItemDTO;
            if (sltItem == null)
            {
                MessageBox.Show("Lütfen yerleşimi yapılacak olan stok bilgisini seçiniz.", "Uyarı", MessageBoxButton.OK);
                return;
            }
            else
            {
                var _logic = HkLogicWorker.GetInstance();
                if (_logic.Sim_PlaceItem(Pallet.PalletNo, sltItem.ItemCode))
                {
                    this.Pallet = _logic.GetPalletData(Pallet.PalletNo);
                    this.BindModel();
                }
                else
                {
                    MessageBox.Show("Yeni çuval yerleştirilemedi.", "Uyarı", MessageBoxButton.OK);
                }
            }
        }
    }
}