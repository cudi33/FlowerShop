namespace FlowerShop.Api.Models;

public class User : BaseEntity
{
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";

    public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
    public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();

    public bool IsActive { get; set; } = true;
    public bool IsAdmin { get; set; } = false;

    public List<UserSession> Sessions { get; set; } = new();
}