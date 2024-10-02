using PlasticsApp.ViewModels;

namespace PlasticsApp.Views;

public partial class DehumidifierPage : ContentPage
{
	public DehumidifierPage()
	{
        this.BindingContext = new DehumidifierViewModel(Navigation);
        InitializeComponent();
	}
}