namespace ShopApp.Mobile.DTOs;

public record RegisterDto(string Username, string Email, string Password);
public record LoginDto(string Email, string Password);
public record AuthResponseDto(string Token, string Username, string Email, string Role, List<string> Permissions);

public record ProductDto(int Id, string Name, string Description, decimal Price, int Quantity, string Category, string? ImageUrl, DateTime CreatedAt);
public record CreateProductDto(string Name, string Description, decimal Price, int Quantity, string Category, string? ImageUrl);
public record UpdateProductDto(string Name, string Description, decimal Price, int Quantity, string Category, string? ImageUrl);

public record ProductQueryDto
{
    public string? Search { get; init; }
    public string? Category { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public string SortBy { get; init; } = "name";
    public bool SortDescending { get; init; } = false;
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public record CartItemDto(int Id, int ProductId, string ProductName, string? ProductImageUrl, decimal Price, int Quantity, decimal Subtotal);
public record CartDto(List<CartItemDto> Items, decimal Total);
public record AddToCartDto(int ProductId, int Quantity);
public record UpdateCartItemDto(int Quantity);

public record OrderDto(int Id, decimal TotalPrice, string Status, DateTime CreatedAt, List<OrderItemDto> Items);
public record OrderItemDto(int ProductId, string ProductName, int Quantity, decimal UnitPrice, decimal Subtotal);

public record UserDto(int Id, string Username, string Email, string Role, bool IsActive, DateTime CreatedAt, List<string>? Permissions = null);
public record RoleDto(int Id, string Name, List<string> Permissions);
public record PermissionDto(int Id, string Name, string Description);
public record UpdateUserRoleDto(int RoleId);
public record AssignPermissionsDto(List<int> PermissionIds);

public record PagedResult<T>(List<T> Items, int TotalCount, int Page, int PageSize, int TotalPages);
public record ApiResponse<T>(bool Success, string Message, T? Data);
public record ApiResponse(bool Success, string Message);