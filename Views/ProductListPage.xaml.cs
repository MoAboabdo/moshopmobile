using ShopApp.Mobile.ViewModels;

namespace ShopApp.Mobile.Views;

public partial class ProductListPage : ContentPage
{
    public ProductListPage(ProductListViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        ((ProductListViewModel)BindingContext).LoadProductsCommand.ExecuteAsync(null);
    }
}