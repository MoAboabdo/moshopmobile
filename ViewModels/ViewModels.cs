using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ShopApp.Mobile.DTOs;
using ShopApp.Mobile.Services;
using System.Collections.ObjectModel;

namespace ShopApp.Mobile.ViewModels;

public class ProductsChangedMessage { }

// ─── Product List ────────────────────────────────────────────
public partial class ProductListViewModel : BaseViewModel
{
    private readonly IProductApiService _productApi;
    private readonly ICartApiService _cartApi;
    private readonly IAuthStateService _authState;

    [ObservableProperty] private ObservableCollection<ProductDto> _products = new();
    [ObservableProperty] private ObservableCollection<string> _categories = new();
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private string _selectedCategory = string.Empty;
    [ObservableProperty] private int _currentPage = 1;
    [ObservableProperty] private int _totalPages = 1;
    [ObservableProperty] private bool _hasNextPage;
    [ObservableProperty] private bool _hasPreviousPage;

    // Permission flags
    [ObservableProperty] private bool _canCreate;
    [ObservableProperty] private bool _canEdit;
    [ObservableProperty] private bool _canDelete;

    // Cart badge
    [ObservableProperty] private int _cartItemCount;
    [ObservableProperty] private bool _hasCartItems;
    [ObservableProperty] private string _cartBadgeText = string.Empty;

    public ProductListViewModel(IProductApiService productApi, ICartApiService cartApi, IAuthStateService authState)
    {
        _productApi = productApi;
        _cartApi = cartApi;
        _authState = authState;
        CanCreate = authState.HasPermission("CreateProduct");
        CanEdit = authState.HasPermission("UpdateProduct");
        CanDelete = authState.HasPermission("DeleteProduct");

        WeakReferenceMessenger.Default.Register<ProductsChangedMessage>(this, (_, _) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
                LoadProductsCommand.ExecuteAsync(null));
        });
    }
    public void Dispose()
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }

    [RelayCommand]
    public async Task LoadProductsAsync()
    {
        await ExecuteAsync(async () =>
        {
            var query = new ProductQueryDto
            {
                Search = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText,
                Category = string.IsNullOrWhiteSpace(SelectedCategory) ? null : SelectedCategory,
                Page = CurrentPage,
                PageSize = 10
            };

            var result = await _productApi.GetProductsAsync(query);
            if (result?.Success == true && result.Data != null)
            {
                Products = new ObservableCollection<ProductDto>(result.Data.Items);
                TotalPages = result.Data.TotalPages;
                HasNextPage = CurrentPage < TotalPages;
                HasPreviousPage = CurrentPage > 1;
            }

            var catResult = await _productApi.GetCategoriesAsync();
            if (catResult?.Success == true && catResult.Data != null)
                Categories = new ObservableCollection<string>(catResult.Data.Prepend("All"));

            await RefreshCartBadgeAsync();
        });
    }

    private async Task RefreshCartBadgeAsync()
    {
        try
        {
            var cart = await _cartApi.GetCartAsync();
            if (cart?.Success == true && cart.Data != null)
            {
                CartItemCount = cart.Data.Items.Sum(i => i.Quantity);
                HasCartItems = CartItemCount > 0;
                CartBadgeText = CartItemCount > 0 ? CartItemCount.ToString() : string.Empty;
            }
        }
        catch { }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        CurrentPage = 1;
        await LoadProductsAsync();
    }

    [RelayCommand]
    private async Task FilterByCategoryAsync(string category)
    {
        SelectedCategory = category == "All" ? string.Empty : category;
        CurrentPage = 1;
        await LoadProductsAsync();
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (CurrentPage < TotalPages) { CurrentPage++; await LoadProductsAsync(); }
    }

    [RelayCommand]
    private async Task PreviousPageAsync()
    {
        if (CurrentPage > 1) { CurrentPage--; await LoadProductsAsync(); }
    }

    [RelayCommand]
    private async Task ViewProductAsync(ProductDto product) =>
        await Shell.Current.GoToAsync($"productdetail?id={product.Id}");

    
    [RelayCommand]
    private async Task GoToCartAsync() =>
        await Shell.Current.GoToAsync("//cart");

    [RelayCommand]
    private async Task AddProductAsync() =>
        await Shell.Current.GoToAsync("addproduct");

    [RelayCommand]
    private async Task EditProductAsync(ProductDto product) =>
        await Shell.Current.GoToAsync($"editproduct?id={product.Id}");

    [RelayCommand]
    private async Task DeleteProductAsync(ProductDto product)
    {
        var confirm = await Shell.Current.DisplayAlert(
            "Delete Product",
            $"Are you sure you want to delete \"{product.Name}\"?",
            "Delete", "Cancel");
        if (!confirm) return;

        await ExecuteAsync(async () =>
        {
            var result = await _productApi.DeleteProductAsync(product.Id);
            if (result?.Success == true)
            {
                await Shell.Current.DisplayAlert("Deleted", "Product deleted.", "OK");
                await LoadProductsAsync();
            }
            else SetError(result?.Message ?? "Failed to delete.");
        });
    }

    [RelayCommand]
    private async Task AddToCartAsync(ProductDto product)
    {
        await ExecuteAsync(async () =>
        {
            var result = await _cartApi.AddToCartAsync(new AddToCartDto(product.Id, 1));
            if (result?.Success == true)
            {
                await RefreshCartBadgeAsync();
                await Shell.Current.DisplayAlert("Added ✓", $"{product.Name} added to cart!", "OK");
            }
            else
                SetError(result?.Message ?? "Failed to add to cart.");
        });
    }
}

