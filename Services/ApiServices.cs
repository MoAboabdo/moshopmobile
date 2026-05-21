using ShopApp.Mobile.DTOs;

namespace ShopApp.Mobile.Services;

// Product API Service
public interface IProductApiService
{
    Task<ApiResponse<PagedResult<ProductDto>>?> GetProductsAsync(ProductQueryDto query);
    Task<ApiResponse<ProductDto>?> GetProductAsync(int id);
    Task<ApiResponse<IEnumerable<string>>?> GetCategoriesAsync();
    Task<ApiResponse<ProductDto>?> CreateProductAsync(CreateProductDto dto);
    Task<ApiResponse<ProductDto>?> UpdateProductAsync(int id, UpdateProductDto dto);
    Task<ApiResponse?> DeleteProductAsync(int id);
}

public class ProductApiService : IProductApiService
{
    private readonly IApiService _api;
    private readonly ITokenStorageService _tokenStorage;

    public ProductApiService(IApiService api, ITokenStorageService tokenStorage)
    {
        _api = api;
        _tokenStorage = tokenStorage;
    }

    private async Task EnsureTokenAsync()
    {
        var token = await _tokenStorage.GetTokenAsync();
        _api.SetToken(token);
    }

    public async Task<ApiResponse<PagedResult<ProductDto>>?> GetProductsAsync(ProductQueryDto query)
    {
        await EnsureTokenAsync();
        var qs = BuildQueryString(query);
        return await _api.GetAsync<ApiResponse<PagedResult<ProductDto>>>($"products?{qs}");
    }

    public async Task<ApiResponse<ProductDto>?> GetProductAsync(int id)
    {
        await EnsureTokenAsync();
        return await _api.GetAsync<ApiResponse<ProductDto>>($"products/{id}");
    }

    public async Task<ApiResponse<IEnumerable<string>>?> GetCategoriesAsync()
    {
        await EnsureTokenAsync();
        return await _api.GetAsync<ApiResponse<IEnumerable<string>>>("products/categories");
    }

    public async Task<ApiResponse<ProductDto>?> CreateProductAsync(CreateProductDto dto)
    {
        await EnsureTokenAsync();
        return await _api.PostAsync<ApiResponse<ProductDto>>("products", dto);
    }

    public async Task<ApiResponse<ProductDto>?> UpdateProductAsync(int id, UpdateProductDto dto)
    {
        await EnsureTokenAsync();
        return await _api.PutAsync<ApiResponse<ProductDto>>($"products/{id}", dto);
    }

    public async Task<ApiResponse?> DeleteProductAsync(int id)
    {
        await EnsureTokenAsync();
        await _api.DeleteAsync($"products/{id}");
        return new ApiResponse(true, "Deleted");
    }

    private static string BuildQueryString(ProductQueryDto q)
    {
        var parts = new List<string>();
        if (!string.IsNullOrEmpty(q.Search)) parts.Add($"search={Uri.EscapeDataString(q.Search)}");
        if (!string.IsNullOrEmpty(q.Category)) parts.Add($"category={Uri.EscapeDataString(q.Category)}");
        if (q.MinPrice.HasValue) parts.Add($"minPrice={q.MinPrice}");
        if (q.MaxPrice.HasValue) parts.Add($"maxPrice={q.MaxPrice}");
        parts.Add($"sortBy={q.SortBy}");
        parts.Add($"sortDescending={q.SortDescending}");
        parts.Add($"page={q.Page}");
        parts.Add($"pageSize={q.PageSize}");
        return string.Join("&", parts);
    }
}

// Cart API Service
public interface ICartApiService
{
    Task<ApiResponse<CartDto>?> GetCartAsync();
    Task<ApiResponse<CartDto>?> AddToCartAsync(AddToCartDto dto);
    Task<ApiResponse<CartDto>?> UpdateCartItemAsync(int itemId, UpdateCartItemDto dto);
    Task<ApiResponse?> RemoveFromCartAsync(int itemId);
    Task<ApiResponse?> ClearCartAsync();
}

public class CartApiService : ICartApiService
{
    private readonly IApiService _api;
    private readonly ITokenStorageService _tokenStorage;

    public CartApiService(IApiService api, ITokenStorageService tokenStorage)
    {
        _api = api;
        _tokenStorage = tokenStorage;
    }

    private async Task EnsureTokenAsync()
    {
        var token = await _tokenStorage.GetTokenAsync();
        _api.SetToken(token);
    }

    public async Task<ApiResponse<CartDto>?> GetCartAsync()
    {
        await EnsureTokenAsync();
        return await _api.GetAsync<ApiResponse<CartDto>>("cart");
    }

    public async Task<ApiResponse<CartDto>?> AddToCartAsync(AddToCartDto dto)
    {
        await EnsureTokenAsync();
        return await _api.PostAsync<ApiResponse<CartDto>>("cart", dto);
    }

    public async Task<ApiResponse<CartDto>?> UpdateCartItemAsync(int itemId, UpdateCartItemDto dto)
    {
        await EnsureTokenAsync();
        return await _api.PutAsync<ApiResponse<CartDto>>($"cart/{itemId}", dto);
    }

