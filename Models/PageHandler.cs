using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using PlasticsApp.ViewModels;

namespace PlasticsApp.Models
{
    public static class PageHandler
    {
        public static void ClosePageByViewmodel(ViewModelBase viewmodel)
        {
            // Get the current navigation page
            var navigationPage = (NavigationPage)Application.Current.MainPage;

            // Navigate through Navigation stack to find the page
            foreach (var page in navigationPage.Navigation.NavigationStack)
            {
                if (page.BindingContext == viewmodel)
                {
                    navigationPage.Navigation.RemovePage(page);
                    break; // Exit loop once the page is found and removed
                }
            }
        }
    }
}
