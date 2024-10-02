namespace PlasticsApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new NavigationPage(new MainPage()) { BarBackgroundColor = Color.FromRgb(255, 199, 13), BarTextColor = Color.FromRgb(51, 51, 51) };
    }
}
