using PlasticsApp.ViewModels;

namespace PlasticsApp.Views;

public partial class LineRejectionPage : TabbedPage
{
    public LineRejectionPage()
    {
        InitializeComponent();
        ((FtqViewModel)Page2.BindingContext).FirstTab = ((OrderViewModel)Page1.BindingContext);
    }
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        ((OrderViewModel)Page1.BindingContext).Unsubscribe();
        ((FtqViewModel)Page2.BindingContext).Unsubscribe();
    }
}
    