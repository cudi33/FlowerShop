using FlowerShop.Maui.Models;
using FlowerShop.Maui.Services;

namespace FlowerShop.Maui.Pages;

public partial class LoginPage : ContentPage
{
    private readonly IAuthService _authService;

    public LoginPage(IAuthService authService)
    {
        InitializeComponent();
        _authService = authService;
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        // إخفاء الخطأ أول
        MessageLabel.IsVisible = false;
        ResetBorders();

        var email = EmailEntry.Text?.Trim() ?? string.Empty;
        var password = PasswordEntry.Text ?? string.Empty;

        // Validation
        bool isValid = true;

        if (string.IsNullOrWhiteSpace(email))
        {
            EmailBorder.BorderColor = Color.FromArgb("#D32F2F");
            isValid = false;
        }
        else if (!email.Contains("@") || !email.Contains("."))
        {
            EmailBorder.BorderColor = Color.FromArgb("#D32F2F");
            ShowError("Geçerli bir e-posta adresi girin.");
            return;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            PasswordBorder.BorderColor = Color.FromArgb("#D32F2F"); 
            isValid = false;
        }
        else if (password.Length < 6)
        {
            PasswordBorder.BorderColor = Color.FromArgb("#D32F2F");
            ShowError("Şifre en az 6 karakter olmalıdır.");
            return;
        }

        if (!isValid)
        {
            ShowError("Lütfen tüm alanları doldurun.");
            return;
        }

        try
        {
            var request = new LoginRequest { Email = email, Password = password };
            var result = await _authService.LoginAsync(request);

            if (result == null)
            {
                ShowError("E-posta veya şifre hatalı.");
                EmailBorder.BorderColor = Color.FromArgb("#D32F2F");
                PasswordBorder.BorderColor = Color.FromArgb("#D32F2F");
                return;
            }

            Preferences.Set("auth_token", result.Token);
            Preferences.Set("user_fullname", result.FullName);
            Preferences.Set("user_email", result.Email);
            Preferences.Set("user_id", result.Id);
            Preferences.Set("is_admin", result.IsAdmin);


            if (result.IsAdmin)
                await Shell.Current.GoToAsync("//admindashboard");
            else
                await Shell.Current.GoToAsync("//main");
        }
        catch (Exception ex)
        {
            ShowError($"Bir hata oluştu: {ex.Message}");
        }
    }

    private void ShowError(string message)
    {
        MessageLabel.Text = message;
        MessageLabel.IsVisible = true;
    }

    private void ResetBorders()
    {
        EmailBorder.BorderColor = Color.FromArgb("#F8BBD9");
        PasswordBorder.BorderColor = Color.FromArgb("#F8BBD9");
    }

    private async void OnHomeTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//main");
    }

    private async void OnRegisterTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//register");
    }
    private async void OnForgotPasswordTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//forgotpassword");
    }
}