    public async Task<ApiResponse?> RemoveFromCartAsync(int itemId)
    {
        await EnsureTokenAsync();
        await _api.DeleteAsync($"cart/{itemId}");
        return new ApiResponse(true, "Removed");
    }

    public async Task<ApiResponse?> ClearCartAsync()
    {
        await EnsureTokenAsync();
        await _api.DeleteAsync("cart");
        return new ApiResponse(true, "Cleared");
    }
}

// Order API Service
public interface IOrderApiService
{
    Task<ApiResponse<IEnumerable<OrderDto>>?> GetOrdersAsync();
    Task<ApiResponse<OrderDto>?> GetOrderAsync(int id);
    Task<ApiResponse<OrderDto>?> CheckoutAsync();
}

public class OrderApiService : IOrderApiService
{
    private readonly IApiService _api;
    private readonly ITokenStorageService _tokenStorage;

    public OrderApiService(IApiService api, ITokenStorageService tokenStorage)
    {
        _api = api;
        _tokenStorage = tokenStorage;
    }

    private async Task EnsureTokenAsync()
    {
        var token = await _tokenStorage.GetTokenAsync();
        _api.SetToken(token);
    }

    public async Task<ApiResponse<IEnumerable<OrderDto>>?> GetOrdersAsync()
    {
        await EnsureTokenAsync();
        return await _api.GetAsync<ApiResponse<IEnumerable<OrderDto>>>("orders");
    }

    public async Task<ApiResponse<OrderDto>?> GetOrderAsync(int id)
    {
        await EnsureTokenAsync();
        return await _api.GetAsync<ApiResponse<OrderDto>>($"orders/{id}");
    }

    public async Task<ApiResponse<OrderDto>?> CheckoutAsync()
    {
        await EnsureTokenAsync();
        return await _api.PostAsync<ApiResponse<OrderDto>>("orders/checkout", new { });
    }
}

// User API Service (Admin)
public interface IUserApiService
{
    Task<ApiResponse<AuthResponseDto>?> RegisterAsync(RegisterDto dto);
    Task<ApiResponse<AuthResponseDto>?> LoginAsync(LoginDto dto);
    Task<ApiResponse<IEnumerable<UserDto>>?> GetUsersAsync();
    Task<ApiResponse<UserDto>?> GetUserAsync(int id);
    Task<ApiResponse?> UpdateRoleAsync(int userId, UpdateUserRoleDto dto);
    Task<ApiResponse?> AssignPermissionsAsync(int userId, AssignPermissionsDto dto);
    Task<ApiResponse<IEnumerable<RoleDto>>?> GetRolesAsync();
    Task<ApiResponse<IEnumerable<PermissionDto>>?> GetPermissionsAsync();
}

public class UserApiService : IUserApiService
{
    private readonly IApiService _api;
    private readonly ITokenStorageService _tokenStorage;

    public UserApiService(IApiService api, ITokenStorageService tokenStorage)
    {
        _api = api;
        _tokenStorage = tokenStorage;
    }

    private async Task EnsureTokenAsync()
    {
        var token = await _tokenStorage.GetTokenAsync();
        _api.SetToken(token);
    }

    public Task<ApiResponse<AuthResponseDto>?> RegisterAsync(RegisterDto dto) =>
        _api.PostAsync<ApiResponse<AuthResponseDto>>("auth/register", dto);

    public Task<ApiResponse<AuthResponseDto>?> LoginAsync(LoginDto dto) =>
        _api.PostAsync<ApiResponse<AuthResponseDto>>("auth/login", dto);

    public async Task<ApiResponse<IEnumerable<UserDto>>?> GetUsersAsync()
    {
        await EnsureTokenAsync();
        return await _api.GetAsync<ApiResponse<IEnumerable<UserDto>>>("users");
    }

    public async Task<ApiResponse<UserDto>?> GetUserAsync(int id)
    {
        await EnsureTokenAsync();
        return await _api.GetAsync<ApiResponse<UserDto>>($"users/{id}");
    }

    public async Task<ApiResponse?> UpdateRoleAsync(int userId, UpdateUserRoleDto dto)
    {
        await EnsureTokenAsync();
        return await _api.PutAsync<ApiResponse>($"users/{userId}/role", dto);
    }

    public async Task<ApiResponse?> AssignPermissionsAsync(int userId, AssignPermissionsDto dto)
    {
        await EnsureTokenAsync();
        return await _api.PutAsync<ApiResponse>($"users/{userId}/permissions", dto);
    }

    public async Task<ApiResponse<IEnumerable<RoleDto>>?> GetRolesAsync()
    {
        await EnsureTokenAsync();
        return await _api.GetAsync<ApiResponse<IEnumerable<RoleDto>>>("roles");
    }

    public async Task<ApiResponse<IEnumerable<PermissionDto>>?> GetPermissionsAsync()
    {
        await EnsureTokenAsync();
        return await _api.GetAsync<ApiResponse<IEnumerable<PermissionDto>>>("roles/permissions");
    }
}