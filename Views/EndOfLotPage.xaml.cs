using PlasticsApp.ViewModels;

namespace PlasticsApp.Views;

public partial class EndOfLotPage : ContentPage
{
	public EndOfLotPage()
    {
        this.BindingContext = new EndOfLotViewModel();
        InitializeComponent();
	} 
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        ((EndOfLotViewModel)this.BindingContext).Unsubscribe();
    }

}