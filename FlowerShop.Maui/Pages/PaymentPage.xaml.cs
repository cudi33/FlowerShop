using FlowerShop.Maui.Models;
using FlowerShop.Maui.Services;
using CommunityToolkit.Maui.Views;

namespace FlowerShop.Maui.Pages;

public partial class PaymentPage : ContentPage
{
    private readonly IOrderService _orderService;
    private OrderRequest? _orderRequest;
    private string _productName = string.Empty;
    private decimal _totalPrice;

    public PaymentPage(IOrderService orderService)
    {
        InitializeComponent();
        _orderService = orderService;
    }

    public void LoadOrder(OrderRequest request, string productName, decimal totalPrice)
    {
        _orderRequest = request;
        _productName = productName;
        _totalPrice = totalPrice;
        ProductNameLabel.Text = $"🌸 {productName}";
        TotalPriceLabel.Text = $"Toplam: {totalPrice:F2} ₺";
    }

    private void OnPaymentMethodChanged(object sender, CheckedChangedEventArgs e)
    {
        if (!e.Value) return;
        if (OnlineRadio.IsChecked)
        {
            OnlinePaymentView.IsVisible = true;
            DeliveryTypeView.IsVisible = false;
        }
        else
        {
            OnlinePaymentView.IsVisible = false;
            DeliveryTypeView.IsVisible = true;
        }
    }

    private async void OnConfirmOrderClicked(object sender, EventArgs e)
    {
        MessageLabel.IsVisible = false;

        if (_orderRequest == null)
        {
            MessageLabel.Text = "Sipariş bilgisi bulunamadı.";
            MessageLabel.IsVisible = true;
            return;
        }

        if (OnlineRadio.IsChecked)
        {
            var cardNumber = CardNumberEntry.Text?.Replace(" ", "") ?? string.Empty;
            if (cardNumber.Length < 16)
            {
                MessageLabel.Text = "Geçerli bir kart numarası giriniz.";
                MessageLabel.IsVisible = true;
                return;
            }
            if (string.IsNullOrWhiteSpace(ExpiryEntry.Text))
            {
                MessageLabel.Text = "Son kullanma tarihini giriniz.";
                MessageLabel.IsVisible = true;
                return;
            }
            if (string.IsNullOrWhiteSpace(CvvEntry.Text) || CvvEntry.Text.Length < 3)
            {
                MessageLabel.Text = "Geçerli bir CVV giriniz.";
                MessageLabel.IsVisible = true;
                return;
            }
            if (string.IsNullOrWhiteSpace(CardHolderEntry.Text))
            {
                MessageLabel.Text = "Kart sahibinin adını giriniz.";
                MessageLabel.IsVisible = true;
                return;
            }
            _orderRequest.PaymentMethod = "Online";
        }
        else
        {
            _orderRequest.PaymentMethod = CashRadio.IsChecked ? "Cash" : "Card";
        }

        ConfirmButton.IsEnabled = false;
        ConfirmButton.Text = "İşleniyor...";

        var success = await _orderService.CreateOrderAsync(_orderRequest);

        ConfirmButton.IsEnabled = true;
        ConfirmButton.Text = "✅ Siparişi Onayla";

        if (success)
        {
            var popup = new Popups.OrderSuccessPopup(_productName, _totalPrice);
            await this.ShowPopupAsync(popup);
            await Shell.Current.GoToAsync("//products");
        }
        else
        {
            MessageLabel.Text = "Sipariş gönderilemedi. Lütfen tekrar deneyin.";
            MessageLabel.IsVisible = true;
        }
    }

    private async void OnBackTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}