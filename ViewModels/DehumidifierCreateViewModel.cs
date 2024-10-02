using Microsoft.IdentityModel.Tokens;
using PlasticsApp.Commands;
using PlasticsApp.Models;
using PlasticsApp.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PlasticsApp.ViewModels
{
    public class DehumidifierCreateViewModel : ViewModelBase
    {
        Dehumidifier Dehumidifier { get; set; } = new();
        private string _machine = string.Empty;
        public string Machine
        {
            get { return _machine; }
            set
            {
                if (_machine != value)
                {
                    _machine = value;
                    OnPropertyChanged();
                }
            }
        }
        private string _granulate = string.Empty;
        public string Granulate
        {
            get { return _granulate; }
            set
            {
                if (_granulate != value)
                {
                    _granulate = value;             
                    OnPropertyChanged();
                }
            }
        }
        public string _lot = string.Empty;
        public string Lot
        {
            get { return _lot; }
            set
            {
                if(_lot != value)
                {
                    _lot = value;
                    OnPropertyChanged();
                }
            }
        }
        private string _time = string.Empty;
        public string Time
        {
            get { return _time; }
            set
            {
                if(_time != value)
                {
                    _time = value.Replace(".", "");
                    OnPropertyChanged();
                }
            }
        }
        public string _weight = string.Empty;
        public string Weight
        {
            get { return _weight; }
            set
            {
                if(_weight != value)
                {
                    _weight = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool _isWeekend = false;
        public bool IsWeekend
        {
            get { return _isWeekend; }
            set
            {
                if(_isWeekend != value)
                {
                    _isWeekend = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand IsWeekendTicked { get; }
        public ICommand SaveButtonPressed { get; }
        private MTS mts = new MTS(); 
        public DehumidifierCreateViewModel()
        {  
            IsWeekendTicked = new RelayCommand(() =>
            {
                if (IsWeekend) { IsWeekend = false; }
                else { IsWeekend = true; }
            });

            SaveButtonPressed = new RelayCommand(() =>
            {
                OnPressSaveButton();
            });

            MessagingCenter.Subscribe<object, string>(this, "ScannedEventTriggered", (sender, message) =>
            { 
                ScanTriggered(message);
            });
        }
        public void Unsubscribe()
        {
            MessagingCenter.Unsubscribe<object, string>(this, "ScannedEventTriggered");
        }

        private async void OnPressSaveButton()
        {
            var page = App.Current.MainPage;

            if (!AllEssentialInformationInserted())
            {
                await page.DisplayAlert("Erro", $"Informações essenciais por preencher!", "Ok");
                return;
            }

            if (Time.IsNullOrEmpty())
            {
                bool isStuffing = await page.DisplayAlert("Verificar", $"Não indicou o tempo de secagem!\nÉ um início de estufagem?", "Sim", "Não");

                if (isStuffing)
                {
                    Dehumidifier.StartDate = DateTime.Now;
                    Dehumidifier.EndDate = DateTime.Now;
                }
                else
                {
                    await page.DisplayAlert("Verificar", $"Então preencha o tempo de secagem", "Ok");
                    return;
                }
            }
            else
            {
                int hours;

                try{ hours = Int32.Parse(Time); } 
                catch(Exception ex) { Debug.WriteLine(ex); return; }

                Dehumidifier.StartDate = DateTime.Now;
                Dehumidifier.EndDate = DateTime.Now.AddHours(hours);
            }

            Dehumidifier.GranulateBatch = Lot;
            Dehumidifier.GranulateBC = Granulate;
            Dehumidifier.LoadWeight = double.Parse(Weight);

            // Execute Changes  !!!!!!!!!!!!!!! UNCOMMENT !!!!!!!!!!!!!!!!
            
            PYMS.AddDehumidifierLoading(Dehumidifier, IsWeekend);
            // If its MTS
            if (Dehumidifier.GranulateBC.Length == 11)
            {
                mts.Transfer(Dehumidifier.GranulateBC, "RECYCLE");
            }
            SnackBarHandler.SendSuccessMessage();
            ClearAllFields();
        }

        private bool AllEssentialInformationInserted()
        {
            if(Machine.IsNullOrEmpty() || Granulate.IsNullOrEmpty() || Lot.IsNullOrEmpty() || Weight.IsNullOrEmpty()) { return false; }
            return true;
        }

        private async void OnSetGranulate()
        {
            if (!Machine.IsNullOrEmpty())
            {
                try
                {
                    // Set last granulate from the selected dehumidifier
                    Dehumidifier.Name = $"{Machine}";
                    PYMS.SetLastGranulateInformationDehumidifier(Dehumidifier);

                    if (Granulate.StartsWith("SB"))
                    {
                        Weight = mts.UnitInfo(Granulate).Qty.ToString();
                    }

                    if (Granulate == Dehumidifier.GranulateBC)
                    {
                        var page = App.Current.MainPage;
                        bool answer = await page.DisplayAlert("Verificar", $"O lote deste Granulado continua a ser  {Dehumidifier.GranulateBatch} ", "Sim", "Não");

                        if (answer)
                        {
                            Lot = Dehumidifier.GranulateBatch;
                        }
                    }
                }
                catch(Exception ex) 
                {
                    var page = App.Current.MainPage;
                    await page.DisplayAlert("Erro", ""+ ex.Message, "Cancelar");
                }

            } // Else hasn't added the dehumifier machine name
            else
            {
                var page = App.Current.MainPage;
                await page.DisplayAlert("Alerta", "Insira primeiro o nome do equipamento", "Cancelar");
            }
        }

        private void ClearAllFields()
        { 
            Granulate = "";
            Lot = "";
            Time = "";
            Weight = "";
            IsWeekend = false;
            Dehumidifier = new();
        }

        private void ScanTriggered(string text)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                // If is machine
                if (text.Substring(0, 4).ToLower() == "pl-d")
                {
                    Machine = text;
                } // Else if is granulate format1
                else if (text.Substring(0, 4) == "0000")
                {
                    Granulate = text;
                    OnSetGranulate();
                } // Else if is granulate format2
                else if(text.Length == 11 && text.StartsWith("SB"))
                {
                    Granulate = text;
                    OnSetGranulate();
                }
            });
        }

    }
}
