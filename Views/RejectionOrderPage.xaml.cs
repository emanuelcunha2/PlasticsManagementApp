using PlasticsApp.ViewModels;

namespace PlasticsApp.Views;

public partial class RejectionOrderPage : ContentPage
{
    public OrderViewModel viewModel;
    public RejectionOrderPage()
	{
		InitializeComponent();
        viewModel = new OrderViewModel();
        this.BindingContext = viewModel;
        viewModel.PropertyChanged += InvalidateTheMeasure;
    }
    public void InvalidateTheMeasure(object sender, EventArgs e)
    {
        this.InvalidateMeasure();
    }
}