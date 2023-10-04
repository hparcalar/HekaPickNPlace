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
using PickNPlace.DTO;

namespace PickNPlace
{
    /// <summary>
    /// Interaction logic for ManualWorkOrder.xaml
    /// </summary>
    public partial class ManualWorkOrder : Window
    {
        public ManualWorkOrder()
        {
            InitializeComponent();
        }

        public PlaceRequestDTO WorkOrder { get; set; }
        public HkAutoPallet[] RawPallets { get; set; }
        public bool SelectionOk { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.SelectionOk = false;
            this.BindModel();
        }

        private void BindModel()
        {
            chkSameMaterial.IsChecked = false;

            if (RawPallets != null && RawPallets.Length > 0 
                && WorkOrder != null && WorkOrder.Items != null && WorkOrder.Items.Length > 0)
            {
                foreach (var pallet in RawPallets)
                {
                    var relatedOrder = WorkOrder.Items.FirstOrDefault(d => d.ItemCode == pallet.RawMaterialCode);
                    if (relatedOrder != null)
                    {
                        if (pallet.PalletNo == 1)
                            txtPallet1Count.Text = relatedOrder.PiecesPerBatch.ToString();
                        else if (pallet.PalletNo == 2)
                        {
                            txtPallet2Count.Text = relatedOrder.PiecesPerBatch.ToString();

                            if (RawPallets.Length > 1 && RawPallets[0].RawMaterialCode == pallet.RawMaterialCode)
                                chkSameMaterial.IsChecked = true;
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(txtPallet1Count.Text))
                txtPallet1Count.Text = "0";
            if (string.IsNullOrEmpty(txtPallet2Count.Text))
                txtPallet2Count.Text = "0";
        }

        #region PALLET COUNT EVENTS
        private void btnPallet1Minus_Click(object sender, RoutedEventArgs e)
        {
            int currentVal = 0;
            if (Int32.TryParse(txtPallet1Count.Text, out currentVal))
            {
                currentVal--;

                if (currentVal < 0)
                    currentVal = 0;

                txtPallet1Count.Text = currentVal.ToString();
            }
        }

        private void btnPallet1Plus_Click(object sender, RoutedEventArgs e)
        {
            int currentVal = 0;
            if (Int32.TryParse(txtPallet1Count.Text, out currentVal))
            {
                currentVal++;

                txtPallet1Count.Text = currentVal.ToString();
            }
        }

        private void btnPallet2Minus_Click(object sender, RoutedEventArgs e)
        {
            int currentVal = 0;
            if (Int32.TryParse(txtPallet2Count.Text, out currentVal))
            {
                currentVal--;

                if (currentVal < 0)
                    currentVal = 0;

                txtPallet2Count.Text = currentVal.ToString();
            }
        }

        private void btnPallet2Plus_Click(object sender, RoutedEventArgs e)
        {
            int currentVal = 0;
            if (Int32.TryParse(txtPallet2Count.Text, out currentVal))
            {
                currentVal++;

                txtPallet2Count.Text = currentVal.ToString();
            }
        }
        #endregion

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private string GenerateRawMatCode()
        {
            try
            {
                var rnd = new Random();
                var genVal = rnd.Next(1000, 9999);
                return genVal.ToString();
            }
            catch (Exception)
            {

            }

            return string.Empty;
        }

        private void btnApprove_Click(object sender, RoutedEventArgs e)
        {
            var rawList = new List<HkAutoPallet>();
            string firstGenCode = string.Empty;

            // set 1st pallet
            if (Convert.ToInt32(txtPallet1Count.Text) > 0)
            {
                rawList.Add(new HkAutoPallet
                {
                    PalletNo = 1,
                    IsEnabled = true,
                    SackType = 3,
                    IsRawMaterial = true,
                });

                string rawCode = GenerateRawMatCode();
                firstGenCode = rawCode;

                if (!string.IsNullOrEmpty(rawCode))
                    rawList[0].RawMaterialCode = "1_" + rawCode;
                else
                    rawList.Clear();
            }

            // set 2nd pallet
            if (Convert.ToInt32(txtPallet2Count.Text) > 0 || (chkSameMaterial.IsChecked == true && Convert.ToInt32(txtPallet1Count.Text) > 0))
            {
                rawList.Add(new HkAutoPallet
                {
                    PalletNo = 2,
                    IsEnabled = true,
                    SackType = 3,
                    IsRawMaterial = true,
                });

                if (chkSameMaterial.IsChecked == true && rawList.Count > 1)
                {
                    rawList[rawList.Count - 1].RawMaterialCode = rawList[0].RawMaterialCode;
                }
                else
                {
                    string rawCode = GenerateRawMatCode();
                    if (!string.IsNullOrEmpty(rawCode))
                    {
                        int maxTryCount = 0;
                        while (rawCode == firstGenCode)
                        {
                            if (maxTryCount > 10)
                                break;

                            rawCode = GenerateRawMatCode();
                            maxTryCount++;
                        }

                        if (rawCode != firstGenCode)
                            rawList[rawList.Count - 1].RawMaterialCode = "2_" + rawCode;
                        else
                            rawList.RemoveAt(rawList.Count - 1);
                    }
                }
            }

            RawPallets = rawList.ToArray();

            // generate manual work order
            var pReq = new PlaceRequestDTO
            {
                RecipeCode = "Manual",
                RecipeName = "Manual",
                RequestNo = "Manual",
            };

            List<PlaceRequestItemDTO> pReqItems = new List<PlaceRequestItemDTO>();
            foreach (var pallet in rawList)
            {
                if (!pReqItems.Any(d => d.ItemCode == pallet.RawMaterialCode))
                {
                    pReqItems.Add(new PlaceRequestItemDTO
                    {
                        ItemCode = pallet.RawMaterialCode,
                        PiecesPerBatch = pallet.PalletNo == 1 ? Convert.ToInt32(txtPallet1Count.Text) : pallet.PalletNo == 2 ? Convert.ToInt32(txtPallet2Count.Text) : 0,
                        ItemName = pallet.RawMaterialCode,
                        SackType = 3,
                    });
                }
            }

            pReq.Items = pReqItems.ToArray();

            WorkOrder = pReq;

            // sign selection as ok then close this form
            this.SelectionOk = true;
            this.Close();
        }
    }
}
