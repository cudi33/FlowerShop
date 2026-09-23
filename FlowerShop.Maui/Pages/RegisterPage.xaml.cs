using FlowerShop.Maui.Models;
using FlowerShop.Maui.Services;

namespace FlowerShop.Maui.Pages;

public partial class RegisterPage : ContentPage
{
    private readonly IAuthService _authService;

    public RegisterPage(IAuthService authService)
    {
        InitializeComponent();
        _authService = authService;
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        MessageLabel.IsVisible = false;
        ResetBorders();

        var fullName = FullNameEntry.Text?.Trim() ?? string.Empty;
        var email = EmailEntry.Text?.Trim() ?? string.Empty;
        var password = PasswordEntry.Text ?? string.Empty;
        var confirm = ConfirmPasswordEntry.Text ?? string.Empty;

        bool isValid = true;

        if (string.IsNullOrWhiteSpace(fullName) || fullName.Length < 3)
        {
            FullNameBorder.Stroke = Color.FromArgb("#D32F2F");
            ShowError("Ad Soyad en az 3 karakter olmalıdır.");
            isValid = false;
        }

        if (string.IsNullOrWhiteSpace(email) || !email.Contains("@") || !email.Contains("."))
        {
            EmailBorder.Stroke = Color.FromArgb("#D32F2F");
            ShowError("Geçerli bir e-posta adresi girin.");
            isValid = false;
        }

        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
        {
            PasswordBorder.Stroke = Color.FromArgb("#D32F2F");
            ShowError("Şifre en az 6 karakter olmalıdır.");
            isValid = false;
        }

        if (password != confirm)
        {
            ConfirmBorder.Stroke = Color.FromArgb("#D32F2F");
            ShowError("Şifreler eşleşmiyor.");
            isValid = false;
        }

        if (!isValid) return;

        var request = new RegisterRequest
        {
            FullName = fullName,
            Email = email,
            Password = password
        };

        var success = await _authService.RegisterAsync(request);

        if (success)
        {
            await DisplayAlert("✅ Başarılı", "Hesabınız oluşturuldu! Giriş yapabilirsiniz.", "Tamam");
            await Shell.Current.GoToAsync("//login");
        }
        else
        {
            ShowError("Kayıt başarısız. E-posta zaten kullanımda olabilir.");
            EmailBorder.Stroke = Color.FromArgb("#D32F2F");
        }
    }

    private void ShowError(string message)
    {
        MessageLabel.Text = message;
        MessageLabel.IsVisible = true;
    }

    private void ResetBorders()
    {
        FullNameBorder.Stroke = Color.FromArgb("#F8BBD9");
        EmailBorder.Stroke = Color.FromArgb("#F8BBD9");
        PasswordBorder.Stroke = Color.FromArgb("#F8BBD9");
        ConfirmBorder.Stroke = Color.FromArgb("#F8BBD9");
    }

    private async void OnLoginTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//login");
    }

    private async void OnBackTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//login");
    }
}