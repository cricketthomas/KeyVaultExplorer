using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KeyVaultExplorer.Views;
using KeyVaultExplorer.Services;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using Azure.Security.KeyVault.Secrets;
using System;

namespace KeyVaultExplorer.ViewModels;

public partial class CreateNewSecretVersionViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool isBusy = false;

    [ObservableProperty]
    private bool isEdit = false;


    public bool HasActivationDate => KeyVaultSecretModel is not null && KeyVaultSecretModel.NotBefore.HasValue;
    public bool HasExpirationDate => KeyVaultSecretModel is not null && KeyVaultSecretModel.ExpiresOn.HasValue;



    [ObservableProperty]
    private string secretValue;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Location))]
    [NotifyPropertyChangedFor(nameof(ExpiresOnTimespan))]
    [NotifyPropertyChangedFor(nameof(NotBeforeTimespan))]
    [NotifyPropertyChangedFor(nameof(HasActivationDate))]
    [NotifyPropertyChangedFor(nameof(HasExpirationDate))]
    private SecretProperties keyVaultSecretModel;

    public TimeSpan? ExpiresOnTimespan => KeyVaultSecretModel is not null && KeyVaultSecretModel.ExpiresOn.HasValue ? KeyVaultSecretModel?.ExpiresOn.Value.LocalDateTime.TimeOfDay : null;
    public TimeSpan? NotBeforeTimespan => KeyVaultSecretModel is not null && KeyVaultSecretModel.NotBefore.HasValue ? KeyVaultSecretModel?.NotBefore.Value.LocalDateTime.TimeOfDay : null;

    public string? Location => KeyVaultSecretModel?.VaultUri.ToString();
    public string? Identifier => KeyVaultSecretModel?.Id.ToString();

    private readonly AuthService _authService;
    private readonly VaultService _vaultService;
    private NotificationViewModel _notificationViewModel;

    public CreateNewSecretVersionViewModel()
    {
        _authService = Defaults.Locator.GetRequiredService<AuthService>();
        _vaultService = Defaults.Locator.GetRequiredService<VaultService>();
        _notificationViewModel = Defaults.Locator.GetRequiredService<NotificationViewModel>();
    }

    [RelayCommand]
    public async Task EditDetails()
    {
        var properties = KeyVaultSecretModel;

        if (properties.NotBefore.HasValue)
            properties.NotBefore = properties.NotBefore.Value.Date + (NotBeforeTimespan.HasValue ? NotBeforeTimespan.Value : TimeSpan.Zero);

        if (properties.ExpiresOn.HasValue)
            properties.ExpiresOn = properties.ExpiresOn.Value.Date + (ExpiresOnTimespan.HasValue ? ExpiresOnTimespan.Value : TimeSpan.Zero);


        //if (NotBeforeTimespan.HasValue)
        //    properties.NotBefore = new DateTimeOffset(properties.NotBefore.Value.Date, NotBeforeTimespan.HasValue ? NotBeforeTimespan.Value : new TimeSpan()); ;




        //if (ExpiresOnTimespan.HasValue)
        //    properties.ExpiresOn = new DateTimeOffset(properties.ExpiresOn.Value.Date, ExpiresOnTimespan.HasValue ? ExpiresOnTimespan.Value : new TimeSpan()); ;

        await _vaultService.UpdateSecret(KeyVaultSecretModel, KeyVaultSecretModel.VaultUri);
    }
}