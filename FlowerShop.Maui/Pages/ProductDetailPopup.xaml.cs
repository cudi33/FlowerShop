using CommunityToolkit.Maui.Views;
using FlowerShop.Maui.Models;

namespace FlowerShop.Maui.Popups;

public partial class ProductDetailPopup : Popup
{
    private readonly ProductDto _product;

    public ProductDetailPopup(ProductDto product)
    {
        InitializeComponent();
        _product = product;

        ProductNameLabel.Text = product.Name;
        CategoryLabel.Text = product.CategoryName;
        PriceLabel.Text = $"{product.Price:N2} ₺";
        StockLabel.Text = $"Stok: {product.StockQuantity} adet";

        if (!string.IsNullOrEmpty(product.ImageUrl))
            ProductImage.Source = product.ImageUrl;
    }

    private void OnCloseClicked(object sender, EventArgs e)
    {
        Close(null);
    }

    private void OnOrderClicked(object sender, EventArgs e)
    {
        Close(_product);
    }
}