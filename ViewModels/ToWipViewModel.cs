using Microsoft.IdentityModel.Tokens;
using PlasticsApp.Commands;
using PlasticsApp.Models;
using PlasticsApp.Services;
using System.Windows.Input;

namespace PlasticsApp.ViewModels
{
    public class ToWipViewModel : ViewModelBase
    {
        private int _toWIPQuantity = 0;
        public int ToWIPQuantity
        {
            get { return _toWIPQuantity; }
            set
            {
                if (_toWIPQuantity != value)
                {
                    _toWIPQuantity = value;
                    OnPropertyChanged();
                }
            }
        }

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
        public void Unsubscribe()
        {
            MessagingCenter.Unsubscribe<object, string>(this, "ScannedEventTriggered");
        }
        public ICommand SaveButtonPressed { get; }
        private MTS _mts = new(); 
        public ToWipViewModel()
        {
            MessagingCenter.Subscribe<object, string>(this, "ScannedEventTriggered", (sender, message) =>
            {
                ScanTriggered(message);
            });

            SaveButtonPressed = new RelayCommand(() =>
            {
                var page = App.Current.MainPage;
                if (!VerifyAllInformationsFilled())
                {
                    page.DisplayAlert("Erro", $"Falta de Informações!", "Ok");
                    return;
                }

                // If its MTS then the order has a mtsBarcode
                if (SelectedProductionOrder.IsMts)
                {
                    _mts.Transfer(SelectedProductionOrder.MtsBarcode, "PLASINJPROD");
                }

                // Move to WIP
                var response = PYMS.MoveToWIP(SelectedProductionOrder, ToWIPQuantity);
                if (response != "")
                {
                    page.DisplayAlert("Erro", $"{response}!", "Ok");
                    return;
                }

                SnackBarHandler.SendSuccessMessage();
                ClearCamps();
            });
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
                        ToWIPQuantity = int.Parse(SelectedProductionOrder.Quantity.IsNullOrEmpty() ? "0" : SelectedProductionOrder.Quantity);

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

                        if(SelectedProductionOrder.DateWIP.Year >= 2010)
                        {
                            page.DisplayAlert("Erro", $"Ordem já se encontra no WIP!", "Ok");
                            SelectedProductionOrder = new();
                            return;
                        }

                    }
                }
            });
        }

        public bool VerifyAllInformationsFilled()
        {
            if(SelectedProductionOrder.OrderNumber.IsNullOrEmpty() 
            || SelectedProductionOrder.PartNumber.IsNullOrEmpty()
            || SelectedProductionOrder.OrderId.IsNullOrEmpty()
            || SelectedProductionOrder.Quantity.IsNullOrEmpty()
            || ToWIPQuantity == 0)
            {
                return false;
            }
            return true;
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

        private void ClearCamps()
        {
            SelectedProductionOrder = new();
            ToWIPQuantity = 0;
        }
    }
}
