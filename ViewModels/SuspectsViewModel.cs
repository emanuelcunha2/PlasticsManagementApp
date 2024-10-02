using PlasticsApp.Commands;
using PlasticsApp.Models;
using PlasticsApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;

namespace PlasticsApp.ViewModels
{
    public class SuspectsViewModel : ViewModelBase
    {
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

        private string _saveButtonText = "Deia Scan da Ordem da Produção";
        public string SaveButtonText
        {
            get { return _saveButtonText; }
            set 
            { 
                _saveButtonText = value;
                OnPropertyChanged();

                if(value != "Bloquear" && value != "Desbloquear"){ SaveButtonEnabled = false; }
                else { SaveButtonEnabled = true; }
            }
        }

        private bool _saveButtonEnabled = true;
        public bool SaveButtonEnabled
        {
            get { return _saveButtonEnabled; }
            set
            {
                if (_saveButtonEnabled != value)
                {
                    _saveButtonEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand SaveButtonPressed { get; }
        public SuspectsViewModel()
        {

            SaveButtonPressed = new RelayCommand(() =>
            {
                OnPressSaveButton();
            });


            MessagingCenter.Subscribe<object, string>(this, "ScannedEventTriggered", (sender, message) =>
            {
                ScanTriggered(message);
            });
        }

        private void OnPressSaveButton()
        {
            bool response = PYMS.ChangeProductionOrderBlockStatus(SelectedProductionOrder, SelectedProductionOrder.Blocked);

            if(!response) 
            {
                var page = App.Current.MainPage;
                page.DisplayAlert("Erro", $"Erro no bloqueio ou desbloqueio da Ordem de Produção!", "Ok");
            }
            else
            {
                SaveButtonText = "Deia Scan da Ordem da Produção";
                SelectedProductionOrder = new();
            }
        }
        
        public void ClearCamps()
        {
            SelectedProductionOrder = new();
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
                    if(!SelectedProductionOrder.Exists) 
                    {
                        var page = App.Current.MainPage;
                        page.DisplayAlert("Erro", $"Ordem de produção inválida!", "Ok");
                        SaveButtonText = "Deia Scan da Ordem da Produção";
                    }
                    else
                    {
                        OnPropertyChanged();
                        if (SelectedProductionOrder.Blocked) { SaveButtonText = "Desbloquear"; }
                        else { SaveButtonText = "Bloquear"; }
                    }
                }
            });
        }


        public void Unsubscribe()
        {
            MessagingCenter.Unsubscribe<object, string>(this, "ScannedEventTriggered");
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
