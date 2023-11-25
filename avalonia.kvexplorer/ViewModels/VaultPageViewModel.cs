﻿using avalonia.kvexplorer.Views;
using avalonia.kvexplorer.Views.Pages;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Input.TextInput;
using Avalonia.Interactivity;
using Avalonia.Remote.Protocol.Input;
using Avalonia.Threading;
using Azure.ResourceManager.KeyVault;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Secrets;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using kvexplorer.shared;
using kvexplorer.shared.Exceptions;
using kvexplorer.shared.Models;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace avalonia.kvexplorer.ViewModels;

public partial class VaultPageViewModel : ViewModelBase
{
    [ObservableProperty]
    public bool isBusy = false;

    [ObservableProperty]
    public string searchQuery;

    [ObservableProperty]
    public TabStripItem selectedTab;

    [ObservableProperty]
    public Uri vaultUri;

    public Dictionary<KeyVaultItemType, bool> LoadedItemTypes { get; set; } = new() { };

    [ObservableProperty]
    public ObservableCollection<KeyVaultContentsAmalgamation> vaultContents;

    private readonly VaultService _vaultService;
    private readonly AuthService _authService;


    public VaultPageViewModel()
    {
        _vaultService = Defaults.Locator.GetRequiredService<VaultService>();
        _authService = Defaults.Locator.GetRequiredService<AuthService>();

        vaultContents = new ObservableCollection<KeyVaultContentsAmalgamation>() { };
        for (int i = 0; i < 50; i++)
        {
            var sp = (new SecretProperties($"{i}_Demo__Key_Token") { ContentType = "application/json", Enabled = true, ExpiresOn = new System.DateTime(), });

            switch (i % 3)
            {
                case 0:
                    VaultContents.Add(new KeyVaultContentsAmalgamation
                    {
                        Name = $"{i}_Secret",
                        Id = new Uri("https://stackoverflow.com/"),
                        Type = KeyVaultItemType.Secret,
                        ContentType = "application/json",
                        VaultUri = new Uri("https://stackoverflow.com/"),
                        Version = "version 1",
                        SecretProperties = sp,
                    });
                    break;

                case 1:
                    VaultContents.Add(new KeyVaultContentsAmalgamation
                    {
                        Name = $"{i}__Key",
                        Id = new Uri("https://stackoverflow.com/"),
                        Type = KeyVaultItemType.Key,
                        ContentType = "application/json",
                        VaultUri = new Uri("https://stackoverflow.com/"),
                        Version = "version 1",
                        SecretProperties = sp,
                    });
                    break;

                case 2:
                    VaultContents.Add(new KeyVaultContentsAmalgamation
                    {
                        Name = $"{i}_Certificate",
                        Id = new Uri("https://stackoverflow.com/"),
                        Type = KeyVaultItemType.Certificate,
                        ContentType = "application/json",
                        VaultUri = new Uri("https://stackoverflow.com/"),
                        Version = "version 1",
                        SecretProperties = sp,
                    });
                    break;
            }
            _vaultContents = VaultContents;
        }
    }

    private IEnumerable<KeyVaultContentsAmalgamation> _vaultContents { get; set; }


    private async Task DelaySetIsBusy(bool val)
    {
        await Task.Delay(1000);
        IsBusy = val;
    }


    public async Task FilterAndLoadVaultValueType(KeyVaultItemType item)
    {
        if (!LoadedItemTypes.ContainsKey(item))
        {
            IsBusy = true;
            switch (item)
            {
                case KeyVaultItemType.Certificate:
                    await GetCertificatesForVault(VaultUri);
                    break;

                case KeyVaultItemType.Key:
                    await GetKeysForVault(VaultUri);
                    break;

                case KeyVaultItemType.Secret:
                    await GetSecretsForVault(VaultUri);
                    break;

                case KeyVaultItemType.All:
                    Task[] tasks = [];
                    VaultContents.Clear();
                    await Task.WhenAll(GetSecretsForVault(VaultUri), GetKeysForVault(VaultUri), GetCertificatesForVault(VaultUri));
                    LoadedItemTypes.TryAdd(KeyVaultItemType.Secret, true);
                    LoadedItemTypes.TryAdd(KeyVaultItemType.Key, true);
                    LoadedItemTypes.TryAdd(KeyVaultItemType.Certificate, true);
                    break;

                default:
                    break;
            }
            LoadedItemTypes.TryAdd(item, true);
        }
        if (item == KeyVaultItemType.All)
            VaultContents = new ObservableCollection<KeyVaultContentsAmalgamation>(_vaultContents.Where(v => v.Name.ToLowerInvariant().Contains(SearchQuery ?? "")));
        else
            VaultContents = new ObservableCollection<KeyVaultContentsAmalgamation>(_vaultContents.Where(v => item == v.Type && v.Name.ToLowerInvariant().Contains(SearchQuery ?? "")));

        await DelaySetIsBusy(false);

    }

