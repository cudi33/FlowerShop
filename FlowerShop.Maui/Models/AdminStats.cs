using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowerShop.Maui.Models;

public class AdminStats
{
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int TotalProducts { get; set; }
    public int TotalUsers { get; set; }
    public decimal TotalRevenue { get; set; }
}