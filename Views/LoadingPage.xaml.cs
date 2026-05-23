namespace ShopApp.Mobile.Views;

public partial class LoadingPage : ContentPage
{
    public LoadingPage()
    {
        BackgroundColor = Color.FromArgb("#6366F1");

        Content = new VerticalStackLayout
        {
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Spacing = 20,
            Children =
            {
                new Label
                {
                    Text = "🛍️",
                    FontSize = 72,
                    HorizontalOptions = LayoutOptions.Center
                },
                new Label
                {
                    Text = "Mohamed Shop",
                    FontSize = 28,
                    FontAttributes = FontAttributes.Bold,
                    HorizontalOptions = LayoutOptions.Center,
                    TextColor = Colors.White
                },
                new ActivityIndicator
                {
                    IsRunning = true,
                    Color = Colors.White,
                    WidthRequest = 50,
                    HeightRequest = 50
                }
            }
        };
    }
}