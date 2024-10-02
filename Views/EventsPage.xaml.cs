using PlasticsApp.ViewModels;

namespace PlasticsApp.Views;

public partial class EventsPage : ContentPage
{
    public EventsViewModel viewModel;
    public EventsPage()
	{
        InitializeComponent();
        viewModel = new EventsViewModel(this.Dispatcher);
        this.BindingContext = viewModel;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        ((EventsViewModel)this.BindingContext).Unsubscribe();
    }

    private void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {

    }
}