using FlowerShop.Maui.Models;
using FlowerShop.Maui.Services;
using System.Net.Http.Json;
using System.Text.Json;

namespace FlowerShop.Maui.Pages;

public partial class AdminCategoriesPage : ContentPage
{
    private readonly IProductService _productService;
    private CategoryDto? _editingCategory = null;

    public AdminCategoriesPage(IProductService productService)
    {
        InitializeComponent();
        _productService = productService;

        ImageUrlEntry.TextChanged += (s, e) =>
        {
            var url = ImageUrlEntry.Text?.Trim();
            if (!string.IsNullOrWhiteSpace(url))
            {
                ImagePreview.Source = url;
                ImagePreview.IsVisible = true;
            }
            else
            {
                ImagePreview.IsVisible = false;
            }
        };
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
    }

    private async Task LoadCategoriesAsync()
    {
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        EmptyView.IsVisible = false;
        CategoriesCollection.IsVisible = false;

        var categories = await _productService.GetCategoriesAsync();

        LoadingIndicator.IsRunning = false;
        LoadingIndicator.IsVisible = false;

        if (categories == null || categories.Count == 0)
        {
            EmptyView.IsVisible = true;
        }
        else
        {
            CategoriesCollection.IsVisible = true;
            CategoriesCollection.ItemsSource = categories;
        }
    }

    private async void OnSaveCategoryClicked(object sender, EventArgs e)
    {
        FormMessageLabel.IsVisible = false;
        NameBorder.Stroke = Color.FromArgb("#F8BBD9");

        var name = NameEntry.Text?.Trim() ?? string.Empty;
        var desc = DescEntry.Text?.Trim();
        var imageUrl = ImageUrlEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            NameBorder.Stroke = Color.FromArgb("#D32F2F");
            ShowFormMessage("Kategori adı zorunludur.", isError: true);
            return;
        }

        if (name.Length < 2)
        {
            NameBorder.Stroke = Color.FromArgb("#D32F2F");
            ShowFormMessage("Kategori adı en az 2 karakter olmalıdır.", isError: true);
            return;
        }

        SaveButton.IsEnabled = false;
        bool success;

        if (_editingCategory != null)
        {
            _editingCategory.Name = name;
            _editingCategory.Description = desc;
            _editingCategory.ImageUrl = imageUrl;
            success = await _productService.UpdateCategoryAsync(_editingCategory);
        }
        else
        {
            var category = new CategoryDto
            {
                Name = name,
                Description = desc,
                ImageUrl = imageUrl
            };
            success = await _productService.AddCategoryAsync(category);
        }

        SaveButton.IsEnabled = true;

        if (success)
        {
            ClearForm();
            ShowFormMessage("İşlem başarılı! ✅", isError: false);
            await LoadCategoriesAsync();
        }
        else
        {
            ShowFormMessage("İşlem başarısız. Tekrar deneyin.", isError: true);
        }
    }

    private void OnEditCategoryClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is CategoryDto category)
        {
            _editingCategory = category;
            NameEntry.Text = category.Name;
            DescEntry.Text = category.Description;
            ImageUrlEntry.Text = category.ImageUrl;
            FormTitleLabel.Text = "Kategori Düzenle ✏️";
            CancelButton.IsVisible = true;
            ShowFormMessage($"'{category.Name}' düzenleniyor...", isError: false);
        }
    }

    private async void OnDeleteCategoryClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is int id)
        {
            bool confirm = await DisplayAlert(
                "Sil",
                "Bu kategoriyi silmek istediğinize emin misiniz?",
                "Evet", "İptal");

            if (!confirm) return;

            var success = await _productService.DeleteCategoryAsync(id);

            if (success)
            {
                ShowFormMessage("Kategori silindi. ✅", isError: false);
                await LoadCategoriesAsync();
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
        _editingCategory = null;
        NameEntry.Text = string.Empty;
        DescEntry.Text = string.Empty;
        ImageUrlEntry.Text = string.Empty;
        ImagePreview.IsVisible = false;
        FormTitleLabel.Text = "Yeni Kategori Ekle";
        CancelButton.IsVisible = false;
        NameBorder.Stroke = Color.FromArgb("#F8BBD9");
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