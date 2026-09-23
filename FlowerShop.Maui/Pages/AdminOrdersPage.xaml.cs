using CommunityToolkit.Maui.Views;
using FlowerShop.Maui.Models;
using FlowerShop.Maui.Services;

namespace FlowerShop.Maui.Pages;

public partial class AdminOrdersPage : ContentPage
{
    private readonly IOrderService _orderService;

    public AdminOrdersPage(IOrderService orderService)
    {
        InitializeComponent();
        _orderService = orderService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var isAdmin = Preferences.Get("is_admin", false);
        if (!isAdmin)
        {
            await Shell.Current.GoToAsync("//main");
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

        var orders = await _orderService.GetAllOrdersAsync();

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

    private async void OnUpdateStatusTapped(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is AdminOrderResponse order)
        {
            var popup = new StatusPopup(order.Id, order.Status);
            var result = await this.ShowPopupAsync(popup);

            if (result is string newStatus)
            {
                var success = await _orderService.UpdateOrderStatusAsync(order.Id, newStatus);
                if (success)
                {
                    await DisplayAlert("✅", "Durum güncellendi.", "Tamam");
                    await LoadOrdersAsync();
                }
                else
                {
                    await DisplayAlert("Hata", "Güncelleme başarısız.", "Tamam");
                }
            }
        }
    }

    private async void OnBackTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//admindashboard");
    }
}