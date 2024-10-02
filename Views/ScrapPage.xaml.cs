using PlasticsApp.ViewModels;

namespace PlasticsApp.Views;

public partial class ScrapPage : ContentPage
{
	public ScrapPage()
    {
        this.BindingContext = new ScrapViewModel();
        InitializeComponent();
	}
     
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        ((ScrapViewModel)this.BindingContext).Unsubscribe();
    }

}