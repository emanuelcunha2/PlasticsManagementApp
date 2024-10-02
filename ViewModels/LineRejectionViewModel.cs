using Microsoft.IdentityModel.Tokens;
using PlasticsApp.Models;
using PlasticsApp.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlasticsApp.ViewModels
{
    public class LineRejectionViewModel : ViewModelBase
    {
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
        ObservableCollection<FailCode> FailCodes { get; set; } = new();

        private string _selectedFailCode = string.Empty;
        public string SelectedFailCode
        {
            get { return _selectedFailCode; }
            set
            {
                _selectedFailCode = value;
                OnPropertyChanged(nameof(SelectedFailCode));
            }
        }

        public LineRejectionViewModel()
        {
            FailCodes = PYMS.GetFailureCodes();
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

                // Scan if its FTQ code and there is no order
                if (SelectedProductionOrder.OrderId.IsNullOrEmpty() && text.Length == 3)
                {
                    page.DisplayAlert("Erro", $"Leia primeiro a Ordem de produção!", "Ok");
                    return;
                }

                // Scan if its FTQ code
                if (text.Length == 3 && text.StartsWith("I"))
                {
                    if (FailCodes.Where(x => x.SmallFailCode == text).Count() != 0) 
                    {
                        SelectedFailCode = text; 
                    }
                    else
                    {
                        page.DisplayAlert("Erro", $"Código inválido!", "Ok");
                    }
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
                        }
                        else { OnPropertyChanged(); }
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
