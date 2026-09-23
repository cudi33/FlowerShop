using FlowerShop.Maui.Services;

namespace FlowerShop.Maui.Pages;

public partial class ChangePasswordPage : ContentPage
{
    private readonly IAuthService _authService;

    public ChangePasswordPage(IAuthService authService)
    {
        InitializeComponent();
        _authService = authService;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ResetBorders();
        var token = Preferences.Get("auth_token", string.Empty);
        if (string.IsNullOrEmpty(token))
            Shell.Current.GoToAsync("//login");
    }

    private async void OnChangePasswordClicked(object sender, EventArgs e)
    {
        MessageLabel.IsVisible = false;
        ResetBorders();

        var oldPassword = OldPasswordEntry.Text ?? string.Empty;
        var newPassword = NewPasswordEntry.Text ?? string.Empty;
        var confirmPassword = ConfirmPasswordEntry.Text ?? string.Empty;

        bool isValid = true;

        if (string.IsNullOrWhiteSpace(oldPassword))
        {
            OldPasswordBorder.Stroke = Color.FromArgb("#D32F2F");
            ShowMessage("Mevcut şifre zorunludur.", isError: true);
            isValid = false;
        }

        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
        {
            NewPasswordBorder.Stroke = Color.FromArgb("#D32F2F");
            ShowMessage("Yeni şifre en az 6 karakter olmalıdır.", isError: true);
            isValid = false;
        }

        if (newPassword != confirmPassword)
        {
            ConfirmPasswordBorder.Stroke = Color.FromArgb("#D32F2F");
            ShowMessage("Şifreler eşleşmiyor.", isError: true);
            isValid = false;
        }

        if (isValid && oldPassword == newPassword)
        {
            NewPasswordBorder.Stroke = Color.FromArgb("#D32F2F");
            ShowMessage("Yeni şifre eskiyle aynı olamaz.", isError: true);
            return;
        }

        if (!isValid) return;

        SubmitButton.IsEnabled = false;
        SubmitButton.Text = "Güncelleniyor...";

        var success = await _authService.ChangePasswordAsync(oldPassword, newPassword);

        SubmitButton.IsEnabled = true;
        SubmitButton.Text = "Şifreyi Güncelle";

        if (success)
        {
            ShowMessage("Şifre başarıyla güncellendi! ✅", isError: false);
            await Task.Delay(1500);
            await Shell.Current.GoToAsync("//main");
        }
        else
        {
            OldPasswordBorder.Stroke = Color.FromArgb("#D32F2F");
            ShowMessage("Mevcut şifre hatalı.", isError: true);
        }
    }

    private void ShowMessage(string message, bool isError)
    {
        MessageLabel.Text = message;
        MessageLabel.TextColor = isError
            ? Color.FromArgb("#D32F2F")
            : Color.FromArgb("#388E3C");
        MessageLabel.IsVisible = true;
    }

    private void ResetBorders()
    {
        OldPasswordBorder.Stroke = Color.FromArgb("#F8BBD9");
        NewPasswordBorder.Stroke = Color.FromArgb("#F8BBD9");
        ConfirmPasswordBorder.Stroke = Color.FromArgb("#F8BBD9");
    }

    private async void OnBackTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//main");
    }
}