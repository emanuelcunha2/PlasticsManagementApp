using PlasticsApp.Services;
using PlasticsApp.ViewModels;
namespace PlasticsApp;

public partial class MainPage : TabbedPage
{
    public MainPage()
    {
        this.BindingContext = new MainPageViewModel();
        InitializeComponent();

 
        Task.Run(() =>
        {
            Thread.Sleep(1500);
            var x = PYMS.GetFailureCodes();
        });
    }
}

