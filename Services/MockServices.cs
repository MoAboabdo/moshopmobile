using ShopApp.Mobile.DTOs;

namespace ShopApp.Mobile.Services;

// ════════════════════════════════════════════════════════════
//  STATIC DATA STORE  (shared between all mock services)
// ════════════════════════════════════════════════════════════
public static class MockDataStore
{
    // ── Tokens ──────────────────────────────────────────────
    // Simple fake JWTs (header.payload.signature — payload is base64 JSON)
    // nameid claim = user id
    public const string AdminToken =
        "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9." +
        "eyJuYW1laWQiOiIxIiwidW5pcXVlX25hbWUiOiJhZG1pbiIsInJvbGUiOiJBZG1pbiIsImV4cCI6OTk5OTk5OTk5OX0." +
        "mock_signature";

    public const string UserToken =
        "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9." +
        "eyJuYW1laWQiOiIyIiwidW5pcXVlX25hbWUiOiJ1c2VyIiwicm9sZSI6IlVzZXIiLCJleHAiOjk5OTk5OTk5OTl9." +
        "mock_signature";

    // ── Users ────────────────────────────────────────────────
    public static readonly List<(string Email, string Password, AuthResponseDto Auth, int Id)> Users =
    [
        (
            Email:    "admin@shop.com",
            Password: "Admin123!",
            Auth: new AuthResponseDto(
                Token:       AdminToken,
                Username:    "Admin",
                Email:       "admin@shop.com",
                Role:        "Admin",
                Permissions: ["CreateProduct","UpdateProduct","DeleteProduct","ManageUsers"]
            ),
            Id: 1
        ),
        (
            Email:    "user@shop.com",
            Password: "User123!",
            Auth: new AuthResponseDto(
                Token:       UserToken,
                Username:    "User",
                Email:       "user@shop.com",
                Role:        "User",
                Permissions: []
            ),
            Id: 2
        ),
    ];

    // ── Products ─────────────────────────────────────────────
    public static List<ProductDto> Products { get; } =
    [
        new(1,  "iPhone 15 Pro",        "أحدث هاتف من آبل",              4999m, 50,  "Electronics",  "https://picsum.photos/seed/iphone/300/300",   DateTime.Now.AddDays(-30)),
        new(2,  "Samsung Galaxy S24",   "هاتف سامسونج الرائد",           3899m, 30,  "Electronics",  "https://picsum.photos/seed/samsung/300/300",  DateTime.Now.AddDays(-28)),
        new(3,  "MacBook Air M3",       "لابتوب آبل الخفيف",             8999m, 20,  "Electronics",  "https://picsum.photos/seed/macbook/300/300",  DateTime.Now.AddDays(-25)),
        new(4,  "Sony WH-1000XM5",      "سماعات بخاصية إلغاء الضوضاء",  1299m, 100, "Electronics",  "https://picsum.photos/seed/sony/300/300",     DateTime.Now.AddDays(-20)),
        new(5,  "Nike Air Max 270",     "حذاء رياضي مريح",               799m,  200, "Shoes",        "https://picsum.photos/seed/nike/300/300",     DateTime.Now.AddDays(-18)),
        new(6,  "Adidas Ultraboost 23", "حذاء للجري",                    899m,  150, "Shoes",        "https://picsum.photos/seed/adidas/300/300",   DateTime.Now.AddDays(-15)),
        new(7,  "T-Shirt Premium",      "تيشيرت قطن 100%",               199m,  500, "Clothing",     "https://picsum.photos/seed/tshirt/300/300",  DateTime.Now.AddDays(-12)),
        new(8,  "Jeans Slim Fit",       "جينز سليم فيت",                 449m,  300, "Clothing",     "https://picsum.photos/seed/jeans/300/300",   DateTime.Now.AddDays(-10)),
        new(9,  "Coffee Maker Deluxe",  "ماكينة قهوة احترافية",          1599m, 40,  "Home",         "https://picsum.photos/seed/coffee/300/300",  DateTime.Now.AddDays(-8)),
        new(10, "Air Fryer XL",         "قلاية هوائية كبيرة",            699m,  60,  "Home",         "https://picsum.photos/seed/airfryer/300/300", DateTime.Now.AddDays(-5)),
    ];

