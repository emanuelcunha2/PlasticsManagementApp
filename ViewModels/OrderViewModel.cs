using PlasticsApp.Models;
using PlasticsApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlasticsApp.ViewModels
{
    public class OrderViewModel : ViewModelBase
    { 

        private int _rejectedQuantity = 0;
        public int RejectedQuantity
        {
            get { return _rejectedQuantity; }
            set
            {
                if (_rejectedQuantity != value)
                {
                    _rejectedQuantity = value;
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
        public OrderViewModel()
        {
            MessagingCenter.Subscribe<object, string>(this, "ScannedEventTriggered", (sender, message) =>
            {
                ScanTriggered(message);
            });
        }

        private void ScanTriggered(string text)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                text = text.ToUpper();
                var page = App.Current.MainPage;

                // Scan if its FTQ code
                if (text.Length == 3 && text.StartsWith("I"))
                {
                    return;
                }

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
                        if (SelectedProductionOrder.DateWIP.Year >= 2010)
                        {
                            page.DisplayAlert("Erro", $"Ordem já se encontra no WIP!", "Ok");
                            SelectedProductionOrder = new();
                            return;
                        }
                    }
                }
            });
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