// ─── Product Detail ──────────────────────────────────────────
[QueryProperty(nameof(ProductId), "id")]
public partial class ProductDetailViewModel : BaseViewModel
{
    private readonly IProductApiService _productApi;
    private readonly ICartApiService _cartApi;
    private readonly IAuthStateService _authState;

    [ObservableProperty] private int _productId;
    [ObservableProperty] private ProductDto? _product;
    [ObservableProperty] private int _quantity = 1;
    [ObservableProperty] private bool _canEdit;
    [ObservableProperty] private bool _canDelete;

    public ProductDetailViewModel(IProductApiService productApi, ICartApiService cartApi, IAuthStateService authState)
    {
        _productApi = productApi;
        _cartApi = cartApi;
        _authState = authState;
        CanEdit = authState.HasPermission("UpdateProduct");
        CanDelete = authState.HasPermission("DeleteProduct");
    }

    partial void OnProductIdChanged(int value) => LoadProductCommand.ExecuteAsync(null);

    [RelayCommand]
    private async Task LoadProductAsync()
    {
        await ExecuteAsync(async () =>
        {
            var result = await _productApi.GetProductAsync(ProductId);
            if (result?.Success == true) Product = result.Data;
            else SetError(result?.Message ?? "Product not found.");
        });
    }

    [RelayCommand]
    private async Task AddToCartAsync()
    {
        if (Product == null) return;
        await ExecuteAsync(async () =>
        {
            var result = await _cartApi.AddToCartAsync(new AddToCartDto(Product.Id, Quantity));
            if (result?.Success == true)
                await Shell.Current.DisplayAlert("Added to Cart ✓", $"{Quantity}x {Product.Name} added!", "OK");
            else
                SetError(result?.Message ?? "Failed.");
        });
    }

    [RelayCommand]
    private async Task EditProductAsync()
    {
        if (Product == null) return;
        await Shell.Current.GoToAsync($"editproduct?id={Product.Id}");
    }

    [RelayCommand]
    private async Task DeleteProductAsync()
    {
        if (Product == null) return;
        var confirm = await Shell.Current.DisplayAlert(
            "Delete", $"Delete \"{Product.Name}\"?", "Delete", "Cancel");
        if (!confirm) return;

        await ExecuteAsync(async () =>
        {
            var result = await _productApi.DeleteProductAsync(Product.Id);
            if (result?.Success == true)
                await Shell.Current.GoToAsync("..");
            else
                SetError(result?.Message ?? "Failed to delete.");
        });
    }
}

// ─── Add Product ─────────────────────────────────────────────
public partial class AddProductViewModel : BaseViewModel
{
    private readonly IProductApiService _productApi;

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private string _priceText = string.Empty;
    [ObservableProperty] private string _quantityText = "0";
    [ObservableProperty] private string _category = string.Empty;
    [ObservableProperty] private string _imageUrl = string.Empty;