    // ── Cart (per user) ──────────────────────────────────────
    private static readonly Dictionary<int, List<CartItemDto>> _carts = new()
    {
        [1] = [],
        [2] = [],
    };
    private static int _cartItemIdCounter = 1;

    public static List<CartItemDto> GetCart(int userId) =>
        _carts.TryGetValue(userId, out var cart) ? cart : (_carts[userId] = []);

    public static CartItemDto AddToCart(int userId, int productId, int quantity)
    {
        var product = Products.First(p => p.Id == productId);
        var cart = GetCart(userId);
        var existing = cart.FirstOrDefault(i => i.ProductId == productId);

        if (existing != null)
        {
            var updated = existing with
            {
                Quantity = existing.Quantity + quantity,
                Subtotal = product.Price * (existing.Quantity + quantity)
            };
            cart[cart.IndexOf(existing)] = updated;
            return updated;
        }

        var item = new CartItemDto(
            Id: _cartItemIdCounter++,
            ProductId: productId,
            ProductName: product.Name,
            ProductImageUrl: product.ImageUrl,
            Price: product.Price,
            Quantity: quantity,
            Subtotal: product.Price * quantity
        );
        cart.Add(item);
        return item;
    }

    public static void UpdateCartItem(int userId, int itemId, int quantity)
    {
        var cart = GetCart(userId);
        var idx = cart.FindIndex(i => i.Id == itemId);
        if (idx < 0) return;
        var item = cart[idx];
        cart[idx] = item with { Quantity = quantity, Subtotal = item.Price * quantity };
    }

    public static void RemoveFromCart(int userId, int itemId) =>
        GetCart(userId).RemoveAll(i => i.Id == itemId);

    public static void ClearCart(int userId) => GetCart(userId).Clear();

    // ── Orders ───────────────────────────────────────────────
    private static int _orderIdCounter = 3;
    public static List<OrderDto> Orders { get; } =
    [
        new(1, 5798m, "Delivered",  DateTime.Now.AddDays(-20),
            [ new(1, "iPhone 15 Pro",  1, 4999m, 4999m),
              new(4, "Sony WH-1000XM5", 1, 799m,  799m) ]),

        new(2, 449m, "Processing", DateTime.Now.AddDays(-3),
            [ new(8, "Jeans Slim Fit", 1, 449m, 449m) ]),
    ];

    public static OrderDto Checkout(int userId)
    {
        var cart = GetCart(userId);
        var items = cart.Select(i => new OrderItemDto(i.ProductId, i.ProductName, i.Quantity, i.Price, i.Subtotal)).ToList();
        var order = new OrderDto(_orderIdCounter++, cart.Sum(i => i.Subtotal), "Pending", DateTime.Now, items);
        Orders.Add(order);
        ClearCart(userId);
        return order;
    }

    // ── Roles & Permissions ──────────────────────────────────
    public static readonly List<RoleDto> Roles =
    [
        new(1, "Admin", ["CreateProduct","UpdateProduct","DeleteProduct","ManageUsers"]),
        new(2, "User",  []),
    ];

    public static readonly List<PermissionDto> Permissions =
    [
        new(1, "CreateProduct", "إنشاء منتج جديد"),
        new(2, "UpdateProduct", "تعديل منتج"),
        new(3, "DeleteProduct", "حذف منتج"),
        new(4, "ManageUsers",   "إدارة المستخدمين"),
    ];

    public static List<UserDto> AllUsers =
    [
        new(1, "Admin", "admin@shop.com", "Admin", true, DateTime.Now.AddDays(-60)),
        new(2, "User",  "user@shop.com",  "User",  true, DateTime.Now.AddDays(-30)),
    ];

    // ── User Permissions (منفصلة عن UserDto عشان نقدر نعدّلها) ──
    // Key = userId, Value = list of permission names
    public static Dictionary<int, List<string>> UserPermissions { get; } = new()
    {
        [1] = ["CreateProduct", "UpdateProduct", "DeleteProduct", "ManageUsers"],
        [2] = [],
    };

