using FlowerShop.Maui.Services;

namespace FlowerShop.Maui.Pages;

public partial class AdminUsersPage : ContentPage
{
    private readonly IUserService _userService;

    public AdminUsersPage(IUserService userService)
    {
        InitializeComponent();
        _userService = userService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadUsersAsync();
    }

    private async Task LoadUsersAsync()
    {
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        EmptyView.IsVisible = false;
        UsersCollection.IsVisible = false;

        var users = await _userService.GetAllUsersAsync();

        LoadingIndicator.IsRunning = false;
        LoadingIndicator.IsVisible = false;

        if (users == null || users.Count == 0)
        {
            EmptyView.IsVisible = true;
        }
        else
        {
            UsersCollection.IsVisible = true;
            UsersCollection.ItemsSource = users;
        }
    }

    private async void OnBackTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//admindashboard");
    }
}