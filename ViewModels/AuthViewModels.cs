using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShopApp.Mobile.DTOs;
using ShopApp.Mobile.Services;
using System.IdentityModel.Tokens.Jwt;

namespace ShopApp.Mobile.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly IUserApiService _userApi;
    private readonly ITokenStorageService _tokenStorage;
    private readonly IAuthStateService _authState;

    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _password = string.Empty;

    public LoginViewModel(IUserApiService userApi, ITokenStorageService tokenStorage, IAuthStateService authState)
    {
        _userApi = userApi;
        _tokenStorage = tokenStorage;
        _authState = authState;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            SetError("Please enter email and password.");
            return;
        }

        await ExecuteAsync(async () =>
        {
            var result = await _userApi.LoginAsync(new LoginDto(Email, Password));
            if (result?.Success == true && result.Data != null)
            {
                await _tokenStorage.SaveTokenAsync(result.Data.Token);
                var userId = GetUserIdFromToken(result.Data.Token);
                _authState.SetUser(result.Data, userId);
                
                if (Shell.Current is AppShell shell)
                    shell.ApplyAuthState();
                await Shell.Current.GoToAsync("//products");
            }
            else
            {
                SetError(result?.Message ?? "Login failed.");
            }
        }, "Login failed. Please try again.");
    }

    [RelayCommand]
    private async Task GoToRegisterAsync() => await Shell.Current.GoToAsync("//register");

    private static int GetUserIdFromToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        var claim = jwt.Claims.FirstOrDefault(c => c.Type == "nameid" || c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
        return int.TryParse(claim?.Value, out var id) ? id : 0;
    }
}

public partial class RegisterViewModel : BaseViewModel
{
    private readonly IUserApiService _userApi;
    private readonly ITokenStorageService _tokenStorage;
    private readonly IAuthStateService _authState;

    [ObservableProperty] private string _username = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string _confirmPassword = string.Empty;

    public RegisterViewModel(IUserApiService userApi, ITokenStorageService tokenStorage, IAuthStateService authState)
    {
        _userApi = userApi;
        _tokenStorage = tokenStorage;
        _authState = authState;
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            SetError("Please fill all fields.");
            return;
        }

        if (Password != ConfirmPassword)
        {
            SetError("Passwords do not match.");
            return;
        }

        await ExecuteAsync(async () =>
        {
            var result = await _userApi.RegisterAsync(new RegisterDto(Username, Email, Password));
            if (result?.Success == true && result.Data != null)
            {
                await _tokenStorage.SaveTokenAsync(result.Data.Token);
                var userId = GetUserIdFromToken(result.Data.Token);
                _authState.SetUser(result.Data, userId);

                if (Shell.Current is AppShell shell)
                    shell.ApplyAuthState();
                await Shell.Current.GoToAsync("//products");
            }
            else
            {
                SetError(result?.Message ?? "Registration failed.");
            }
        }, "Registration failed. Please try again.");
    }

    [RelayCommand]
    private async Task GoToLoginAsync() => await Shell.Current.GoToAsync("//login");

    private static int GetUserIdFromToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        var claim = jwt.Claims.FirstOrDefault(c => c.Type == "nameid" || c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
        return int.TryParse(claim?.Value, out var id) ? id : 0;
    }
}