    public AddProductViewModel(IProductApiService productApi) => _productApi = productApi;

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Description) ||
            string.IsNullOrWhiteSpace(Category))
        {
            SetError("Please fill Name, Description and Category.");
            return;
        }

        if (!decimal.TryParse(PriceText, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var price) || price <= 0)
        {
            SetError("Enter a valid price greater than 0.");
            return;
        }

        if (!int.TryParse(QuantityText, out var qty) || qty < 0)
        {
            SetError("Enter a valid quantity (0 or more).");
            return;
        }

        await ExecuteAsync(async () =>
        {
            var dto = new CreateProductDto(Name, Description, price, qty, Category,
                         string.IsNullOrWhiteSpace(ImageUrl) ? null : ImageUrl);
            var result = await _productApi.CreateProductAsync(dto);
            if (result?.Success == true)
            {
                await Shell.Current.GoToAsync("..");
                WeakReferenceMessenger.Default.Send(new ProductsChangedMessage());
            }
            else
                SetError(result?.Message ?? "Failed to create product.");
        });
    }

    [RelayCommand]
    private async Task CancelAsync() => await Shell.Current.GoToAsync("..");
}

// ─── Edit Product ─────────────────────────────────────────────
[QueryProperty(nameof(ProductId), "id")]
public partial class EditProductViewModel : BaseViewModel
{
    private readonly IProductApiService _productApi;

    [ObservableProperty] private int _productId;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private string _priceText = string.Empty;
    [ObservableProperty] private string _quantityText = string.Empty;
    [ObservableProperty] private string _category = string.Empty;
    [ObservableProperty] private string _imageUrl = string.Empty;

    public EditProductViewModel(IProductApiService productApi) => _productApi = productApi;

    partial void OnProductIdChanged(int value) => LoadProductCommand.ExecuteAsync(null);

    [RelayCommand]
    private async Task LoadProductAsync()
    {
        await ExecuteAsync(async () =>
        {
            var result = await _productApi.GetProductAsync(ProductId);
            if (result?.Success == true && result.Data != null)
            {
                var p = result.Data;
                Name = p.Name;
                Description = p.Description;
                PriceText = p.Price.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
                QuantityText = p.Quantity.ToString();
                Category = p.Category;
                ImageUrl = p.ImageUrl ?? string.Empty;
            }
            else SetError("Product not found.");
        });
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Description) ||
            string.IsNullOrWhiteSpace(Category))
        {
            SetError("Please fill Name, Description and Category.");
            return;
        }

        if (!decimal.TryParse(PriceText, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var price) || price <= 0)
        {
            SetError("Enter a valid price.");
            return;
        }

        if (!int.TryParse(QuantityText, out var qty) || qty < 0)
        {
            SetError("Enter a valid quantity.");
            return;
        }

        await ExecuteAsync(async () =>
        {
            var dto = new UpdateProductDto(Name, Description, price, qty, Category,
                         string.IsNullOrWhiteSpace(ImageUrl) ? null : ImageUrl);
            var result = await _productApi.UpdateProductAsync(ProductId, dto);
            if (result?.Success == true)
            {
                await Shell.Current.GoToAsync("..");
                WeakReferenceMessenger.Default.Send(new ProductsChangedMessage());
            }
            else
                SetError(result?.Message ?? "Failed to update.");
        });
    }

    [RelayCommand]
    private async Task CancelAsync() => await Shell.Current.GoToAsync("..");
}

// ─── Cart ────────────────────────────────────────────────────
public partial class CartViewModel : BaseViewModel
{
    private readonly ICartApiService _cartApi;
    private readonly IOrderApiService _orderApi;

    [ObservableProperty] private ObservableCollection<CartItemDto> _items = new();
    [ObservableProperty] private decimal _total;
    [ObservableProperty] private bool _isEmpty;

    public CartViewModel(ICartApiService cartApi, IOrderApiService orderApi)
    {
        _cartApi = cartApi;
        _orderApi = orderApi;
    }

    [RelayCommand]
    public async Task LoadCartAsync()
    {
        await ExecuteAsync(async () =>
        {
            var result = await _cartApi.GetCartAsync();
            if (result?.Success == true && result.Data != null)
            {
                Items = new ObservableCollection<CartItemDto>(result.Data.Items);
                Total = result.Data.Total;
                IsEmpty = !Items.Any();
            }
        });
    }

