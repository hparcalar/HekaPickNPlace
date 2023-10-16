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
using System.Text.RegularExpressions;

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
        private string RawMatCode1;
        private string RawMatCode2;

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
                        {
                            txtPallet1Count.Text = relatedOrder.PiecesPerBatch.ToString();
                            RawMatCode1 = pallet.RawMaterialCode;
                        }
                        else if (pallet.PalletNo == 2)
                        {
                            txtPallet2Count.Text = relatedOrder.PiecesPerBatch.ToString();
                            RawMatCode2 = pallet.RawMaterialCode;

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

            this.CheckRawMatButtonTexts();
        }

        private void CheckRawMatButtonTexts()
        {
            if (RawPallets != null)
            {
                var plt1 = RawPallets.FirstOrDefault(d => d.PalletNo == 1);
                if (plt1 != null)
                {
                    RawMatCode1 = plt1.RawMaterialCode;
                }

                var plt2 = RawPallets.FirstOrDefault(d => d.PalletNo == 2);
                if (plt2 != null)
                {
                    RawMatCode2 = plt2.RawMaterialCode;
                }
            }
           
            if (!string.IsNullOrEmpty(RawMatCode1))
            {
                btnSelectMat1.Content = "HAMMADDE TÜRÜNÜ DEĞİŞTİR";
                txtRawMat1.Content = RawMatCode1;
            }
            else
            {
                btnSelectMat1.Content = "YENİ HAMMADDE";
                txtRawMat1.Content = "YOK";
            }

            if (!string.IsNullOrEmpty(RawMatCode2))
            {
                btnSelectMat2.Content = "HAMMADDE TÜRÜNÜ DEĞİŞTİR";
                txtRawMat2.Content = RawMatCode2;
            }
            else
            {
                btnSelectMat2.Content = "YENİ HAMMADDE";
                txtRawMat2.Content = "YOK";
            }
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

        private string GenerateRawMatCode(int palletNo)
        {
            try
            {
                var recipeItems = WorkOrder != null && WorkOrder.Items != null ? WorkOrder.Items : null;
                if (recipeItems != null)
                {
                    var lastItem = recipeItems.Where(d => !string.IsNullOrEmpty(d.ItemCode)).OrderByDescending(d => d.ItemCode).FirstOrDefault();
                    if (lastItem != null)
                    {
                        var numPart = Regex.Match(lastItem.ItemCode, "[0-9]+").Value;
                        if (numPart != null)
                        {
                            var currentList = recipeItems.ToList();
                            currentList.Add(new PlaceRequestItemDTO
                            {
                                ItemCode = "HM_" + string.Format("{0:000}", Convert.ToInt32(numPart) + 1),
                                ItemName = "HM_" + string.Format("{0:000}", Convert.ToInt32(numPart) + 1),
                                PiecesPerBatch = 0,
                                SackType = 3,
                            });

                            return "HM_" + string.Format("{0:000}", Convert.ToInt32(numPart) + 1);
                        }
                    }
                }
                else
                {
                    WorkOrder = new PlaceRequestDTO
                    {
                        RecipeCode = "Manual",
                        RecipeName = "Manual",
                        RequestNo = "Manual",
                    };

                    WorkOrder.Items = new PlaceRequestItemDTO[] { 
                        new PlaceRequestItemDTO
                        {
                            ItemCode = "HM_001",
                            ItemName = "HM_001",
                            PiecesPerBatch = 0,
                            SackType = 3,
                        }
                    };

                    return "HM_001";
                }
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
                if (string.IsNullOrEmpty(RawMatCode1))
                {
                    MessageBox.Show("1. Palet için HAMMADDE türü atamasını yapmalısınız.", "Uyarı", MessageBoxButton.OK);
                    return;
                }

                rawList.Add(new HkAutoPallet
                {
                    PalletNo = 1,
                    RawMaterialCode = RawMatCode1,
                    IsEnabled = true,
                    SackType = 3,
                    IsRawMaterial = true,
                });
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
                    rawList[rawList.Count - 1].RawMaterialCode = RawMatCode1;
                }
                else
                {
                    if (string.IsNullOrEmpty(RawMatCode2))
                    {
                        MessageBox.Show("2. Palet için HAMMADDE türü atamasını yapmalısınız.", "Uyarı", MessageBoxButton.OK);
                        return;
                    }

                    rawList[rawList.Count - 1].RawMaterialCode = RawMatCode2;
                }
            }

            RawPallets = rawList.ToArray();

            // generate manual work order
            PlaceRequestDTO pReq = this.WorkOrder;
            if (pReq == null)
            {
                pReq = new PlaceRequestDTO
                {
                    RecipeCode = "Manual",
                    RecipeName = "Manual",
                    RequestNo = "Manual",
                };
            }

            if (pReq.Items == null)
                pReq.Items = new PlaceRequestItemDTO[0];

            List<PlaceRequestItemDTO> pReqItems = pReq.Items.ToList();
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
                else
                {
                    var cItem = pReqItems.FirstOrDefault(d => d.ItemCode == pallet.RawMaterialCode);
                    if (cItem != null && !(pallet.PalletNo == 2 && chkSameMaterial.IsChecked == true))
                    {
                        cItem.PiecesPerBatch = pallet.PalletNo == 1 ? Convert.ToInt32(txtPallet1Count.Text) : pallet.PalletNo == 2 ? Convert.ToInt32(txtPallet2Count.Text) : 0;
                    }
                }
            }

            pReq.Items = pReqItems.ToArray();

            WorkOrder = pReq;

            // sign selection as ok then close this form
            this.SelectionOk = true;
            this.Close();
        }
        
        private void btnSelectMat1_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(RawMatCode1) || 
                (!string.IsNullOrEmpty(RawMatCode1) 
                    && MessageBox.Show("Yeni bir hammadde türüne geçmek istediğinizden emin misiniz?", "Uyarı", MessageBoxButton.YesNo) == MessageBoxResult.Yes))
            {
                RawMatCode1 = GenerateRawMatCode(1);
                chkSameMaterial.IsChecked = false;

                if (!string.IsNullOrEmpty(RawMatCode1))
                {
                    btnSelectMat1.Content = "HAMMADDE TÜRÜNÜ DEĞİŞTİR";
                    txtRawMat1.Content = RawMatCode1;
                }
                else
                {
                    btnSelectMat1.Content = "YENİ HAMMADDE";
                    txtRawMat1.Content = "YOK";
                }
            }
        }

        private void btnSelectMat2_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(RawMatCode2) ||
                (!string.IsNullOrEmpty(RawMatCode2)
                    && MessageBox.Show("Yeni bir hammadde türüne geçmek istediğinizden emin misiniz?", "Uyarı", MessageBoxButton.YesNo) == MessageBoxResult.Yes))
            {
                RawMatCode2 = GenerateRawMatCode(2);

                if (!string.IsNullOrEmpty(RawMatCode2))
                {
                    btnSelectMat2.Content = "HAMMADDE TÜRÜNÜ DEĞİŞTİR";
                    txtRawMat2.Content = RawMatCode2;
                }
                else
                {
                    btnSelectMat2.Content = "YENİ HAMMADDE";
                    txtRawMat2.Content = "YOK";
                }
            }
        }
    }
}
