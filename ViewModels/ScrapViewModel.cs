using PlasticsApp.Commands;
using PlasticsApp.Models;
using PlasticsApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PlasticsApp.ViewModels
{
    public class ScrapViewModel : ViewModelBase
    {
        private int _scrapQuantity = 0;
        public int ScrapQuantity
        {
            get { return _scrapQuantity; }
            set 
            { 
                _scrapQuantity = value;
                OnPropertyChanged();
            }
        }

        private ProductionOrder _selectedProductionOrder = new();
        public ProductionOrder SelectedProductionOrder
        {
            get { return _selectedProductionOrder; }
            set
            {
                _selectedProductionOrder = value;
                OnPropertyChanged(nameof(SelectedProductionOrder));
            }
        }
        public ICommand SaveButtonPressed { get; }
        public ScrapViewModel()
        {
            MessagingCenter.Subscribe<object, string>(this, "ScannedEventTriggered", (sender, message) =>
            {
                ScanTriggered(message);
            });

            SaveButtonPressed = new RelayCommand(() =>
            {
                if (!IsPartNumberValid()) { return; }

                if(ScrapQuantity > 100)
                {
                    var page = App.Current.MainPage;
                    page.DisplayAlert("Erro", $"A quantidade para Refugo deve ser menor que 100!", "Ok");
                    return;
                }

                if(ScrapQuantity <= 0)
                {
                    var page = App.Current.MainPage;
                    page.DisplayAlert("Erro", $"A quantidade para Refugo deve ser superior a 0!", "Ok");
                    return;
                }

                var response = PYMS.ScrapThisDirectPN(SelectedProductionOrder.PartNumber, ScrapQuantity);

                if(response != "")
                {
                    var page = App.Current.MainPage;
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
                if (text.StartsWith("I")) { text = text.Substring(1); }
                else { return; }

                if (IsOrderValid(text))
                {
                    SelectedProductionOrder = PYMS.GetProductionOrder(int.Parse(text));
                    if (!SelectedProductionOrder.Exists)
                    {
                        var page = App.Current.MainPage;
                        page.DisplayAlert("Erro", $"Ordem de produção inválida!", "Ok");
                        return;
                    }
                    else
                    {
                        SelectedProductionOrder.PartNumber = SelectedProductionOrder.PartNumber.Remove(0, 4);
                    }
                }
            });
        }

        public void Unsubscribe()
        {
            MessagingCenter.Unsubscribe<object, string>(this, "ScannedEventTriggered");
        }

        public void ClearCamps()
        {
            SelectedProductionOrder = new();
            ScrapQuantity = 0;
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

        public bool IsPartNumberValid()
        {
            if(SelectedProductionOrder.PartNumber.Length != 8)
            {
                var page = App.Current.MainPage;
                page.DisplayAlert("Erro", $"O Part Number deve ter 8 dígitos!", "Ok");
                return false;
            }
            return true;
        }

    }
}