    [RelayCommand]
    private async Task RemoveItemAsync(CartItemDto item)
    {
        await ExecuteAsync(async () =>
        {
            await _cartApi.RemoveFromCartAsync(item.Id);
            await LoadCartAsync();
        });
    }

    [RelayCommand]
    private async Task IncreaseQuantityAsync(CartItemDto item)
    {
        await ExecuteAsync(async () =>
        {
            await _cartApi.UpdateCartItemAsync(item.Id, new UpdateCartItemDto(item.Quantity + 1));
            await LoadCartAsync();
        });
    }

    [RelayCommand]
    private async Task DecreaseQuantityAsync(CartItemDto item)
    {
        if (item.Quantity <= 1) { await RemoveItemAsync(item); return; }
        await ExecuteAsync(async () =>
        {
            await _cartApi.UpdateCartItemAsync(item.Id, new UpdateCartItemDto(item.Quantity - 1));
            await LoadCartAsync();
        });
    }

    [RelayCommand]
    private async Task CheckoutAsync()
    {
        if (!Items.Any()) return;
        var confirm = await Shell.Current.DisplayAlert(
            "Checkout", $"Place order for {Total:C}?", "Yes", "Cancel");
        if (!confirm) return;

        await ExecuteAsync(async () =>
        {
            var result = await _orderApi.CheckoutAsync();
            if (result?.Success == true)
            {
                await Shell.Current.DisplayAlert("Order Placed! 🎉", "Your order has been placed.", "OK");
                await LoadCartAsync();
                await Shell.Current.GoToAsync("//orders");
            }
            else
                SetError(result?.Message ?? "Checkout failed.");
        });
    }
}

// ─── Orders ──────────────────────────────────────────────────
public partial class OrdersViewModel : BaseViewModel
{
    private readonly IOrderApiService _orderApi;

    [ObservableProperty] private ObservableCollection<OrderDto> _orders = new();
    [ObservableProperty] private bool _isEmpty;

    public OrdersViewModel(IOrderApiService orderApi) => _orderApi = orderApi;

    [RelayCommand]
    public async Task LoadOrdersAsync()
    {
        await ExecuteAsync(async () =>
        {
            var result = await _orderApi.GetOrdersAsync();
            if (result?.Success == true && result.Data != null)
            {
                Orders = new ObservableCollection<OrderDto>(result.Data);
                IsEmpty = !Orders.Any();
            }
        });
    }

    [RelayCommand]
    private async Task ViewOrderAsync(OrderDto order) =>
        await Shell.Current.GoToAsync($"orderdetail?id={order.Id}");
}

// ─── Order Detail ─────────────────────────────────────────────
[QueryProperty(nameof(OrderId), "id")]
public partial class OrderDetailViewModel : BaseViewModel
{
    private readonly IOrderApiService _orderApi;

    [ObservableProperty] private int _orderId;
    [ObservableProperty] private OrderDto? _order;

    public OrderDetailViewModel(IOrderApiService orderApi) => _orderApi = orderApi;

    partial void OnOrderIdChanged(int value) => LoadOrderCommand.ExecuteAsync(null);

    [RelayCommand]
    private async Task LoadOrderAsync()
    {
        await ExecuteAsync(async () =>
        {
            var result = await _orderApi.GetOrderAsync(OrderId);
            if (result?.Success == true) Order = result.Data;
            else SetError("Order not found.");
        });
    }
}

// ─── Profile ──────────────────────────────────────────────────
public partial class ProfileViewModel : BaseViewModel
{
    private readonly IAuthStateService _authState;
    private readonly ITokenStorageService _tokenStorage;
    private readonly IApiService _apiService;

    [ObservableProperty] private string _username = string.Empty;
    [ObservableProperty] private string _role = string.Empty;
    [ObservableProperty] private string _permissions = string.Empty;

    public ProfileViewModel(IAuthStateService authState, ITokenStorageService tokenStorage, IApiService apiService)
    {
        _authState = authState;
        _tokenStorage = tokenStorage;
        _apiService = apiService;
        LoadProfile();
    }

