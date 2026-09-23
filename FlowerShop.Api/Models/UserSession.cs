namespace FlowerShop.Api.Models;

public class UserSession
{
    public int Id { get; set; }
    public int UserId { get; set; }

    public string Token { get; set; } = ""; // GUID
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }

    public User? User { get; set; }
}
