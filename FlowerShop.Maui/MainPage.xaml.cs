using CommunityToolkit.Maui.Views;
using FlowerShop.Maui.Models;
using FlowerShop.Maui.Services;
using FlowerShop.Maui.Pages;

namespace FlowerShop.Maui;

public partial class MainPage : ContentPage
{
    private readonly IProductService _productService;
    private readonly IAuthService _authService;

    public MainPage(IProductService productService, IAuthService authService)
    {
        InitializeComponent();
        _productService = productService;
        _authService = authService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadCategoriesAsync();
        await LoadFeaturedAsync();
        UpdateHeader();
    }

    private void UpdateHeader()
    {
        var fullname = Preferences.Get("user_fullname", string.Empty);
        var isLoggedIn = !string.IsNullOrEmpty(fullname);

        LoginLabel.IsVisible = !isLoggedIn;
        UserNameLabel.IsVisible = isLoggedIn;
        LogoutLabel.IsVisible = isLoggedIn;
        ChangePasswordLabel.IsVisible = isLoggedIn;

        if (isLoggedIn)
            UserNameLabel.Text = $"👤 {fullname}";
    }

    private async Task LoadCategoriesAsync()
    {
        var categories = await _productService.GetCategoriesAsync();
        CategoriesCollectionView.ItemsSource = categories;
    }

    private async Task LoadFeaturedAsync()
    {
        var products = await _productService.GetProductsAsync(null, null);
        // أول 6 منتجات كـ "الأكثر مبيعاً"
        FeaturedCollectionView.ItemsSource = products.Take(6).ToList();
    }

    // ضغط على كاتيغوري — يروح لـ ProductsPage مفلتر
    private async void OnCategoryTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is CategoryDto category)
        {
            var productsPage = Handler.MauiContext.Services.GetService<Pages.ProductsPage>();
            await Shell.Current.GoToAsync("//products");

            // نبعت الـ categoryId عبر Preferences مؤقتاً
            Preferences.Set("filter_category_id", category.Id);
            Preferences.Set("filter_category_name", category.Name);
        }
    }

    // ضغط على منتج من الـ Featured
    private async void OnFeaturedProductTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is ProductDto product)
        {
            var token = Preferences.Get("auth_token", string.Empty);
            if (string.IsNullOrEmpty(token))
            {
                bool goLogin = await DisplayAlert(
                    "Giriş Gerekli",
                    "Sipariş vermek için giriş yapmanız gerekiyor.",
                    "Giriş Yap", "İptal");
                if (goLogin)
                    await Shell.Current.GoToAsync("//login");
                return;
            }

            var popup = new Popups.ProductDetailPopup(product);
            var result = await this.ShowPopupAsync(popup);

            if (result is ProductDto selectedProduct)
            {
                var orderPage = Handler.MauiContext.Services.GetService<OrderPage>();
                orderPage.LoadProduct(selectedProduct);
                await Navigation.PushAsync(orderPage);
            }
        }
    }

    private async void OnCategoriesTapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("//products");
    }

    private async void OnShopNowClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//products");
    }

    private async void OnOrdersTapped(object sender, EventArgs e)
    {
        var token = Preferences.Get("auth_token", string.Empty);
        if (string.IsNullOrEmpty(token))
        {
            await Shell.Current.GoToAsync("//login");
            return;
        }
        var isAdmin = Preferences.Get("is_admin", false);
        if (isAdmin)
            await Shell.Current.GoToAsync("//adminorders");
        else
            await Shell.Current.GoToAsync("//myorders");
    }

    private async void OnLoginTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//login");
    }

    private async void OnChangePasswordTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//changepassword");
    }

    private async void OnLogoutTapped(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert(
            "Çıkış",
            "Çıkış yapmak istediğinize emin misiniz?",
            "Evet", "İptal");
        if (!confirm) return;

        await _authService.LogoutAsync();
        Preferences.Remove("auth_token");
        Preferences.Remove("user_fullname");
        Preferences.Remove("user_email");
        Preferences.Remove("user_id");
        Preferences.Remove("is_admin");
        UpdateHeader();
        await DisplayAlert("", "Başarıyla çıkış yapıldı. 🌸", "Tamam");
    }
}