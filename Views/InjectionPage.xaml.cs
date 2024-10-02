using PlasticsApp.Models;
using PlasticsApp.ViewModels;
using System.Diagnostics;

namespace PlasticsApp.Views;

public partial class InjectionPage : ContentPage
{
	public InjectionPage()
	{ 
        this.BindingContext = new InjectionViewModel(Navigation);
        InitializeComponent();


	}
}