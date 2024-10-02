using Azure;
using CommunityToolkit.Maui.Core;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Maui.Layouts;
using PlasticsApp.Commands;
using PlasticsApp.Models;
using PlasticsApp.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;

namespace PlasticsApp.ViewModels
{
    public class EventsViewModel : ViewModelBase
    {
        public Section Section1 { get; set; } = new(1);
        public Section Tab1 { get; set; } = new(1);
        public Section Section2 { get; set; } = new(2);
        public Section Tab2 { get; set; } = new(2);
        public Section Section3 { get; set; } = new(3);
        public Section Tab3 { get; set; } = new(3);
        public Section Section4 { get; set; } = new(4);
        public Section Tab4 { get; set; } = new(4);
        public Section Section5 { get; set; } = new(5);
        public Section Tab5 { get; set; } = new(5);
        public Section Section6 { get; set; } = new(6);
        public Section Tab6 { get; set; } = new(6);
        public Section Section7 { get; set; } = new(7);
        public Section Tab7 { get; set; } = new(7);
        public Section Section8 { get; set; } = new(8);

        private bool _isSaveButtonVisible = false;
        public bool IsSaveButtonVisible
        {
            get { return _isSaveButtonVisible;}
            set
            {
                _isSaveButtonVisible = value;
                OnPropertyChanged();
            }
        }
        private bool _diferedChangeOver = false;
        public bool DiferedChangeOver
        {
            get { return _diferedChangeOver;}
            set
            {
                _diferedChangeOver = value;
                OnPropertyChanged();
            }
        }
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

