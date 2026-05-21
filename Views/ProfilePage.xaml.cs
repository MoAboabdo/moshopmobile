using ShopApp.Mobile.ViewModels;

namespace ShopApp.Mobile.Views;

public partial class ProfilePage : ContentPage
{
    public ProfilePage(ProfileViewModel vm) { InitializeComponent(); BindingContext = vm; }
}