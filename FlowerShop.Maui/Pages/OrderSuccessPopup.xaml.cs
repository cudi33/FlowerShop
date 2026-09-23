using CommunityToolkit.Maui.Views;

namespace FlowerShop.Maui.Popups;

public partial class OrderSuccessPopup : Popup
{
    public OrderSuccessPopup(string productName, decimal total)
    {
        InitializeComponent();
        ProductNameLabel.Text = productName;
        TotalLabel.Text = $"Toplam: {total:F2} ₺";
    }

    private void OnTamamClicked(object sender, EventArgs e)
    {
        Close();
    }
}