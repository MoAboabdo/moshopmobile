using ShopApp.Mobile.Services;
using ShopApp.Mobile.Views;

namespace ShopApp.Mobile;

public partial class AppShell : Shell
{
    private readonly IAuthStateService _authState;

    public AppShell(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _authState = serviceProvider.GetRequiredService<IAuthStateService>();

        // Register all named routes
        Routing.RegisterRoute("productdetail", typeof(ProductDetailPage));
        Routing.RegisterRoute("orderdetail", typeof(OrderDetailPage));
        Routing.RegisterRoute("users", typeof(UsersPage));
        Routing.RegisterRoute("addproduct", typeof(AddProductPage));
        Routing.RegisterRoute("editproduct", typeof(EditProductPage));

        // Always start at login — ApplyAuthState() called after successful login
        //Dispatcher.Dispatch(async () => await GoToAsync("//login"));
        Dispatcher.Dispatch(async () =>
        {
            await _authState.InitializeAsync();

            if (_authState.IsLoggedIn)
            {
                ApplyAuthState();

                await GoToAsync("//products");
            }
            else
            {
                await GoToAsync("//login");
            }
        });
    }

    /// <summary>Show/hide Admin flyout item based on role. Call after login.</summary>
    public void ApplyAuthState()
    {
        AdminFlyoutItem.IsVisible = _authState.IsAdmin;
    }
}