using Azure;
using Microsoft.IdentityModel.Tokens;
using PlasticsApp.Commands;
using PlasticsApp.Models;
using PlasticsApp.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;

namespace PlasticsApp.ViewModels
{
    public class EndOfLotViewModel : ViewModelBase
    {
        public ObservableCollection<ProductionOrder> ProductionOrders { get; set; } = new();

        private ProductionOrder _selectedProductionOrder = new();
        public ProductionOrder SelectedProductionOrder
        {
            get { return _selectedProductionOrder; }
            set
            {
                _selectedProductionOrder = value;
                OnPropertyChanged();
            }
        }
        private int _quantityOrders = 0;
        public int QuantityOrders
        {
            get { return _quantityOrders; }
            set
            {
                _quantityOrders = value;
                OnPropertyChanged();
            }
        }
        private int _quantityParts = 0;
        public int QuantityParts
        {
            get { return _quantityParts; }
            set
            {
                _quantityParts = value;
                OnPropertyChanged();
            }
        }
        private bool _isLotRejected = false;
        public bool IsLotRejected
        {
            get { return _isLotRejected; }
            set
            {
                ClearCamps();
                _isLotRejected = value;
                OnPropertyChanged();
            }
        }

        private bool _isEmptyTableMessageVisible = true;
        public bool IsEmptyTableMessageVisible
        {
            get { return _isEmptyTableMessageVisible; }
            set
            {
                _isEmptyTableMessageVisible = value;
                OnPropertyChanged();
            }
        }

        private bool _isProductionOrdersTableVisible = false;
        public bool IsProductionOrdersTableVisible
        {
            get { return _isProductionOrdersTableVisible;}
            set
            {
                _isProductionOrdersTableVisible = value;
                OnPropertyChanged();
            }
        }

        public ICommand SaveButtonPressed { get; }
        public ICommand LotRejectedTicked { get; }
        
        public void Unsubscribe()
        {
            MessagingCenter.Unsubscribe<object, string>(this, "ScannedEventTriggered");
        }

        public EndOfLotViewModel() 
        {
            ProductionOrders.CollectionChanged += OnProductionsChanged;

            SaveButtonPressed = new RelayCommand(() =>
            {
                if (!IsNeededInformationFilled())
                {
                    var page = App.Current.MainPage;
                    page.DisplayAlert("Erro no fecho da palete", $"Informações insuficientes!", "Ok");
                }

                if (!IsLotRejected)
                {
                    SaveLot();
                }
                else
                {
                    SaveRejectedLot();
                }
                SnackBarHandler.SendSuccessMessage();
                ClearCamps();
            });

            LotRejectedTicked = new RelayCommand(() =>
            {
                if(IsLotRejected) 
                {
                    IsLotRejected = false;
                    return;
                }
                IsLotRejected = true;
            });

            MessagingCenter.Subscribe<object, string>(this, "ScannedEventTriggered", (sender, message) =>
            {
                ScanTriggered(message);
            });
             
        }
        private void OnProductionsChanged(object sender, NotifyCollectionChangedEventArgs e) 
        {
            if (ProductionOrders.Count == 0)
            {
                IsProductionOrdersTableVisible = false;
                IsEmptyTableMessageVisible = true;
            }
            else
            {
                IsProductionOrdersTableVisible = true;
                IsEmptyTableMessageVisible = false;
            }
        }
        private void ScanTriggered(string text)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                text = text.ToUpper();
                var page = App.Current.MainPage;

                // Scan if its an order
                if (text.StartsWith("I"))
                {
                    text = text.Substring(1);

                    if (IsOrderValid(text))
                    {
                        SelectedProductionOrder = PYMS.GetProductionOrder(int.Parse(text));

                        if (!SelectedProductionOrder.Exists)
                        {
                            page.DisplayAlert("Erro", $"Ordem de produção inválida!", "Ok");
                            SelectedProductionOrder = new();
                            return;
                        }

                        if (SelectedProductionOrder.Blocked)
                        {
                            page.DisplayAlert("Erro", $"Ordem Encontra se bloqueada!", "Ok");
                            SelectedProductionOrder = new();
                            return;
                        }

                        // If Lot rejected it will add one order at a time to the list
                        if (IsLotRejected)
                        {
                            InsertSingleProductionOrder();
                            return;
                        }
                        // Else it will add lot of orders found
                        else
                        {
                            InsertProductionOrdersFromLot();
                            return;
                        }
                    }
                }
                else
                { 
                    page.DisplayAlert("Erro", $"Ordem de produção inválida!", "Ok");
                }

            });
        }
        public bool IsNeededInformationFilled()
        {
            if(QuantityOrders == 0 || QuantityParts == 0 || ProductionOrders.Count() == 0 || SelectedProductionOrder.PartNumber.IsNullOrEmpty())
            {
                return false;
            }
            return true;
        }
        public void SaveLot()
        {
            var page = App.Current.MainPage;
            var response = PYMS.ClosePalette(SelectedProductionOrder.PartNumber);
            if (response != "")
            {
                page.DisplayAlert("Erro no fecho da palete", $"{response}!", "Ok");
                return;
            }
        }

        public void SaveRejectedLot()
        {
            var page = App.Current.MainPage;
            var response = PYMS.ClosePaletteByOrderNr(ProductionOrders);
            if (response != "")
            {
                page.DisplayAlert("Erro no fecho da palete", $"{response}!", "Ok");
                return;
            }
        }

        public void ClearCamps()
        {
            ProductionOrders.Clear();
            SelectedProductionOrder = new();
            QuantityParts = 0;
            QuantityOrders = 0;
        }

        public void InsertSingleProductionOrder()
        {
            // After already checked if order exists and if its not blocked
            // Check if order is already in the list
            if(!ProductionOrders.Where(x => x.OrderNumber == SelectedProductionOrder.OrderNumber).Any())
            {
                var OrderToInsert = PYMS.GetProductionOrder(int.Parse(SelectedProductionOrder.OrderNumber));
                ProductionOrders.Add(OrderToInsert);
                QuantityOrders++;
                QuantityParts += OrderToInsert.QuantityWIP;
            }
            else
            {
                var page = App.Current.MainPage;
                page.DisplayAlert("Erro", $"Ordem de produção já existe na lista!", "Ok");
                return; 
            }
        }

        public void InsertProductionOrdersFromLot()
        {
            var newProductionOrders = PYMS.GetOrdersForWarehouse(SelectedProductionOrder.PartNumber, ref _quantityOrders , ref _quantityParts);
            
            foreach(ProductionOrder prodOrder in newProductionOrders)
            { 
                ProductionOrders.Add(prodOrder);
            }

            OnPropertyChanged(nameof(QuantityOrders));
            OnPropertyChanged(nameof(QuantityParts));

            if (QuantityOrders == 0 && QuantityParts == 0)
            {
                ProductionOrders.Clear();
                SelectedProductionOrder = new();
                var page = App.Current.MainPage;
                page.DisplayAlert("Erro", $"Sem quantidade!", "Ok");
            }
        }

        private static bool IsOrderValid(string text)
        {
            try { int.Parse(text); }
            catch (Exception)
            {
                var page = App.Current.MainPage;
                page.DisplayAlert("Erro", $"Ordem de produção inválida!", "Ok");
                return false;
            }
            return true;

        }
    }
}
