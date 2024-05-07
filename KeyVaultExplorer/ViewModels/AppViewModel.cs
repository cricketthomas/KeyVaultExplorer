using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KeyVaultExplorer.Views;
using KeyVaultExplorer.Services;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace KeyVaultExplorer.ViewModels;

public partial class AppViewModel : ViewModelBase
{

    private readonly AuthService _authService;

    [ObservableProperty]
    public string email;


    [ObservableProperty]
    public bool isAuthenticated = false;
    public AppViewModel()
    {
        _authService = Defaults.Locator.GetRequiredService<AuthService>();

    }

    [RelayCommand]
    public void About()
    {
        var aboutWindow = new AboutPageWindow()
        {
            Title = "About Key Vault Explorer",
            Width = 380,
            Height = 200,
            SizeToContent = SizeToContent.Manual,
            WindowStartupLocation = WindowStartupLocation.Manual,
        };

        var top = Avalonia.Application.Current.GetTopLevel() as MainWindow;
        aboutWindow.ShowDialog(top);
    }

    public async Task RefreshTokenAndGetAccountInformation()
    {
        var cancellation = new CancellationToken();
        var account = await _authService.RefreshTokenAsync(cancellation);

        if (account is null)
            account = await _authService.LoginAsync(cancellation);
        //.ClaimsPrincipal.Identities.First().FindFirst("email").Value.ToLowerInvariant();
        var identity = account.ClaimsPrincipal.Identities.First();
        var email = identity.FindAll("email").First().Value ?? account.Account.Username;

        string[] name = identity.FindAll("name").First().Value.Split(" ");

        Email = email.ToLowerInvariant();

    }
}