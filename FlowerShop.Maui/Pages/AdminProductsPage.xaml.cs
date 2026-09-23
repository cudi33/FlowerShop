using FlowerShop.Maui.Models;
using FlowerShop.Maui.Services;
using System.Text.Json;

namespace FlowerShop.Maui.Pages;

public partial class AdminProductsPage : ContentPage
{
    private readonly IProductService _productService;
    private ProductDto? _editingProduct = null;
    private List<CategoryDto> _categories = new();

    public AdminProductsPage(IProductService productService)
    {
        InitializeComponent();
        _productService = productService;
    }

    private async void OnSelectImageClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                FileTypes = FilePickerFileType.Images
            });

            if (result == null) return;

            var token = Preferences.Get("auth_token", string.Empty);
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("X-Auth-Token", token);

            using var stream = await result.OpenReadAsync();
            using var content = new MultipartFormDataContent();
            using var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
                result.ContentType ?? "image/jpeg");
            content.Add(streamContent, "file", result.FileName);

            var response = await httpClient.PostAsync("https://localhost:7052/upload", content);

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var json = JsonSerializer.Deserialize<UploadResponse>(jsonString,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (json != null)
                {
                    ImageUrlEntry.Text = json.ImageUrl;
                    ImagePreview.Source = json.ImageUrl;
                    ImagePreview.IsVisible = true;
                }
            }
            else
            {
                await DisplayAlert("Hata", "Resim yüklenemedi.", "Tamam");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", ex.Message, "Tamam");
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadCategoriesAsync();
        await LoadProductsAsync();
    }

    private async Task LoadCategoriesAsync()
    {
        _categories = await _productService.GetCategoriesAsync();
        CategoryPicker.ItemsSource = _categories;
    }

    private async Task LoadProductsAsync()
    {
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        EmptyView.IsVisible = false;
        ProductsCollection.IsVisible = false;

        var products = await _productService.GetProductsAsync();

        LoadingIndicator.IsRunning = false;
        LoadingIndicator.IsVisible = false;

        if (products == null || products.Count == 0)
        {
            EmptyView.IsVisible = true;
        }
        else
        {
            ProductsCollection.IsVisible = true;
            ProductsCollection.ItemsSource = products;
        }
    }

    private async void OnSaveProductClicked(object sender, EventArgs e)
    {
        FormMessageLabel.IsVisible = false;
        ResetBorders();

        var name = NameEntry.Text?.Trim() ?? string.Empty;
        var priceText = PriceEntry.Text?.Trim() ?? string.Empty;
        var stockText = StockEntry.Text?.Trim() ?? string.Empty;
        var imageUrl = ImageUrlEntry.Text?.Trim();
        var selectedCategory = CategoryPicker.SelectedItem as CategoryDto;

        bool isValid = true;

        if (string.IsNullOrWhiteSpace(name) || name.Length < 2)
        {
            NameBorder.Stroke = Color.FromArgb("#D32F2F");
            ShowFormMessage("Ürün adı en az 2 karakter olmalıdır.", isError: true);
            isValid = false;
        }

        if (!decimal.TryParse(priceText, out decimal price) || price <= 0)
        {
            PriceBorder.Stroke = Color.FromArgb("#D32F2F");
            ShowFormMessage("Geçerli bir fiyat giriniz.", isError: true);
            isValid = false;
        }

        if (!int.TryParse(stockText, out int stock) || stock < 0)
        {
            StockBorder.Stroke = Color.FromArgb("#D32F2F");
            ShowFormMessage("Geçerli bir stok miktarı giriniz.", isError: true);
            isValid = false;
        }

        if (selectedCategory == null)
        {
            CategoryBorder.Stroke = Color.FromArgb("#D32F2F");
            ShowFormMessage("Lütfen bir kategori seçiniz.", isError: true);
            isValid = false;
        }

        if (!isValid) return;

        SaveButton.IsEnabled = false;
        bool success;

        if (_editingProduct != null)
        {
            _editingProduct.Name = name;
            _editingProduct.Price = price;
            _editingProduct.StockQuantity = stock;
            _editingProduct.CategoryId = selectedCategory!.Id;
            _editingProduct.ImageUrl = imageUrl;
            success = await _productService.UpdateProductAsync(_editingProduct);
        }
        else
        {
            var product = new ProductDto
            {
                Name = name,
                Price = price,
                StockQuantity = stock,
                CategoryId = selectedCategory!.Id,
                ImageUrl = imageUrl
            };
            success = await _productService.AddProductAsync(product);
        }

        SaveButton.IsEnabled = true;

        if (success)
        {
            ClearForm();
            ShowFormMessage("İşlem başarılı! ✅", isError: false);
            await LoadProductsAsync();
        }
        else
        {
            ShowFormMessage("İşlem başarısız. Tekrar deneyin.", isError: true);
        }
    }

    private void OnEditProductClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is ProductDto product)
        {
            _editingProduct = product;
            NameEntry.Text = product.Name;
            PriceEntry.Text = product.Price.ToString();
            StockEntry.Text = product.StockQuantity.ToString();
            ImageUrlEntry.Text = product.ImageUrl;
            CategoryPicker.SelectedItem = _categories.FirstOrDefault(c => c.Id == product.CategoryId);
            FormTitleLabel.Text = "Ürün Düzenle ✏️";
            CancelButton.IsVisible = true;
            ShowFormMessage($"'{product.Name}' düzenleniyor...", isError: false);
        }
    }

    private async void OnDeleteProductClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is int id)
        {
            bool confirm = await DisplayAlert(
                "Sil",
                "Bu ürünü silmek istediğinize emin misiniz?",
                "Evet", "İptal");

            if (!confirm) return;

            var success = await _productService.DeleteProductAsync(id);

            if (success)
            {
                ShowFormMessage("Ürün silindi. ✅", isError: false);
                await LoadProductsAsync();
            }
            else
            {
                await DisplayAlert("Hata", "Silme işlemi başarısız.", "Tamam");
            }
        }
    }

    private void OnCancelEditClicked(object sender, EventArgs e)
    {
        ClearForm();
    }

    private void ClearForm()
    {
        _editingProduct = null;
        NameEntry.Text = string.Empty;
        PriceEntry.Text = string.Empty;
        StockEntry.Text = string.Empty;
        ImageUrlEntry.Text = string.Empty;
        ImagePreview.IsVisible = false;
        CategoryPicker.SelectedItem = null;
        FormTitleLabel.Text = "Yeni Ürün Ekle";
        CancelButton.IsVisible = false;
        ResetBorders();
    }

    private void ResetBorders()
    {
        NameBorder.Stroke = Color.FromArgb("#F8BBD9");
        PriceBorder.Stroke = Color.FromArgb("#F8BBD9");
        StockBorder.Stroke = Color.FromArgb("#F8BBD9");
        CategoryBorder.Stroke = Color.FromArgb("#F8BBD9");
    }

    private void ShowFormMessage(string message, bool isError)
    {
        FormMessageLabel.Text = message;
        FormMessageLabel.TextColor = isError
            ? Color.FromArgb("#D32F2F")
            : Color.FromArgb("#388E3C");
        FormMessageLabel.IsVisible = true;
    }

    private async void OnBackTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//admindashboard");
    }
}