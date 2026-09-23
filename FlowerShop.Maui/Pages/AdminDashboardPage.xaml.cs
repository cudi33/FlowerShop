using FlowerShop.Maui.Services;

namespace FlowerShop.Maui.Pages;

public partial class AdminDashboardPage : ContentPage
{
    private readonly IOrderService _orderService;
    private readonly IAuthService _authService;

    public AdminDashboardPage(IOrderService orderService, IAuthService authService)
    {
        InitializeComponent();
        _orderService = orderService;
        _authService = authService;
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

        var fullName = Preferences.Get("user_fullname", string.Empty);
        WelcomeLabel.Text = $"Hoş geldiniz, {fullName} 👋";

        await LoadStatsAsync();
    }

    private async Task LoadStatsAsync()
    {
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;

        var stats = await _orderService.GetStatsAsync();

        LoadingIndicator.IsRunning = false;
        LoadingIndicator.IsVisible = false;

        if (stats != null)
        {
            TotalOrdersLabel.Text = stats.TotalOrders.ToString();
            PendingOrdersLabel.Text = stats.PendingOrders.ToString();
            TotalProductsLabel.Text = stats.TotalProducts.ToString();
            TotalUsersLabel.Text = stats.TotalUsers.ToString();
            TotalRevenueLabel.Text = $"{stats.TotalRevenue:F2} ₺";
        }
    }

    private async void OnOrdersTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//adminorders");
    }

    private async void OnProductsTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//adminproducts");
    }

    private async void OnCategoriesTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//admincategories");
    }

    private async void OnUsersTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//adminusers");
    }

    private async void OnLogoutTapped(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Çıkış", "Çıkış yapmak istediğinize emin misiniz?", "Evet", "İptal");
        if (!confirm) return;

        await _authService.LogoutAsync();

        Preferences.Remove("auth_token");
        Preferences.Remove("user_fullname");
        Preferences.Remove("user_email");
        Preferences.Remove("user_id");
        Preferences.Remove("is_admin");

        await Shell.Current.GoToAsync("//main");
    }
}