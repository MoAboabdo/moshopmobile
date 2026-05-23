using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using ShopApp.Mobile.Services;
using ShopApp.Mobile.ViewModels;
using ShopApp.Mobile.Views;
using Microsoft.Extensions.DependencyInjection;

namespace ShopApp.Mobile;

public static class MauiProgram
{

    private const bool UseMockServices = true;

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        if(UseMockServices)
        {
            // Mock Services
            builder.Services.AddSingleton<IApiService, MockApiService>();
            builder.Services.AddSingleton<ITokenStorageService, MockTokenStorageService>();
            builder.Services.AddSingleton<IProductApiService, MockProductApiService>();
            builder.Services.AddSingleton<ICartApiService, MockCartApiService>();
            builder.Services.AddSingleton<IOrderApiService, MockOrderApiService>();
            builder.Services.AddSingleton<IUserApiService, MockUserApiService>();
            builder.Services.AddSingleton<IAuthStateService, MockAuthStateService>();
        } 
        else
        {
            // HTTP Client
            builder.Services.AddHttpClient<IApiService, ApiService>(client =>
            {
                client.BaseAddress = new Uri(ApiConstants.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            // Services
            builder.Services.AddSingleton<ITokenStorageService, TokenStorageService>();
            builder.Services.AddScoped<IProductApiService, ProductApiService>();
            builder.Services.AddScoped<ICartApiService, CartApiService>();
            builder.Services.AddScoped<IOrderApiService, OrderApiService>();
            builder.Services.AddScoped<IUserApiService, UserApiService>();
            builder.Services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);

        builder.Services.AddSingleton<IAuthStateService, AuthStateService>();
        }




        // ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<ProductListViewModel>();
        builder.Services.AddTransient<ProductDetailViewModel>();
        builder.Services.AddTransient<AddProductViewModel>();
        builder.Services.AddTransient<EditProductViewModel>();
        builder.Services.AddTransient<CartViewModel>();
        builder.Services.AddTransient<OrdersViewModel>();
        builder.Services.AddTransient<OrderDetailViewModel>();
        builder.Services.AddTransient<AdminDashboardViewModel>();
        builder.Services.AddTransient<UsersViewModel>();
        builder.Services.AddTransient<ProfileViewModel>();

        // Views (Pages)
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<ProductListPage>();
        builder.Services.AddTransient<ProductDetailPage>();
        builder.Services.AddTransient<AddProductPage>();
        builder.Services.AddTransient<EditProductPage>();
        builder.Services.AddTransient<CartPage>();
        builder.Services.AddTransient<OrdersPage>();
        builder.Services.AddTransient<OrderDetailPage>();
        builder.Services.AddTransient<AdminDashboardPage>();
        builder.Services.AddTransient<UsersPage>();
        builder.Services.AddTransient<ProfilePage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}