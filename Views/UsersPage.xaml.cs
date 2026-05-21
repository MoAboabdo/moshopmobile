using ShopApp.Mobile.ViewModels;

namespace ShopApp.Mobile.Views;

public partial class UsersPage : ContentPage
{
    public UsersPage(UsersViewModel vm) { InitializeComponent(); BindingContext = vm; }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        ((UsersViewModel)BindingContext).LoadUsersCommand.ExecuteAsync(null);
    }
}