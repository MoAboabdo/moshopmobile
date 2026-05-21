using ShopApp.Mobile.ViewModels;

namespace ShopApp.Mobile.Views;

public partial class OrderDetailPage : ContentPage
{
    public OrderDetailPage(OrderDetailViewModel vm) { InitializeComponent(); BindingContext = vm; }
}