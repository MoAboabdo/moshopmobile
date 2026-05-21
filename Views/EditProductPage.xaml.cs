using ShopApp.Mobile.ViewModels;

namespace ShopApp.Mobile.Views;

public partial class EditProductPage : ContentPage
{
    public EditProductPage(EditProductViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}