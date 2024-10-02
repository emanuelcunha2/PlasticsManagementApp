using PlasticsApp.ViewModels;

namespace PlasticsApp.Views;

public partial class DehumidifierLoadPage : ContentPage
{
	public DehumidifierLoadPage()
	{
        this.BindingContext = new DehumidifierLoadViewModel();
        InitializeComponent();
	}
    protected override void OnDisappearing()
    { 
        base.OnDisappearing();
        ((DehumidifierLoadViewModel)this.BindingContext).Unsubscribe();
    }
}