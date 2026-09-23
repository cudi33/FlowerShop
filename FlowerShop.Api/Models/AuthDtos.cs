namespace FlowerShop.Api.Models;

public record RegisterRequest(string FullName, string Email, string Password);
public record LoginRequest(string Email, string Password);
public record LoginResponse(string Token, int UserId, string FullName, string Email, bool IsAdmin);
public record ChangePasswordRequest(string OldPassword, string NewPassword);
