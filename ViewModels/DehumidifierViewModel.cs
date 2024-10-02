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
using CommunityToolkit.Maui.Alerts; 

namespace PlasticsApp.ViewModels
{
    public class DehumidifierViewModel
    {
        private readonly INavigation _navigation;
        public ObservableCollection<PageSelectionItem> DehumidifierPageItems { get; set; }
        public ICommand ClickedItem { get; }
        public DehumidifierViewModel(INavigation navigation)
        {
            _navigation = navigation;

            var newDehumidifierPageItemsCollection = new ObservableCollection<PageSelectionItem>
            {
                new PageSelectionItem(typeof(DehumidifierCreatePage)) { Designation = "Novo Material", ImageSource = "box" }, // Create Material
                new PageSelectionItem(typeof(DehumidifierLoadPage)) { Designation = "Carregamento", ImageSource = "trolley" }, // Load Material
            };

            DehumidifierPageItems = newDehumidifierPageItemsCollection;

            ClickedItem = new RelayCommand(async (parameter) =>
            {
                if (parameter is PageSelectionItem item)
                {
                    // Create a new instance of the destination page using reflection
                    Page destinationPage = Activator.CreateInstance(item.DestinationPage) as Page;

                    if (destinationPage != null)
                    {
                        await _navigation.PushAsync(destinationPage);
                    }
                }
                return;
            });
        }
    }
}
