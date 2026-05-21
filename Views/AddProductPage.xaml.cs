using ShopApp.Mobile.ViewModels;

namespace ShopApp.Mobile.Views;

public partial class AddProductPage : ContentPage
{
    public AddProductPage(AddProductViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}