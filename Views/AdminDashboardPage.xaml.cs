using ShopApp.Mobile.ViewModels;

namespace ShopApp.Mobile.Views;

public partial class AdminDashboardPage : ContentPage
{
    public AdminDashboardPage(AdminDashboardViewModel vm) { InitializeComponent(); BindingContext = vm; }
}