    public static List<string> GetUserPermissions(int userId) =>
        UserPermissions.TryGetValue(userId, out var perms) ? perms : (UserPermissions[userId] = []);

    public static void AddPermissionToUser(int userId, string permissionName)
    {
        var perms = GetUserPermissions(userId);
        if (!perms.Contains(permissionName))
            perms.Add(permissionName);
    }

    public static void SetUserRole(int userId, string roleName)
    {
        var idx = AllUsers.FindIndex(u => u.Id == userId);
        if (idx < 0) return;
        AllUsers[idx] = AllUsers[idx] with { Role = roleName };

        var role = Roles.FirstOrDefault(r => r.Name == roleName);
        if (role != null)
        {
            UserPermissions[userId] = [.. role.Permissions];
        }
    }

    // ── Product helpers ──────────────────────────────────────
    private static int _productIdCounter = 11;

    public static int NextProductId() => _productIdCounter++;
}


// ════════════════════════════════════════════════════════════
//  MOCK  —  IProductApiService
// ════════════════════════════════════════════════════════════
public class MockProductApiService : IProductApiService
{
    public Task<ApiResponse<PagedResult<ProductDto>>?> GetProductsAsync(ProductQueryDto q)
    {
        var all = MockDataStore.Products.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(q.Search))
            all = all.Where(p => p.Name.Contains(q.Search, StringComparison.OrdinalIgnoreCase) ||
                                 p.Description.Contains(q.Search, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(q.Category))
            all = all.Where(p => p.Category.Equals(q.Category, StringComparison.OrdinalIgnoreCase));

        if (q.MinPrice.HasValue) all = all.Where(p => p.Price >= q.MinPrice.Value);
        if (q.MaxPrice.HasValue) all = all.Where(p => p.Price <= q.MaxPrice.Value);

        all = q.SortBy?.ToLower() switch
        {
            "price" => q.SortDescending ? all.OrderByDescending(p => p.Price) : all.OrderBy(p => p.Price),
            "date" => q.SortDescending ? all.OrderByDescending(p => p.CreatedAt) : all.OrderBy(p => p.CreatedAt),
            _ => q.SortDescending ? all.OrderByDescending(p => p.Name) : all.OrderBy(p => p.Name),
        };

        var list = all.ToList();
        var totalCount = list.Count;
        var items = list.Skip((q.Page - 1) * q.PageSize).Take(q.PageSize).ToList();
        var totalPages = (int)Math.Ceiling(totalCount / (double)q.PageSize);

        var result = new PagedResult<ProductDto>(items, totalCount, q.Page, q.PageSize, Math.Max(1, totalPages));
        return Task.FromResult<ApiResponse<PagedResult<ProductDto>>?>(new(true, "OK", result));
    }

    public Task<ApiResponse<ProductDto>?> GetProductAsync(int id)
    {
        var p = MockDataStore.Products.FirstOrDefault(x => x.Id == id);
        return p is null
            ? Task.FromResult<ApiResponse<ProductDto>?>(new(false, "Product not found", null))
            : Task.FromResult<ApiResponse<ProductDto>?>(new(true, "OK", p));
    }

    public Task<ApiResponse<IEnumerable<string>>?> GetCategoriesAsync()
    {
        var cats = MockDataStore.Products.Select(p => p.Category).Distinct().OrderBy(c => c);
        return Task.FromResult<ApiResponse<IEnumerable<string>>?>(new(true, "OK", cats));
    }

    public Task<ApiResponse<ProductDto>?> CreateProductAsync(CreateProductDto dto)
    {
        var product = new ProductDto(
            MockDataStore.NextProductId(),
            dto.Name, dto.Description, dto.Price, dto.Quantity,
            dto.Category, dto.ImageUrl, DateTime.Now);
        MockDataStore.Products.Add(product);
        return Task.FromResult<ApiResponse<ProductDto>?>(new(true, "Product created", product));
    }