        private string _materialBarcode1 = string.Empty;
        public string MaterialBarcode1
        {
            get { return _materialBarcode1; }
            set
            {
                if (_materialBarcode1 != value)
                {
                    _materialBarcode1 = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _materialBarcode2 = string.Empty;
        public string MaterialBarcode2
        {
            get { return _materialBarcode2; }
            set
            {
                if (_materialBarcode2 != value)
                {
                    _materialBarcode2 = value;
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

        private MTS _mts = new();

        private string _type = string.Empty;
        public string Type
        {
            get { return _type; }
            set
            {
                _type = value;
                OnPropertyChanged();
            }
        }

        private string _mouldingText = string.Empty;
        public string MouldingText
        {
            get { return _mouldingText; }
            set
            {
                _mouldingText = value;
                OnPropertyChanged();
            }
        }
        private string _changeOverTimer = "00:00:00";
        public string ChangeOverTimer
        {
            get { return _changeOverTimer; }
            set
            {
                _changeOverTimer = value;
                OnPropertyChanged();
            }
        }

        private bool _isCausesVisible = false;
        public bool IsCausesVisible
        {
            get { return _isCausesVisible;}
            set
            {
                _isCausesVisible = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ChangeOverProblem> ChangeOverProblems { get; set; } = new();
        private ChangeOverProblem _selectedChangeOverProblem;
        public ChangeOverProblem SelectedChangeOverProblem
        {
            get { return _selectedChangeOverProblem; }
            set
            {
                _selectedChangeOverProblem = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> EventCauses { get; set; } = new();
        public ObservableCollection<Section> Sections { get; set; }
        public ObservableCollection<Event> SelectedEvents { get; set; }
        public ObservableCollection<string> RunnerItems { get; set; } = new()
        {
            "Cold",
            "Hot",
        };
        public ObservableCollection<string> TemperatureItems { get; set; } = new()
        {
            "Down",
            "Same",
            "Up"
        };
        public ObservableCollection<string> RoboItems { get; set; } = new()
        {
            "Yes",
            "No", 
        };

        private string _selectedRunnerItem = string.Empty;
        public string SelectedRunnerItem
        {
            get { return _selectedRunnerItem; }
            set
            {
                _selectedRunnerItem = value;
                OnPropertyChanged();
            }
        }

        private string _selectedTemperatureItem = string.Empty;
        public string SelectedTemperatureItem
        {
            get { return _selectedTemperatureItem; }
            set
            {
                _selectedTemperatureItem = value;
                OnPropertyChanged();
            }
        }

        private string _selectedRoboItem = string.Empty;
        public string SelectedRoboItem
        {
            get { return _selectedRoboItem; }
            set
            {
                _selectedRoboItem = value;
                OnPropertyChanged();
            }
        }
        public ICommand AdvanceSection { get; }
        public ICommand RegressSection { get; }
        public ICommand ClickedEvent { get; }
        public ICommand SaveButtonPressed { get; }
        

        private Event _selectedEvent = new();
        public Event SelectedEvent
        {
            get => _selectedEvent;
            set
            { 
                _selectedEvent = value;
                if(value != null)
                { 

                    if (value.Id != -1)
                    { 
                        if (value.Id == 1)
                        {
                            TabsReactChangeOverEventStop();
                            return;
                        }
                        TabsReachEventStop();
                    }
                    else
                    {
                        ResetTabs();
                    }
                }
                else
                {
                    ResetTabs();
                }

                OnPropertyChanged();
            }
        }
        public void Unsubscribe()
        {
            MessagingCenter.Unsubscribe<object, string>(this, "ScannedEventTriggered");
        }

        private IDispatcherTimer _timer;
        private TimeSpan _watch = new TimeSpan(0,0,0);

        private bool _mandatoryIssue = false;
        public EventsViewModel(IDispatcher dispatcher)
        {
            ResetTabs(); 
            ChangeOverProblems = PYMS.GetChangeOverIssues();

            _timer = dispatcher.CreateTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(1000);
            _timer.Tick += (s, e) =>
            {
                _watch.Add(TimeSpan.FromSeconds(1));
                ChangeOverTimer = _watch.ToString();
            };

            SaveButtonPressed = new RelayCommand(() =>
            {
                AfterSavePressed();
            }); 

            var page = App.Current.MainPage;

            // Set Sections Enter Validation:
            // #1 - Initial Info
            Section1.ButtonClicked = new Func<bool>(() => 
            {
                // Find if informations are in the right format
                if (Machine.IsNullOrEmpty())
                {
                    page.DisplayAlert("Erro", $"Equipamento inválido!", "Ok");
                    return false;
                }

                if (Machine.Substring(0, 1).ToLower() != "p")
                {
                    page.DisplayAlert("Erro", $"Equipamento inválido!", "Ok");
                    return false;
                }

                if (Type.IsNullOrEmpty())
                {
                    page.DisplayAlert("Erro", $"Tipo inválido!", "Ok");
                    return false;
                }

                if (Type.Substring(0, 1).ToLower() != "s")
                {
                    page.DisplayAlert("Erro", $"Tipo inválido!", "Ok");
                    return false;
                }


                Type = Type.ToUpper();
                // Decide if its time to Move  to next section
                // if its START EVENT, go to Event Reading Section
                if (Type.ToLower() == "start")
                {
                    if (PYMS.HasOpenEvents(Machine))
                    {
                        page.DisplayAlert("Erro", $"Existem eventos abertos nesta máquina!", "Ok");
                        return false;
                    }

                    // go to Event Reading Section(3)
                    IsCausesVisible = false;
                    Section3.IsVisible = true;
                    SelectedEvent = new();
                    if (Section3.IsVisible) { Section1.IsVisible = false; }
                    TabsReactEventCreate();
                }

                if (Type.ToLower() == "stop")
                {
                    SelectedEvents = PYMS.GetOpenEvents(Machine);
                    OnPropertyChanged(nameof(SelectedEvents));

                    // If no events
                    if (SelectedEvents.Count == 0)
                    {
                        page.DisplayAlert("Erro", $"Não existem eventos abertos nesta máquina!", "Ok");
                        return false;
                    }

                    // otherwise is a EVENT STOP and then has to go for the Open Events section(2)
                    Section2.IsVisible = true;
                    if (Section2.IsVisible) { Section1.IsVisible = false; }
                }

                return true;
            });

            // #2 - Event Selection
            Section2.ButtonClicked = new Func<bool>(() =>
            {
                if(SelectedEvent.Id == -1)
                    page.DisplayAlert("Erro", $"Selecione um evento!", "Ok");
                else
                {
                    OnEventSelected(SelectedEvent);
                } 
                return false;
            });

            // #3 - Event Details
            Section3.ButtonClicked = new Func<bool>(() =>
            {
                if (SelectedEvent.Description.IsNullOrEmpty())
                {
                    page.DisplayAlert("Erro", $"Falta de informações sobre o evento!", "Ok");
                    return false;
                }

                // If closed cavities event(14), the extra info must supply the amount
                if (SelectedEvent.Id == 14)
                {
                    // If number of closed cavities is valid
                    if (!int.TryParse(SelectedEvent.Informations, out int resultParse))
                    {
                        page.DisplayAlert("Erro Informações Extra", "Erro na identificação de cavidades tapadas!", "Ok");
                        return false;
                    }
                    else
                    {
                        TabsReactEventCreateChangeOver();
                        // Redirect to production order reading
                        Section4.IsVisible = true;
                        Section3.IsVisible = false;
                        return true;
                    }
                }

                // Mold change
                if (SelectedEvent.Id == 1)
                {
                    TabsReactEventCreateChangeOver();
                    // Redirect to production order reading
                    Section4.IsVisible = true;
                    Section3.IsVisible = false;
                    return true;
                }
                else
                {
                    // if its any event than the handled go to save Section(8)
                    TabsReactEventCreateNormal();
                    Section8.IsVisible = true;
                    Section3.IsVisible = false;
                }
                return true;
            });

            // #4 - Production Order
            Section4.ButtonClicked = new Func<bool>(() =>
            {
                if (!SelectedProductionOrder.Exists)
                    page.DisplayAlert("Erro", $"Falta de informações da ordem de produção!", "Ok");
                else
                {
                    if (SelectedEvent.Id != 14)
                    {
                        // Move to section 5 - Verifications
                        Section5.IsVisible = true;
                        if (Section5.IsVisible) { Section4.IsVisible = false; }
                    }
                    else
                    {
                        // Move to final section
                        Section7.IsVisible = true;
                        if (Section7.IsVisible) { Section4.IsVisible = false; }
                    }
                }

                return false; 
            });

            // #5 - Verifications
            Section5.ButtonClicked = new Func<bool>(() =>
            {
                // If conditions met go to end
                if (!MaterialBarcode1.IsNullOrEmpty() && (!this.MaterialBarcode2.IsNullOrEmpty() || this.SelectedProductionOrder.Material2.IsNullOrEmpty()))
                {
                    if (!MouldingText.IsNullOrEmpty())
                    {
                        if (IsVerificationSectionValidated())
                        {
                            // Move to Save Section
                            Section8.IsVisible = true;
                            Section5.IsVisible = false;
                        }
                    }
                }
                return false;
            });

            // $6 - Change Over Settings
            Section6.ButtonClicked = new Func<bool>(() =>
            {
                Section6.IsVisible = false;
                Section7.IsVisible = true;
                TimeSpan watch = PYMS.GetCOTargetDiff(SelectedEvent.StopId, Machine, SelectedRunnerItem, SelectedTemperatureItem, SelectedRoboItem == "Yes");
                _watch = watch;

                _mandatoryIssue = !(watch.TotalSeconds > 0);
                ChangeOverTimer = _watch.ToString(); 
                return true;
            });

            // $7 Change Over Problems
            Section7.ButtonClicked = new Func<bool>(() =>
            {
                Section8.IsVisible = true;
                return true;
            }); 
             
            Sections = new ObservableCollection<Section>
            {
                Section1,
                Section2,
                Section3,
                Section4,
                Section5,
                Section6,
                Section7
            };

            Section1.IsVisible = true;

            AdvanceSection = new RelayCommand( (parameter) =>
            {
                if(parameter is Section section)
                {
                    var res = section.ButtonClicked();
                    if (!res) { return; }

                    int sectionDif = 1;

                    var nextSection = Sections.Where(x => x.Number == (section.Number + 1)).FirstOrDefault();
                    while (nextSection != null && !nextSection.IsPartOfTemplate)
                    {
                        sectionDif++;
                        nextSection = Sections.Where(x => x.Number == (section.Number + sectionDif)).FirstOrDefault();
                    }

                    if(nextSection != null) 
                        nextSection.IsVisible = true;
                }
            });

            RegressSection = new RelayCommand((parameter) =>
            {
                if (parameter is Section section)
                {
                    section.IsVisible = false;

                    int sectionDif = 1;

                    var previousSection = Sections.Where(x => x.Number == (section.Number - 1)).FirstOrDefault(); 
                    while(previousSection != null && !previousSection.IsPartOfTemplate)
                    {
                        sectionDif++;
                        previousSection = Sections.Where(x => x.Number == (section.Number - sectionDif)).FirstOrDefault();
                    }
                    if (previousSection != null) 
                        previousSection.IsVisible = true;

                    if(previousSection.Number == 1) { ResetTabs(); }
                    if(section.Number == 2) { SelectedEvent = new(); }
                }
            });

            MessagingCenter.Subscribe<object, string>(this, "ScannedEventTriggered", (sender, message) =>
            {
                ScanTriggered(message);
            });
        }

        public void ScanTriggered(string text)
        {

            MainThread.BeginInvokeOnMainThread(() =>
            {
                var page = App.Current.MainPage;
                // Initial Information
                if (Section1.IsActive)
                {
                    // If is machine
                    if (text.Substring(0, 1).ToLower() == "p")
                    {
                        Machine = text;
                    } // Else if is machine type
                    else if (text.Substring(0, 1).ToLower() == "s")
                    {
                        Type = text;
                    }
                    else { page.DisplayAlert("Erro", $"Scan Inválido!", "Ok"); return; }

                    // Decide if its time to Move  to next section
                    if(!Machine.IsNullOrEmpty() && !Type.IsNullOrEmpty())
                    {
                        // if its START EVENT, go to Event Reading Section
                        if (Type.ToLower() == "start")
                        {
                            // Check if Machine has open events
                            if (PYMS.HasOpenEvents(Machine))
                            {
                                page.DisplayAlert("Erro", $"Existem eventos abertos nesta máquina!", "Ok");
                                return;
                            }

                            // go to Event Reading Section(3)
                            IsCausesVisible = false;
                            Section3.IsVisible = true;
                            SelectedEvent = new();
                            TabsReactEventCreate();
                            if (Section3.IsVisible) { Section1.IsVisible = false; }
                            return;
                        }

                        if(Type.ToLower() == "stop")
                        {
                            SelectedEvents = PYMS.GetOpenEvents(Machine); 

                            OnPropertyChanged(nameof(SelectedEvents));

                            // If no events
                            if (SelectedEvents.Count == 0)
                            {
                                page.DisplayAlert("Erro", $"Não existem eventos abertos nesta máquina!", "Ok");
                                return;
                            }

                            // otherwise is a EVENT STOP and then has to go for the Open Events section(2)
                            Section2.IsVisible = true;
                            if (Section2.IsVisible) { Section1.IsVisible = false;}
                            return;
                        }
                    }
                    return;
                }
                
                // Event Details
                if(Section3.IsActive)
                {
                    if (Int32.TryParse(text, out int eventId))
                    {
                        SelectedEvent = new();
                        IsCausesVisible = false;
                        SelectedEvent.Id = eventId;
                        SelectedEvent.Description = PYMS.GetEventDescriptionByCode(text);

                        // Different types of events Change Mold
                        if(eventId == 1) 
                        {
                            TabsReactEventCreateChangeOver();
                            // Move to production Order Reading
                            Section4.IsVisible = true;
                            if (Section4.IsVisible) { Section3.IsVisible = false; }
                            return;
                        }

                        // Get Causes peripheral
                        if (eventId == 7)
                        {
                            TabsReactEventCreateNormal();
                            PYMS.GetCauses(eventId.ToString(), EventCauses);
                            OnPropertyChanged("EventCauses");
                            IsCausesVisible = true;
                            return;
                        }

                        // Cavities
                        if(eventId == 14)
                        {
                            TabsReactEventCreateCavities();
                            return;
                        }

                        // If they arent none of those events it sends to save
                        TabsReactEventCreateNormal();
                        Section8.IsVisible = true;
                        if (Section8.IsVisible) { Section3.IsVisible = false; }
                    }
                    else { page.DisplayAlert("Erro", $"Scan Inválido!", "Ok"); }
                    return;
                }

                // Production Order
                if (Section4.IsActive)
                {
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

                            if(SelectedEvent.Id != 14)
                            {
                                // Move to section 5 - Verifications
                                Section5.IsVisible = true;
                                if (Section5.IsVisible) { Section4.IsVisible = false; }
                            } 
                            else
                            {
                                // Move to final section
                                Section8.IsVisible = true; 
                                if (Section8.IsVisible) { Section4.IsVisible = false; }
                            }
                            
                        }
                    }
                    else { page.DisplayAlert("Erro", $"Scan Inválido!", "Ok"); }
                    return;
                }

                // Verifications
                if (Section5.IsActive)
                {
                    // Mould Text
                    if (text.ToLower().StartsWith("st")) 
                    {
                        MouldingText = text;
                    } 

                    // Granulate Text
                    if(text.Length == 35 || text.ToLower().StartsWith("sb"))
                    {
                        if (MaterialBarcode1.IsNullOrEmpty())
                        {
                            MaterialBarcode1 = text;
                        }
                        else
                        {
                            MaterialBarcode2 = text;
                        }
                    } 

                    // If conditions met go to end
                    if (!MaterialBarcode1.IsNullOrEmpty() && (!this.MaterialBarcode2.IsNullOrEmpty() || this.SelectedProductionOrder.Material2.IsNullOrEmpty()))
                    {
                        if (!MouldingText.IsNullOrEmpty())
                        {
                            if (IsVerificationSectionValidated())
                            {
                                // Move to Save Section
                                Section8.IsVisible = true;
                                Section5.IsVisible = false;
                            }
                        }
                    }

                }
            });
        }

        private void OnEventSelected(Event selectedEvent)
        {
            // If selected event countains change over (1)
            if(selectedEvent.Id == 1)
            { 
                DiferedChangeOver = true; 
                Section6.IsVisible = true;
                if (Section6.IsVisible) { Section2.IsVisible = false; }
                return;
            }

            // If it isnt a change over go to save section(8) 
            Section8.IsVisible = true;
            if (Section8.IsVisible) { Section2.IsVisible = false; } 
        }

        private void TabsReactChangeOverEventStop()
        {
            Section1.IsPartOfTemplate = true;
            Section3.IsPartOfTemplate = false;
            Section4.IsPartOfTemplate = false;
            Section5.IsPartOfTemplate = false;

            Section6.IsPartOfTemplate = true;
            Section7.IsPartOfTemplate = true;
        }

        private void TabsReachEventStop()
        {
            Section1.IsPartOfTemplate = true;
            Section2.IsPartOfTemplate = true;
            Section3.IsPartOfTemplate = false;
            Section4.IsPartOfTemplate = false;
            Section5.IsPartOfTemplate = false;
            Section6.IsPartOfTemplate = false;
            Section7.IsPartOfTemplate = false;
        }

        private void TabsReactEventCreateNormal()
        {
            Section1.IsPartOfTemplate = true;
            Section2.IsPartOfTemplate = false;
            Section3.IsPartOfTemplate = true;
            Section4.IsPartOfTemplate = false;
            Section5.IsPartOfTemplate = false;
            Section6.IsPartOfTemplate = false;
            Section7.IsPartOfTemplate = false;
        }
        private void TabsReactEventCreateChangeOver()
        {
            Section1.IsPartOfTemplate = true;
            Section2.IsPartOfTemplate = false;
            Section3.IsPartOfTemplate = true;
            Section4.IsPartOfTemplate = true;
            Section5.IsPartOfTemplate = true;
            Section6.IsPartOfTemplate = false;
            Section7.IsPartOfTemplate = false;
        }
        private void TabsReactEventCreateCavities()
        {
            Section1.IsPartOfTemplate = true;
            Section2.IsPartOfTemplate = false;
            Section3.IsPartOfTemplate = true;
            Section4.IsPartOfTemplate = true;
            Section5.IsPartOfTemplate = false;
            Section6.IsPartOfTemplate = false;
            Section7.IsPartOfTemplate = false;
        }
        private void ResetTabs()
        {
            Section1.IsPartOfTemplate = true;
            Section2.IsPartOfTemplate = true;
            Section3.IsPartOfTemplate = true;
            Section4.IsPartOfTemplate = true;
            Section5.IsPartOfTemplate = true;
            Section6.IsPartOfTemplate = false;
            Section7.IsPartOfTemplate = false;
        }

        private void TabsReactEventCreate()
        {
            Section1.IsPartOfTemplate = true;
            Section2.IsPartOfTemplate = false;
            Section3.IsPartOfTemplate = true;
            Section4.IsPartOfTemplate = true;
            Section5.IsPartOfTemplate = true;
            Section6.IsPartOfTemplate = false;
            Section7.IsPartOfTemplate = false;
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

        public bool IsVerificationSectionValidated()
        {
            var page = App.Current.MainPage;

            // Machine different than selected machine
            if (Machine != SelectedProductionOrder.Machine) 
            {
                page.DisplayAlert("Erro molde", $"Equipamento da Ordem diferente do equipamento selecionado!", "Ok");
                return false;
            }

            // Get the associated PartNumber of the barcode 1 and 2
            var partnumberBarcorde1 = MaterialBarcode1.Length == 11 ? _mts.UnitInfo(MaterialBarcode1).PN : MaterialBarcode1.Substring(4, 8);
            var partnumberBarcorde2 = MaterialBarcode2.IsNullOrEmpty() ? null :
            MaterialBarcode2.Length == 11 ? _mts.UnitInfo(MaterialBarcode2).PN : MaterialBarcode2.Substring(4, 8);

            if (MaterialBarcode1.Length == 35 || MaterialBarcode1.Length == 11)
            {
                // If its a a double injection material
                if (SelectedProductionOrder.Material2 == partnumberBarcorde1)
                {
                    // Barcodes Switch
                    string barcodeHolder = MaterialBarcode1;
                    MaterialBarcode1 = MaterialBarcode2;
                    MaterialBarcode2 = barcodeHolder;

                    // PartNumbers Switch 
                    string partnumberHolder = partnumberBarcorde1;
                    partnumberBarcorde1 = partnumberBarcorde2;
                    partnumberBarcorde2 = partnumberHolder;
                }

                // If the selected Barcode1 correspondent Partnumber doesn't match the found partNumber
                if (partnumberBarcorde1 != SelectedProductionOrder.Material) 
                {
                    page.DisplayAlert("Erro molde", $"O código de barras do material 1 não corresponde ao seu material!", "Ok");
                    return false;
                }
            }

            // If the selected Barcode2 correspondent Partnumber doesn't match the found partNumber
            if (MaterialBarcode2.Length == 35 || MaterialBarcode2.Length == 11)
            {
                if (partnumberBarcorde2 != SelectedProductionOrder.Material2)
                {
                    page.DisplayAlert("Erro molde", $"O código de barras do material 2 não corresponde ao seu material!", "Ok");
                    return false;
                }
            }

            // If selected mold is different from production order mold
            if (MouldingText != SelectedProductionOrder.ToolBarcode) 
            {
                page.DisplayAlert("Erro molde", $"O molde do scan não corresponde molde do equipamento desta ordem!", "Ok");
                return false;
            }

            // Check mold status
            var response = PYMS.CheckMouldStatus(SelectedProductionOrder.ToolNumber);

            if (response != "")
            {
                page.DisplayAlert("Erro molde", $"{response}", "Ok");
                return false; ;
            }

            // Check if granulate is already dried
            SelectedProductionOrder.GranulateBarcode = MaterialBarcode1;
            var dehumidifierFound = PYMS.GetLastDehumidifierOfGranulate(SelectedProductionOrder.GranulateBarcode);
            if (dehumidifierFound == "Not Found")
            {
                page.DisplayAlert("Erro", $"Erro de leitura das informações da estufa para este granulado!", "Ok");
                return false;
            }

            // Check granulate info from dehumidifier
            Dehumidifier dehumidifierSelected = new() { Name = dehumidifierFound };
            PYMS.SetLastGranulateInformationDehumidifierSpecificGranulate(dehumidifierSelected, SelectedProductionOrder.GranulateBarcode);

            if (!dehumidifierSelected.CanBeUsed)
            {
                if (dehumidifierSelected.GranulateBC.IsNullOrEmpty())
                {
                    page.DisplayAlert("Erro", $"O granulado não existe na base de dados! Verifique se foi colocado na estufa!", "Ok");
                    return false;
                }
                else
                {
                    if (DiferedChangeOver)
                    {
                        page.DisplayAlert("Erro", $"O Granulado ainda não se encontra seco! Mas o processo de Change Over continuará!", "Ok"); 
                    }
                    else
                    {
                        page.DisplayAlert("Erro", $"O granulado da estufa {dehumidifierSelected.Name} ainda não se encontra disponível!!", "Ok");
                        return false;
                    }
                }
            }
            return true;
        }

        public void AfterSavePressed()
        {
            var page = App.Current.MainPage; 
            try
            {
                string strResult; 

                // IT'S A START EVENT
                if (Type.ToUpper() == "START")
                {

                    strResult = PYMS.RecordEventStart(Machine, SelectedEvent.Id, (!SelectedEvent.Causes.IsNullOrEmpty()) ? SelectedEvent.Causes : SelectedEvent.Informations, SelectedProductionOrder);

                    if (strResult != "")
                    {
                        page.DisplayAlert("Erro", $"Erro na Gravação de Início de Evento!", "Ok");
                        return;
                    }
                    else
                    {
                        if (SelectedEvent.Id == 1) // CHANGE OVER
                        {
                            SelectedProductionOrder.GranulateBarcode = MaterialBarcode1;
                            SelectedProductionOrder.GranulateBarcode2 = MaterialBarcode2;
                            SelectedProductionOrder.ToolBarcode = MouldingText;

                            strResult = PYMS.UpdateGranulateAndTooling(SelectedProductionOrder); 
                            strResult = PYMS.StartOfChangeOver(SelectedProductionOrder);
                        } 
                    }
                }
                else   // IT'S A STOP EVENT
                {
                    int stop_id;

                    if (SelectedEvent.Id == 1) // If is change over
                    {

                        stop_id = SelectedEvent.StopId;

                        string runner = SelectedRunnerItem;
                        string temp = SelectedTemperatureItem;
                        bool robot = SelectedRoboItem == "Yes";

                        ChangeOverProblem item = SelectedChangeOverProblem;
                        byte? issue = null;

                        if (item != null && item.Id > 0) 
                            issue = Convert.ToByte(item.Id);
                        else
                        {
                            issue = 0;
                        }
                        
                        if (_mandatoryIssue)
                        {
                            SnackBarHandler.SendCustomMessage("Tempo de change over ultrapassou o objectivo");
                        }

                        PYMS.EndOfChangeOver(stop_id, Machine, runner, temp, robot, issue);
                    }
                    else
                    {
                        stop_id = SelectedEvent.StopId;

                        // Record END EVENT
                        strResult = PYMS.RecordEventStop(stop_id);

                        if (strResult != "")
                        {
                            page.DisplayAlert("Erro", $"Erro na Gravação de Fecho de Evento!", "Ok");
                            return;
                        }
                    }
                }

                SnackBarHandler.SendSuccessMessage();
                PageHandler.ClosePageByViewmodel((this));
            }
            catch (Exception ex)
            {
                page.DisplayAlert("Erro", $"Erro na Gravação de Fecho de Evento!\n" + ex.Message, "Ok");
                return;
            }
        }

    }
}
 