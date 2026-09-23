namespace FlowerShop.Api.Security;

public class CurrentUser
{
    public int? UserId { get; set; }
    public bool IsAuthenticated => UserId.HasValue;
}