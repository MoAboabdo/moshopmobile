using CommunityToolkit.Mvvm.ComponentModel;

namespace ShopApp.Mobile.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool _isBusy;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    public bool IsNotBusy => !IsBusy;

    protected void SetError(string message)
    {
        ErrorMessage = message;
        HasError = !string.IsNullOrEmpty(message);
    }

    protected void ClearError()
    {
        ErrorMessage = string.Empty;
        HasError = false;
    }

    protected async Task ExecuteAsync(Func<Task> action, string? errorMessage = null)
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            ClearError();
            await action();
        }
        catch (Exception ex)
        {
            SetError(errorMessage ?? ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }
}