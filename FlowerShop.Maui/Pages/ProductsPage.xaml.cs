using CommunityToolkit.Maui.Views;
using FlowerShop.Maui.Models;
using FlowerShop.Maui.Services;

namespace FlowerShop.Maui.Pages;

public partial class ProductsPage : ContentPage
{
    private readonly IProductService _productService;
    private List<CategoryDto> _categories = new();

    public ProductsPage(IProductService productService)
    {
        InitializeComponent();
        _productService = productService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadCategoriesAsync();

        // ✅ تحقق إذا في فلتر من MainPage
        var filterId = Preferences.Get("filter_category_id", 0);
        if (filterId != 0)
        {
            // اختار الكاتيغوري تلقائياً
            var cat = _categories.FirstOrDefault(c => c.Id == filterId);
            if (cat != null)
                CategoryPicker.SelectedItem = cat;

            // امسح الفلتر
            Preferences.Remove("filter_category_id");
            Preferences.Remove("filter_category_name");
        }
        else
        {
            await LoadProductsAsync();
        }
    }

    private async Task LoadCategoriesAsync()
    {
        _categories = await _productService.GetCategoriesAsync();
        var list = new List<CategoryDto>
        {
            new CategoryDto { Id = 0, Name = "Tümü" }
        };
        list.AddRange(_categories);
        CategoryPicker.ItemsSource = list;
        CategoryPicker.SelectedIndex = 0;
    }

    private async Task LoadProductsAsync()
    {
        var search = ProductSearchBar.Text?.Trim();
        int? categoryId = null;
        if (CategoryPicker.SelectedItem is CategoryDto selectedCategory && selectedCategory.Id != 0)
            categoryId = selectedCategory.Id;

        var products = await _productService.GetProductsAsync(search, categoryId);
        ProductsCollectionView.ItemsSource = products;
    }

    private async void OnSearchClicked(object sender, EventArgs e)
    {
        await LoadProductsAsync();
    }

    private async void OnCategoryChanged(object sender, EventArgs e)
    {
        if (CategoryPicker.SelectedIndex == -1)
            return;
        await LoadProductsAsync();
    }

    private async void OnOrderProductClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is ProductDto product)
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

            // ✅ أظهر الـ Popup أولاً
            var popup = new Popups.ProductDetailPopup(product);
            var result = await this.ShowPopupAsync(popup);

            // لو ضغط "Sipariş Ver"
            if (result is ProductDto selectedProduct)
            {
                var orderPage = Handler.MauiContext.Services.GetService<OrderPage>();
                orderPage.LoadProduct(selectedProduct);
                await Navigation.PushAsync(orderPage);
            }
        }
    }

    private async void OnBackTapped(object sender, EventArgs e)
    {
        var isAdmin = Preferences.Get("is_admin", false);
        if (isAdmin)
            await Shell.Current.GoToAsync("//admindashboard");
        else
            await Shell.Current.GoToAsync("//main");
    }
}