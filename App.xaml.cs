namespace ShopApp.Mobile;

public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;

    public App(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        Application.Current.UserAppTheme = AppTheme.Light;
        _serviceProvider = serviceProvider;
        MainPage = new AppShell(serviceProvider);
    }
}