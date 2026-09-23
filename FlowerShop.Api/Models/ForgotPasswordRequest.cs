namespace FlowerShop.Api.Models
{
    public record ForgotPasswordRequest(
        string Email,
        string NewPassword,
        string ConfirmPassword
    );
}