    public Task<ApiResponse<ProductDto>?> UpdateProductAsync(int id, UpdateProductDto dto)
    {
        var idx = MockDataStore.Products.FindIndex(p => p.Id == id);
        if (idx < 0)
            return Task.FromResult<ApiResponse<ProductDto>?>(new(false, "Product not found", null));

        var updated = MockDataStore.Products[idx] with
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Quantity = dto.Quantity,
            Category = dto.Category,
            ImageUrl = dto.ImageUrl,
        };
        MockDataStore.Products[idx] = updated;
        return Task.FromResult<ApiResponse<ProductDto>?>(new(true, "Product updated", updated));
    }

    public Task<ApiResponse?> DeleteProductAsync(int id)
    {
        MockDataStore.Products.RemoveAll(p => p.Id == id);
        return Task.FromResult<ApiResponse?>(new(true, "Product deleted"));
    }
}


// ════════════════════════════════════════════════════════════
//  MOCK  —  ICartApiService
// ════════════════════════════════════════════════════════════
public class MockCartApiService : ICartApiService
{
    // The mock doesn't have a real auth token, so we default to userId = 2 (User).
    // After login the ViewModels call the service directly, so we read from
    // a simple static field that MockUserApiService sets.
    private static CartDto BuildCart(int userId)
    {
        var items = MockDataStore.GetCart(userId);
        return new CartDto(items, items.Sum(i => i.Subtotal));
    }

    public Task<ApiResponse<CartDto>?> GetCartAsync()
    {
        var cart = BuildCart(MockSession.CurrentUserId);
        return Task.FromResult<ApiResponse<CartDto>?>(new(true, "OK", cart));
    }

    public Task<ApiResponse<CartDto>?> AddToCartAsync(AddToCartDto dto)
    {
        MockDataStore.AddToCart(MockSession.CurrentUserId, dto.ProductId, dto.Quantity);
        var cart = BuildCart(MockSession.CurrentUserId);
        return Task.FromResult<ApiResponse<CartDto>?>(new(true, "Added", cart));
    }

    public Task<ApiResponse<CartDto>?> UpdateCartItemAsync(int itemId, UpdateCartItemDto dto)
    {
        MockDataStore.UpdateCartItem(MockSession.CurrentUserId, itemId, dto.Quantity);
        var cart = BuildCart(MockSession.CurrentUserId);
        return Task.FromResult<ApiResponse<CartDto>?>(new(true, "Updated", cart));
    }

    public Task<ApiResponse?> RemoveFromCartAsync(int itemId)
    {
        MockDataStore.RemoveFromCart(MockSession.CurrentUserId, itemId);
        return Task.FromResult<ApiResponse?>(new(true, "Removed"));
    }

    public Task<ApiResponse?> ClearCartAsync()
    {
        MockDataStore.ClearCart(MockSession.CurrentUserId);
        return Task.FromResult<ApiResponse?>(new(true, "Cleared"));
    }
}


// ════════════════════════════════════════════════════════════
//  MOCK  —  IOrderApiService
// ════════════════════════════════════════════════════════════
public class MockOrderApiService : IOrderApiService
{
    public Task<ApiResponse<IEnumerable<OrderDto>>?> GetOrdersAsync()
    {
        return Task.FromResult<ApiResponse<IEnumerable<OrderDto>>?>(
            new(true, "OK", MockDataStore.Orders));
    }

    public Task<ApiResponse<OrderDto>?> GetOrderAsync(int id)
    {
        var order = MockDataStore.Orders.FirstOrDefault(o => o.Id == id);
        return order is null
            ? Task.FromResult<ApiResponse<OrderDto>?>(new(false, "Order not found", null))
            : Task.FromResult<ApiResponse<OrderDto>?>(new(true, "OK", order));
    }

    public Task<ApiResponse<OrderDto>?> CheckoutAsync()
    {
        var cart = MockDataStore.GetCart(MockSession.CurrentUserId);
        if (cart.Count == 0)
            return Task.FromResult<ApiResponse<OrderDto>?>(new(false, "Cart is empty", null));

        var order = MockDataStore.Checkout(MockSession.CurrentUserId);
        return Task.FromResult<ApiResponse<OrderDto>?>(new(true, "Order placed", order));
    }
}


