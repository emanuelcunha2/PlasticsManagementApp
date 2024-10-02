using PlasticsApp.Commands;
using PlasticsApp.Models;
using PlasticsApp.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PlasticsApp.ViewModels
{
    public class InjectionViewModel : ViewModelBase
    {
        private readonly INavigation _navigation;
        public ObservableCollection<PageSelectionItem> InjectionPageItems { get; set; }
        public ICommand ClickedItem { get; }
        public InjectionViewModel(INavigation navigation)
        {
            _navigation = navigation;

            var newInjectionPageItemsCollection = new ObservableCollection<PageSelectionItem>
            {
                new PageSelectionItem(typeof(DehumidifierPage)) { Designation = "Desumificador", ImageSource = "desumidifier" }, // Dehumidifier
                new PageSelectionItem(typeof(SuspectsPage)) { Designation = "Suspeitos", ImageSource = "suspects" }, // Suspects
                new PageSelectionItem(typeof(LineRejectionPage)) { Designation = "Rejeição Linha", ImageSource = "rejection" }, // Rejection
                new PageSelectionItem(typeof(ToWipPage)) { Designation = "Para WIP", ImageSource = "towip" }, // To WIP
                new PageSelectionItem(typeof(ScrapPage)) { Designation = "Refugo", ImageSource = "scrap" }, // Scrap
                new PageSelectionItem(typeof(EventsPage)) { Designation = "Eventos", ImageSource = "megaphone" }, // Events
                new PageSelectionItem(typeof(EndOfLotPage)) { Designation = "Fim de Lote", ImageSource = "lot" } // End of lot
            };

            InjectionPageItems = newInjectionPageItemsCollection;

            ClickedItem = new RelayCommand(async (parameter) =>
            {
                try
                {
                    if (parameter is PageSelectionItem item)
                    {
                        // Create a new instance of the destination page using reflection
                        Page destinationPage = Activator.CreateInstance(item.DestinationPage) as Page;

                        if (destinationPage != null)
                        {
                            await _navigation.PushAsync(destinationPage, true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    var page = App.Current.MainPage;
                    await page.DisplayAlert("Erro", $"{ex.Message} +  {ex.InnerException}", "Ok");
                }
 
                return;
            });
        }

    }
}
