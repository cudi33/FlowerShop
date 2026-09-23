using System.Net.Http.Json;

namespace FlowerShop.Maui.Pages;

public partial class ForgotPasswordPage : ContentPage
{
    public ForgotPasswordPage()
    {
        InitializeComponent();
    }

    private async void OnResetPasswordClicked(object sender, EventArgs e)
    {
        MessageLabel.IsVisible = false;
        ResetBorders();

        var email = EmailEntry.Text?.Trim() ?? string.Empty;
        var newPassword = NewPasswordEntry.Text ?? string.Empty;
        var confirmPassword = ConfirmPasswordEntry.Text ?? string.Empty;

        bool isValid = true;

        if (string.IsNullOrWhiteSpace(email))
        {
            EmailBorder.BorderColor = Color.FromArgb("#D32F2F");
            ShowMessage("E-posta zorunludur.", isError: true);
            isValid = false;
        }

        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
        {
            NewPasswordBorder.BorderColor = Color.FromArgb("#D32F2F");
            ShowMessage("Yeni şifre en az 6 karakter olmalıdır.", isError: true);
            isValid = false;
        }

        if (newPassword != confirmPassword)
        {
            ConfirmPasswordBorder.BorderColor = Color.FromArgb("#D32F2F");
            ShowMessage("Şifreler eşleşmiyor.", isError: true);
            isValid = false;
        }

        if (!isValid) return;

        try
        {
            using var httpClient = new HttpClient();
            var request = new
            {
                Email = email,
                NewPassword = newPassword,
                ConfirmPassword = confirmPassword
            };

            var response = await httpClient.PostAsJsonAsync(
                "https://localhost:7052/auth/forgot-password", request);

            if (response.IsSuccessStatusCode)
            {
                ShowMessage("Şifreniz başarıyla güncellendi! ✅", isError: false);
                await Task.Delay(1500);
                await Shell.Current.GoToAsync("//login");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                ShowMessage(error.Trim('"'), isError: true);
            }
        }
        catch (Exception ex)
        {
            ShowMessage("Bağlantı hatası: " + ex.Message, isError: true);
        }
    }

    private void ResetBorders()
    {
        EmailBorder.BorderColor = Color.FromArgb("#F8BBD9");
        NewPasswordBorder.BorderColor = Color.FromArgb("#F8BBD9");
        ConfirmPasswordBorder.BorderColor = Color.FromArgb("#F8BBD9");
    }

    private void ShowMessage(string message, bool isError)
    {
        MessageLabel.Text = message;
        MessageLabel.TextColor = isError
            ? Color.FromArgb("#D32F2F")
            : Color.FromArgb("#388E3C");
        MessageLabel.IsVisible = true;
    }

    private async void OnBackTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//login");
    }

    private async void OnLoginTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//login");
    }
}