    private void LoadProfile()
    {
        Username = _authState.Username ?? "";
        Role = _authState.Role ?? "";
        Permissions = _authState.Permissions.Any()
            ? string.Join(", ", _authState.Permissions)
            : "None";
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        await _tokenStorage.RemoveTokenAsync();
        await _tokenStorage.RemoveUserDataAsync();

        _apiService.SetToken(null);

        _authState.ClearUser();
        if (Shell.Current is AppShell shell)
            shell.ApplyAuthState();
        await Shell.Current.GoToAsync("//login");
    }
}

// ─── Admin Dashboard ──────────────────────────────────────────
public partial class AdminDashboardViewModel : BaseViewModel
{
    [ObservableProperty] private bool _isAdmin;

    public AdminDashboardViewModel(IAuthStateService authState)
    {
        IsAdmin = authState.IsAdmin;
    }

    [RelayCommand]
    private async Task GoToUsersAsync() =>
        await Shell.Current.GoToAsync("users");

    [RelayCommand]
    private async Task GoToProductsAsync() =>
        await Shell.Current.GoToAsync("//products");

    [RelayCommand]
    private async Task GoToOrdersAsync() =>
        await Shell.Current.GoToAsync("//orders");

    [RelayCommand]
    private async Task GoToPermissionsAsync() =>
        await Shell.Current.GoToAsync("users");
}

// ─── Users (Admin) ────────────────────────────────────────────
public partial class UsersViewModel : BaseViewModel
{
    private readonly IUserApiService _userApi;

    [ObservableProperty] private ObservableCollection<UserDto> _users = new();
    [ObservableProperty] private ObservableCollection<RoleDto> _roles = new();
    [ObservableProperty] private ObservableCollection<PermissionDto> _permissions = new();

    public UsersViewModel(IUserApiService userApi) => _userApi = userApi;

    [RelayCommand]
    public async Task LoadUsersAsync()
    {
        await ExecuteAsync(async () =>
        {
            var u = await _userApi.GetUsersAsync();
            if (u?.Success == true && u.Data != null)
                Users = new ObservableCollection<UserDto>(u.Data);

            var r = await _userApi.GetRolesAsync();
            if (r?.Success == true && r.Data != null)
                Roles = new ObservableCollection<RoleDto>(r.Data);

            var p = await _userApi.GetPermissionsAsync();
            if (p?.Success == true && p.Data != null)
                Permissions = new ObservableCollection<PermissionDto>(p.Data);
        });
    }

    [RelayCommand]
    private async Task UpdateRoleAsync(UserDto user)
    {
        var roleNames = Roles.Select(r => r.Name).ToArray();
        var selected = await Shell.Current.DisplayActionSheet(
            $"Change role for {user.Username}", "Cancel", null, roleNames);
        if (selected == null || selected == "Cancel") return;

        var role = Roles.FirstOrDefault(r => r.Name == selected);
        if (role == null) return;

        await ExecuteAsync(async () =>
        {
            var result = await _userApi.UpdateRoleAsync(user.Id, new UpdateUserRoleDto(role.Id));
            if (result?.Success == true)
            {
                await Shell.Current.DisplayAlert("Updated ✓", $"{user.Username} is now {selected}.", "OK");
                await LoadUsersAsync();
            }
            else SetError(result?.Message ?? "Failed.");
        });
    }

    [RelayCommand]
    private async Task ManagePermissionsAsync(UserDto user)
    {
        var permNames = Permissions.Select(p => p.Name).ToArray();
        var selected = await Shell.Current.DisplayActionSheet(
            $"Grant permission to {user.Username}", "Cancel", null, permNames);
        if (selected == null || selected == "Cancel") return;

        var perm = Permissions.FirstOrDefault(p => p.Name == selected);
        if (perm == null) return;

        await ExecuteAsync(async () =>
        {
            var result = await _userApi.AssignPermissionsAsync(
                user.Id, new AssignPermissionsDto(new List<int> { perm.Id }));
            if (result?.Success == true)
                await Shell.Current.DisplayAlert("Done ✓", $"\"{selected}\" granted to {user.Username}.", "OK");
            else
                SetError(result?.Message ?? "Failed.");
        });
    }
}