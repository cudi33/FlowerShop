using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace FlowerShop.Maui.Models;

public class AdminOrderResponse
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public DateOnly DeliveryDate { get; set; }
    public bool IsSurprise { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
}