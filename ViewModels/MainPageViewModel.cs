using PlasticsApp.Commands;
using PlasticsApp.Models;
using PlasticsApp.Services;
using PlasticsApp.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PlasticsApp.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        private readonly INavigation _navigation;
        public ObservableCollection<PageSelectionItem> MainPageItems { get; set; } = new();
        public MainPageViewModel() 
        {  
            // Initialize the existent main page items
            MainPageItems.Add(new PageSelectionItem(typeof(InjectionPage)) { Designation = "Injecção", ImageSource = "injection"}); // Injection
            MainPageItems.Add(new PageSelectionItem(typeof(InjectionPage)) { Designation = "Pintura", ImageSource = "painting" }); // Painting
            MainPageItems.Add(new PageSelectionItem(typeof(InjectionPage)) { Designation = "Montagem Final", ImageSource = "assembly" }); // Final Assembly
            MainPageItems.Add(new PageSelectionItem(typeof(InjectionPage)) { Designation = "Armazém", ImageSource = "forklift" }); // Warehouse
        }       
    }
}