    public async Task GetKeysForVault(Uri kvUri)
    {
        var keys = _vaultService.GetVaultAssociatedKeys(kvUri);
        await foreach (var key in keys)
        {
            VaultContents.Add(new KeyVaultContentsAmalgamation
            {
                Name = key.Name,
                Id = key.Id,
                Type = KeyVaultItemType.Key,
                VaultUri = key.VaultUri,
                ValueUri = key.Id,
                Version = key.Version,
                KeyProperties = key,
                LastModifiedDate = key.UpdatedOn.HasValue ? key.UpdatedOn.Value.ToLocalTime() : key.CreatedOn.Value.ToLocalTime()
            });
        }
        _vaultContents = VaultContents;
    }

    public async Task GetSecretsForVault(Uri kvUri)
    {
        var values = _vaultService.GetVaultAssociatedSecrets(kvUri);
        await foreach (var secret in values)
        {
            VaultContents.Add(new KeyVaultContentsAmalgamation
            {
                Name = secret.Name,
                Id = secret.Id,
                Type = KeyVaultItemType.Secret,
                ContentType = secret.ContentType,
                VaultUri = secret.VaultUri,
                ValueUri = secret.Id,
                Version = secret.Version,
                SecretProperties = secret,
                LastModifiedDate = secret.UpdatedOn.HasValue ? secret.UpdatedOn.Value.ToLocalTime() : secret.CreatedOn.Value.ToLocalTime()
            }); ;
        }

        _vaultContents = VaultContents;
    }

    public async Task GetCertificatesForVault(Uri kvUri)
    {
        var certs = _vaultService.GetVaultAssociatedCertificates(kvUri);
        await foreach (var cert in certs)
        {
            VaultContents.Add(new KeyVaultContentsAmalgamation
            {
                Name = cert.Name,
                Id = cert.Id,
                Type = KeyVaultItemType.Certificate,
                VaultUri = cert.VaultUri,
                ValueUri = cert.Id,
                Version = cert.Version,
                CertificateProperties = cert,
                LastModifiedDate = cert.UpdatedOn.HasValue ? cert.UpdatedOn.Value.ToLocalTime() : cert.CreatedOn.Value.ToLocalTime()
            });
        }
        _vaultContents = VaultContents;
    }

    partial void OnSearchQueryChanged(string value)
    {
        var isValidEnum = Enum.TryParse(SelectedTab?.Name.ToString(), true, out KeyVaultItemType parsedEnumValue) && Enum.IsDefined(typeof(KeyVaultItemType), parsedEnumValue);
        var item = isValidEnum ? parsedEnumValue : KeyVaultItemType.Secret;
        string query = value?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(query))
        {
            var contents = _vaultContents;
            if (item != KeyVaultItemType.All)
            {
                contents = contents.Where(k => k.Type == item);
            }
            VaultContents = new ObservableCollection<KeyVaultContentsAmalgamation>(contents);
            return;
        }
        //var toFilter = CheckedBoxes.Where(v => v.Value == true).Select(s => s.Key).ToList();
        //       && toFilter.Contains(v.Type)

