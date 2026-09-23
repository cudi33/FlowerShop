using CommunityToolkit.Maui.Views;

namespace FlowerShop.Maui.Pages;

public partial class StatusPopup : Popup
{
    public string? SelectedStatus { get; private set; }

    public StatusPopup(int orderId, string currentStatus)
    {
        InitializeComponent();
        OrderInfoLabel.Text = $"Sipariş #{orderId} — Mevcut: {currentStatus}";
    }

    private void OnStatusSelected(object sender, EventArgs e)
    {
        if (sender is Button btn)
        {
            SelectedStatus = btn.CommandParameter?.ToString();
            Close(SelectedStatus);
        }
    }

    private void OnCloseTapped(object sender, EventArgs e)
    {
        Close(null);
    }
}