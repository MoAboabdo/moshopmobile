using ShopApp.Mobile.ViewModels;
namespace ShopApp.Mobile.Views;

public partial class OrdersPage : ContentPage
{
    public OrdersPage(OrdersViewModel vm) { InitializeComponent(); BindingContext = vm; }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        ((OrdersViewModel)BindingContext).LoadOrdersCommand.ExecuteAsync(null);
    }
}