using PlasticsApp.ViewModels;

namespace PlasticsApp.Views;

public partial class ToWipPage : ContentPage
{
	public ToWipPage()
	{
        this.BindingContext = new ToWipViewModel();
        InitializeComponent();
	}

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        ((ToWipViewModel)this.BindingContext).Unsubscribe();
    }

}