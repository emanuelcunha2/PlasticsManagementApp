using Microsoft.IdentityModel.Tokens;
using PlasticsApp.Commands;
using PlasticsApp.Models;
using PlasticsApp.Services;
using PlasticsApp.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PlasticsApp.ViewModels
{
    public class FtqViewModel : ViewModelBase
    {
        public OrderViewModel FirstTab { get; set; }
        private int _orderFailQuantity = 0;

        public int OrderFailQuantity
        {
            get { return _orderFailQuantity; }
            set
            {
                if (_orderFailQuantity != value)
                {
                    _orderFailQuantity = value;
                    OnPropertyChanged();
                }
            }
        }
        private bool _isOrderFailTableVisible = false;
        public bool IsOrderFailTableVisible
        {
            get { return _isOrderFailTableVisible; }
            set
            {
                _isOrderFailTableVisible = value;
                OnPropertyChanged();
            }
        }
        private bool _isEmptyTableMessageVisible = true;
        public bool IsEmptyTableMessageVisible
        {
            get { return _isEmptyTableMessageVisible;}
            set
            {
                _isEmptyTableMessageVisible = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<FailCode> FailCodes { get; set; } = new();
        public ObservableCollection<OrderFail> OrderFails { get; set; } = new();

        private FailCode _selectedFailCode = new();
        public FailCode SelectedFailCode
        {
            get { return _selectedFailCode; }
            set
            {
                _selectedFailCode = value;

                if(FirstTab != null)
                {
                    if (FirstTab.SelectedProductionOrder.PartNumber.IsNullOrEmpty())
                    {
                        var page = App.Current.MainPage;
                        page.DisplayAlert("Erro", $"Introduza primeiro a ordem de produção!", "Ok");
                        _selectedFailCode = new();
                    }
                }

                SmallCode = _selectedFailCode == null ? "" : _selectedFailCode.SmallFailCode;
                OnPropertyChanged();
            }
        }
        private string _smallCode = "";
        public string SmallCode
        {
            get { return _smallCode; }
            set
            {
                _smallCode = value;
                OnPropertyChanged(nameof(SmallCode));
            }
        }

        private bool _failQuantityEnabled = true;
        public bool FailQuantityEnabled
        {
            get { return _failQuantityEnabled; }
            set
            {
                _failQuantityEnabled = value;
                OnPropertyChanged(nameof(FailQuantityEnabled));
            }
        }

        public RejectionFtqPage Sender;
        public ICommand DeleteOrderFail { get; }
        public ICommand AddOrderFail { get; } 
        public ICommand SaveButtonPressed { get; }
        public FtqViewModel() 
        {
            FailCodes = PYMS.GetFailureCodes(); 

            OrderFails.CollectionChanged += OnOrderFailsChange; 

            MessagingCenter.Subscribe<object, string>(this, "ScannedEventTriggered", (sender, message) =>
            {
                ScanTriggered(message);
            });

            DeleteOrderFail = new RelayCommand((parameter) => 
            {
                if(parameter is OrderFail orderFail)
                {
                    OrderFails.Remove(orderFail);
                    OnPropertyChanged(nameof(OrderFails));
                }
            });


            AddOrderFail = new RelayCommand(() =>
            {
                FailQuantityEnabled = false;
                FailQuantityEnabled = true;

                if (!VerifyCodeInformationFilled()) { return;}

                // If this Order Fail is new
                if(OrderFails.Where(x => x.Code == SelectedFailCode.FullFailCode).Count() == 0)
                {
                    OrderFails.Add(new OrderFail { Code = SelectedFailCode.FullFailCode, Quantity = OrderFailQuantity, Index = SelectedFailCode.Description });
                }
                else
                {
                    OrderFails.Where(x => x.Code == SelectedFailCode.FullFailCode).FirstOrDefault().Quantity += OrderFailQuantity;
                }
                 
                OrderFailQuantity = 0;
            });

            SaveButtonPressed = new RelayCommand(() =>
            {  
                if (!VerifyAllInformationFilled())
                {
                    var page = App.Current.MainPage;
                    page.DisplayAlert("Erro", $"Informações não preenchidas!", "Ok");
                    return;
                }

                var response = PYMS.InsertOrderFailure(FirstTab.SelectedProductionOrder, OrderFails);

                SnackBarHandler.SendSuccessMessage();
                ResetInformations(); 
            });
        } 

        public bool VerifyAllInformationFilled()
        {
            if(OrderFails.Count == 0 
            || FirstTab.SelectedProductionOrder.PartNumber.IsNullOrEmpty() 
            || FirstTab.SelectedProductionOrder.OrderNumber.IsNullOrEmpty() 
            || FirstTab.SelectedProductionOrder.Quantity.IsNullOrEmpty())
            {
                return false;
            }
            return true;
        }

        public void ResetInformations()
        {
            OrderFailQuantity = 0;
            SelectedFailCode = new();

            FirstTab.SelectedProductionOrder = new(); 

            FirstTab.RejectedQuantity = 0;
            OrderFails.Clear();
        }

        public bool VerifyCodeInformationFilled()
        {
            if (SelectedFailCode.FullFailCode.IsNullOrEmpty())
            {
                var page = App.Current.MainPage;
                page.DisplayAlert("Erro", $"Introduza o código da falha!", "Ok");
                return false;
            }

            if (OrderFailQuantity == 0)
            {
                var page = App.Current.MainPage;
                page.DisplayAlert("Erro", $"Introduza uma quantidade!", "Ok");
                return false;
            }
            return true;
        }

        public void OnOrderFailsChange(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(OrderFails.Count == 0)
            {
                IsOrderFailTableVisible = false;
                IsEmptyTableMessageVisible = true;
            }
            else
            {
                IsOrderFailTableVisible = true;
                IsEmptyTableMessageVisible = false;
            }

            if (FirstTab != null)
            {
                int quantity = 0;
                foreach (OrderFail of in OrderFails)
                {
                    quantity += of.Quantity;
                }
                FirstTab.RejectedQuantity = quantity;
            }
        }
        public void Unsubscribe()
        {
            MessagingCenter.Unsubscribe<object, string>(this, "ScannedEventTriggered");
        }
        private void ScanTriggered(string text)
        {
            MainThread.BeginInvokeOnMainThread(async() =>
            {
                text = text.ToUpper();
                var page = App.Current.MainPage;

                // Scan if its FTQ code and there is no order
                if (FirstTab.SelectedProductionOrder.PartNumber.IsNullOrEmpty())
                {
                    await page.DisplayAlert("Erro", $"Introduza primeiro a ordem de produção!", "Ok");
                    return;
                }

                // Scan if its FTQ code
                if (text.Length == 3 && text.StartsWith("I") )
                {
                    if (FailCodes.Where(x => x.FullFailCode.StartsWith(text)).Count() != 0)
                    {
                        SelectedFailCode = FailCodes.Where(x => x.FullFailCode.StartsWith(text)).FirstOrDefault();
                        SmallCode = SelectedFailCode.SmallFailCode;
                    }
                    else
                    {
                        await page.DisplayAlert("Erro", $"Código inválido!", "Ok");
                    }
                    return;
                }
            });
        }
    }
}
