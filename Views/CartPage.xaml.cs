using ShopApp.Mobile.ViewModels;

namespace ShopApp.Mobile.Views;

public partial class CartPage : ContentPage
{
    public CartPage(CartViewModel vm) { InitializeComponent(); BindingContext = vm; }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        ((CartViewModel)BindingContext).LoadCartCommand.ExecuteAsync(null);
    }
}