        var list = _vaultContents.Where(v => v.Name.ToLowerInvariant().Contains(query));
        if (item != KeyVaultItemType.All)
            list = list.Where(k => k.Type == item);
        VaultContents = new ObservableCollection<KeyVaultContentsAmalgamation>(list);
    }

    private readonly INotificationManager _notificationManager;

    [RelayCommand]
    private async Task Refresh()
    {
        var isValidEnum = Enum.TryParse(SelectedTab?.Name.ToString(), true, out KeyVaultItemType parsedEnumValue) && Enum.IsDefined(typeof(KeyVaultItemType), parsedEnumValue);
        var item = isValidEnum ? parsedEnumValue : KeyVaultItemType.Secret;
        LoadedItemTypes.Remove(item);
        VaultContents = new ObservableCollection<KeyVaultContentsAmalgamation>(_vaultContents.Where(v => v.Type != item));
        await FilterAndLoadVaultValueType(item);
    }



    [RelayCommand]
    private async Task Copy(KeyVaultContentsAmalgamation keyVaultItem)
    {
        if (keyVaultItem is null) return;

        var topLevel = (Avalonia.Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).MainWindow;
        var clipboard = TopLevel.GetTopLevel(topLevel)?.Clipboard;
        Notification notif;
        try
        {
            string value = string.Empty;
            //_ = keyVaultItem.Type switch
            //{
            //    KeyVaultItemType.Key => value = (await _vaultService.GetKey(keyVaultItem.VaultUri, keyVaultItem.Name)).Key.ToRSA().ToXmlString(true),
            //    KeyVaultItemType.Secret => value = (await _vaultService.GetSecret(keyVaultItem.VaultUri, keyVaultItem.Name)).Value,
            //    KeyVaultItemType.Certificate => value = (await _vaultService.GetCertificate(keyVaultItem.VaultUri, keyVaultItem.Name)).Name,
            //    _ => throw new NotImplementedException()
            //};

            if (keyVaultItem.Type == KeyVaultItemType.Key)
            {
                var key = await _vaultService.GetKey(keyVaultItem.VaultUri, keyVaultItem.Name);
                if (key.KeyType == KeyType.Rsa)
                {
                    using var rsa = key.Key.ToRSA();
                    var publicKey = rsa.ExportRSAPublicKey();
                    string pem = "-----BEGIN PUBLIC KEY-----\n" + Convert.ToBase64String(publicKey) + "\n-----END PUBLIC KEY-----";
                    value = pem;
                }
            }

            if (keyVaultItem.Type == KeyVaultItemType.Secret)
            {
                var sv = await _vaultService.GetSecret(keyVaultItem.VaultUri, keyVaultItem.Name);
                value = sv.Value;
            }
            if (keyVaultItem.Type == KeyVaultItemType.Certificate)
            {
                var certValue = await _vaultService.GetCertificate(keyVaultItem.VaultUri, keyVaultItem.Name);
            }

            var dataObject = new DataObject();
            dataObject.Set(DataFormats.Text, value);
            await clipboard.SetTextAsync(value);
            // TODO: figure out why set data object async fails here.
            notif = new Notification("Copied", $"The value of '{keyVaultItem.Name}' has been copied to the clipboard.", NotificationType.Success);
        }
        catch (KeyVaultItemNotFoundException ex)
        {
            notif = new Notification($"A value was not found for '{keyVaultItem.Name}'", $"The value of was not able to be retrieved.\n {ex.Message}", NotificationType.Error);
        }

        var nm = new WindowNotificationManager(topLevel)
        {
            Position = NotificationPosition.BottomRight,
            MaxItems = 1,
        };
        nm.TemplateApplied += (sender, args) =>
        {
            nm.Show(notif);
        };
    }

    [RelayCommand]
    private async Task OpenInAzure(KeyVaultContentsAmalgamation keyVaultItem)
    {
        if (keyVaultItem is null) return;
        var tenantName = _authService.Account.Username.Split("@").TakeLast(1).Single();
        var uri = $"https://portal.azure.com/#@{tenantName}/asset/Microsoft_Azure_KeyVault/{keyVaultItem.Type}/{keyVaultItem.Id}";
        Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true, Verb = "open" });
    }

    [RelayCommand]
    private async Task CopyUri(KeyVaultContentsAmalgamation keyVaultItem)
    {
        if (keyVaultItem is null) return;
        var topLevel = (Avalonia.Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).MainWindow;
        var clipboard = TopLevel.GetTopLevel(topLevel)?.Clipboard;
        await clipboard.SetTextAsync(keyVaultItem.Id.ToString());
    }

}