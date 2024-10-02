using PlasticsApp.ViewModels;

namespace PlasticsApp.Views;

public partial class SuspectsPage : ContentPage
{
	public SuspectsPage()
	{
        this.BindingContext = new SuspectsViewModel();
        InitializeComponent();
	}
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        ((SuspectsViewModel)this.BindingContext).Unsubscribe();
    }
}