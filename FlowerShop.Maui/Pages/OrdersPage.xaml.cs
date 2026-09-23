using FlowerShop.Maui.Services;

namespace FlowerShop.Maui.Pages;

public partial class OrdersPage : ContentPage
{
    private readonly IOrderService _orderService;

    public OrdersPage(IOrderService orderService)
    {
        InitializeComponent();
        _orderService = orderService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var token = Preferences.Get("auth_token", string.Empty);
        if (string.IsNullOrEmpty(token))
        {
            await Shell.Current.GoToAsync("//login");
            return;
        }

        await LoadOrdersAsync();
    }

    private async Task LoadOrdersAsync()
    {
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        EmptyView.IsVisible = false;
        OrdersCollection.IsVisible = false;

        var orders = await _orderService.GetMyOrdersAsync();

        LoadingIndicator.IsRunning = false;
        LoadingIndicator.IsVisible = false;

        if (orders == null || orders.Count == 0)
        {
            EmptyView.IsVisible = true;
        }
        else
        {
            OrdersCollection.IsVisible = true;
            OrdersCollection.ItemsSource = orders;
        }
    }

    private async void OnBackTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//main");
    }

    private async void OnBrowseProductsTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//products");
    }
}