// ════════════════════════════════════════════════════════════
//  MOCK  —  IUserApiService
// ════════════════════════════════════════════════════════════
public class MockUserApiService : IUserApiService
{
    public Task<ApiResponse<AuthResponseDto>?> LoginAsync(LoginDto dto)
    {
        var match = MockDataStore.Users.FirstOrDefault(u =>
            u.Email.Equals(dto.Email, StringComparison.OrdinalIgnoreCase) &&
            u.Password == dto.Password);

        if (match == default)
            return Task.FromResult<ApiResponse<AuthResponseDto>?>(
                new(false, "Invalid email or password", null));

        MockSession.CurrentUserId = match.Id;

        // جيب الـ role والـ permissions الحالية (ممكن تكون اتغيّرت بعد login قبل كده)
        var currentUser = MockDataStore.AllUsers.FirstOrDefault(u => u.Id == match.Id);
        var currentPerms = MockDataStore.GetUserPermissions(match.Id);

        var authResponse = match.Auth with
        {
            Role = currentUser?.Role ?? match.Auth.Role,
            Permissions = [.. currentPerms],
        };

        return Task.FromResult<ApiResponse<AuthResponseDto>?>(new(true, "Login successful", authResponse));
    }

    public Task<ApiResponse<AuthResponseDto>?> RegisterAsync(RegisterDto dto)
    {
        // For the demo, registration always succeeds and creates a regular user
        var newId = MockDataStore.AllUsers.Count + 1;
        var fakeUser = new AuthResponseDto(
            Token: MockDataStore.UserToken,
            Username: dto.Username,
            Email: dto.Email,
            Role: "User",
            Permissions: []
        );
        MockSession.CurrentUserId = newId;
        return Task.FromResult<ApiResponse<AuthResponseDto>?>(new(true, "Registered", fakeUser));
    }

    public Task<ApiResponse<IEnumerable<UserDto>>?> GetUsersAsync()
    {
        var users = MockDataStore.AllUsers.Select(u =>
            u with { Permissions = MockDataStore.GetUserPermissions(u.Id) }
        ).ToList();

        return Task.FromResult<ApiResponse<IEnumerable<UserDto>>?>(new(true, "OK", users));
    }

    public Task<ApiResponse<UserDto>?> GetUserAsync(int id)
    {
        var user = MockDataStore.AllUsers.FirstOrDefault(u => u.Id == id);
        if (user is null)
            return Task.FromResult<ApiResponse<UserDto>?>(new(false, "Not found", null));

        var userWithPerms = user with { Permissions = MockDataStore.GetUserPermissions(id) };
        return Task.FromResult<ApiResponse<UserDto>?>(new(true, "OK", userWithPerms));
    }

    public Task<ApiResponse?> UpdateRoleAsync(int userId, UpdateUserRoleDto dto)
    {
        var role = MockDataStore.Roles.FirstOrDefault(r => r.Id == dto.RoleId);
        if (role is null)
            return Task.FromResult<ApiResponse?>(new(false, "Role not found"));

        // بيحدّث الـ role والـ permissions مع بعض
        MockDataStore.SetUserRole(userId, role.Name);

        return Task.FromResult<ApiResponse?>(new(true, $"Role updated to {role.Name}"));
    }

    public Task<ApiResponse?> AssignPermissionsAsync(int userId, AssignPermissionsDto dto)
    {
        // جيب أسماء الـ permissions من الـ IDs
        var permNames = dto.PermissionIds
            .Select(id => MockDataStore.Permissions.FirstOrDefault(p => p.Id == id)?.Name)
            .Where(name => name != null)
            .ToList();

        foreach (var name in permNames)
            MockDataStore.AddPermissionToUser(userId, name!);

        return Task.FromResult<ApiResponse?>(new(true, "Permissions assigned"));
    }

    public Task<ApiResponse<IEnumerable<RoleDto>>?> GetRolesAsync() =>
        Task.FromResult<ApiResponse<IEnumerable<RoleDto>>?>(new(true, "OK", MockDataStore.Roles));

