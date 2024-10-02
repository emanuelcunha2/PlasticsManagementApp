using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using PlasticsApp.ViewModels;

namespace PlasticsApp.Views;

public partial class DehumidifierCreatePage : ContentPage
{
	public DehumidifierCreatePage()
	{
        this.BindingContext = new DehumidifierCreateViewModel();
        InitializeComponent();
	}
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        ((DehumidifierCreateViewModel)this.BindingContext).Unsubscribe();
    }

 
}