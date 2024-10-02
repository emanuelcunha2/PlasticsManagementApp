using PlasticsApp.ViewModels;

namespace PlasticsApp.Views;

public partial class RejectionFtqPage : ContentPage
{
    public FtqViewModel viewModel;
	public RejectionFtqPage()
	{
		InitializeComponent();
        viewModel = new FtqViewModel();
        this.BindingContext = viewModel; 
        viewModel.PropertyChanged += InvalidateTheMeasure;
    }
    public void InvalidateTheMeasure(object sender, EventArgs e)
    {
        this.InvalidateMeasure();
    }
    
}