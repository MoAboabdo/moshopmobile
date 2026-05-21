namespace ShopApp.Mobile.Views;

public partial class ProductDetailPage : ContentPage
{
    private readonly ViewModels.ProductDetailViewModel _vm;

    public ProductDetailPage(ViewModels.ProductDetailViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    private void OnIncreaseQuantity(object sender, EventArgs e)
    {
        if (_vm.Quantity < 99) _vm.Quantity++;
    }

    private void OnDecreaseQuantity(object sender, EventArgs e)
    {
        if (_vm.Quantity > 1) _vm.Quantity--;
    }
}