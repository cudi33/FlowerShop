using FlowerShop.Maui.Models;
using FlowerShop.Maui.Services;

namespace FlowerShop.Maui.Pages;

public partial class OrderPage : ContentPage
{
    private readonly IOrderService _orderService;
    private ProductDto? _product;

    public OrderPage(IOrderService orderService)
    {
        InitializeComponent();
        _orderService = orderService;
    }

    public void LoadProduct(ProductDto product)
    {
        _product = product;
        ProductNameLabel.Text = product.Name;
        ProductCategoryLabel.Text = product.CategoryName;
        ProductPriceLabel.Text = $"{product.Price:N2} ₺";
        if (!string.IsNullOrEmpty(product.ImageUrl))
            ProductImage.Source = product.ImageUrl;
    }

    private async void OnOrderClicked(object sender, EventArgs e)
    {
        MessageLabel.IsVisible = false;
        QuantityBorder.BorderColor = Color.FromArgb("#F8BBD9");
        AddressBorder.BorderColor = Color.FromArgb("#F8BBD9");

        if (_product == null)
        {
            ShowError("Ürün seçilmedi.");
            return;
        }

        if (!int.TryParse(QuantityEntry.Text, out int quantity) || quantity <= 0)
        {
            QuantityBorder.BorderColor = Color.FromArgb("#D32F2F");
            ShowError("Geçerli bir adet giriniz (en az 1).");
            return;
        }

        if (quantity > 100)
        {
            QuantityBorder.BorderColor = Color.FromArgb("#D32F2F");
            ShowError("Maksimum 100 adet sipariş verebilirsiniz.");
            return;
        }

        var address = AddressEditor.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(address) || address.Length < 10)
        {
            AddressBorder.BorderColor = Color.FromArgb("#D32F2F");
            ShowError("Lütfen geçerli bir teslimat adresi giriniz (en az 10 karakter).");
            return;
        }

        if (DeliveryDatePicker.Date < DateTime.Today)
        {
            ShowError("Teslimat tarihi bugünden önce olamaz.");
            return;
        }

        // ✅ Items list مطابقة للـ API
        var request = new OrderRequest
        {
            Items = new List<OrderItemRequest>
            {
                new OrderItemRequest
                {
                    ProductId = _product.Id,
                    Quantity = quantity
                }
            },
            DeliveryAddress = address,
            DeliveryDate = DateOnly.FromDateTime(DeliveryDatePicker.Date),
            DeliveryTime = TimeOnly.FromTimeSpan(DeliveryTimePicker.Time),
            IsSurprise = IsSurpriseCheckBox.IsChecked,
            Notes = NotesEditor.Text?.Trim()
        };

        var paymentPage = Handler.MauiContext.Services.GetService<PaymentPage>();
        paymentPage.LoadOrder(request, _product.Name, _product.Price * quantity);
        await Navigation.PushAsync(paymentPage);
    }

    private void ShowError(string message)
    {
        MessageLabel.Text = message;
        MessageLabel.IsVisible = true;
    }

    private async void OnBackTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnProductsTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//products");
    }
}