    public Task<ApiResponse<IEnumerable<PermissionDto>>?> GetPermissionsAsync() =>
        Task.FromResult<ApiResponse<IEnumerable<PermissionDto>>?>(new(true, "OK", MockDataStore.Permissions));
}


// ════════════════════════════════════════════════════════════
//  MOCK  —  IApiService  (no-op — not needed with mock services)
// ════════════════════════════════════════════════════════════
public class MockApiService : IApiService
{
    public Task<T?> GetAsync<T>(string endpoint) => Task.FromResult<T?>(default);
    public Task<T?> PostAsync<T>(string endpoint, object data) => Task.FromResult<T?>(default);
    public Task<T?> PutAsync<T>(string endpoint, object data) => Task.FromResult<T?>(default);
    public Task<bool> DeleteAsync(string endpoint) => Task.FromResult(true);
    public void SetToken(string? token) { }
}


// ════════════════════════════════════════════════════════════
//  MOCK  —  ITokenStorageService  (in-memory, no SecureStorage)
// ════════════════════════════════════════════════════════════
public class MockTokenStorageService : ITokenStorageService
{
    private const string TokenKey = "auth_token";

    public Task SaveTokenAsync(string token)
    {
        Preferences.Set(TokenKey, token);
        return Task.CompletedTask;
    }

    public Task<string?> GetTokenAsync()
    {
        var token = Preferences.Get(TokenKey, null);
        return Task.FromResult<string?>(token);
    }

    public Task RemoveTokenAsync()
    {
        Preferences.Remove(TokenKey);
        return Task.CompletedTask;
    }

    public Task RemoveUserDataAsync()
    {
        Preferences.Remove(TokenKey);
        return Task.CompletedTask;
    }
}

public class MockAuthStateService : IAuthStateService
{
    private readonly ITokenStorageService _tokenStorage;
    private const string UserKey = "mock_auth_user";

    public MockAuthStateService(ITokenStorageService tokenStorage)
    {
        _tokenStorage = tokenStorage;
    }

    public bool IsLoggedIn { get; private set; }
    public string? Username { get; private set; }
    public string? Role { get; private set; }
    public List<string> Permissions { get; private set; } = new();
    public int UserId { get; private set; }
    public bool IsAdmin => Role == "Admin";

    public void SetUser(AuthResponseDto authResponse, int userId)
    {
        IsLoggedIn = true;
        Username = authResponse.Username;
        Role = authResponse.Role;
        Permissions = authResponse.Permissions;
        UserId = userId;

        // احفظ في Preferences بدل SecureStorage
        var json = System.Text.Json.JsonSerializer.Serialize(authResponse);
        Preferences.Set(UserKey, json);
        Preferences.Set("mock_user_id", userId);
    }

    public void ClearUser()
    {
        IsLoggedIn = false;
        Username = null;
        Role = null;
        Permissions = new();
        UserId = 0;
        Preferences.Remove(UserKey);
        Preferences.Remove("mock_user_id");
    }

    public async Task InitializeAsync()
    {
        var json = Preferences.Get(UserKey, null);
        if (string.IsNullOrWhiteSpace(json)) return;

        var user = System.Text.Json.JsonSerializer.Deserialize<AuthResponseDto>(json);
        if (user == null) return;

        var token = await _tokenStorage.GetTokenAsync();
        if (string.IsNullOrWhiteSpace(token)) return;

        IsLoggedIn = true;
        Username = user.Username;
        Role = user.Role;
        Permissions = user.Permissions;
        UserId = Preferences.Get("mock_user_id", 0);

        // حدّث الـ MockSession
        MockSession.CurrentUserId = UserId;
    }

    public bool HasPermission(string permission) =>
        IsAdmin || Permissions.Contains(permission);
}


// ════════════════════════════════════════════════════════════
//  Helper — current logged-in user id (used by cart & orders)
// ════════════════════════════════════════════════════════════
public static class MockSession
{
    public static int CurrentUserId { get; set; } = 2; // default: regular user
}