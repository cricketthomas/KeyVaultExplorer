﻿using Azure.ResourceManager.KeyVault;
using Azure.Security.KeyVault.Secrets;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using kvexplorer.shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

namespace avalonia.kvexplorer.ViewModels;

public partial class VaultPageViewModel : ViewModelBase
{
    private readonly AuthService _authService;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(FilterValuesCommand))]
    public string searchQuery;

    [ObservableProperty]
    public ObservableCollection<SecretProperties> secretList;

    IEnumerable<SecretProperties> _secretList { get; set; }


    [ObservableProperty]
    public ObservableCollection<SecretProperties> secretListFiltered;

    public VaultPageViewModel(AuthService authService, VaultService vaultService)
    {
        _authService = authService;
    }

    public VaultPageViewModel()
    {
        _authService = new AuthService();
        secretList = new ObservableCollection<SecretProperties>()   {
            new SecretProperties("Salesforce Password" ) { ContentType = "application/json", Enabled = true, ExpiresOn = new System.DateTime(),  },
            new SecretProperties("SysAdminPassword" ) { ContentType = "application/json", Enabled = true, ExpiresOn = new System.DateTime(),  },
            new SecretProperties("AzureAPIKey" ) { ContentType = "application/json", Enabled = true, ExpiresOn = new System.DateTime(),  },
            new SecretProperties("1__Key" ) { ContentType = "application/json", Enabled = true, ExpiresOn = new System.DateTime(),  },
            new SecretProperties("2__Key" ) { ContentType = "application/json", Enabled = true, ExpiresOn = new System.DateTime(),  },
            new SecretProperties("3__Key" ) { ContentType = "application/json", Enabled = true, ExpiresOn = new System.DateTime(),  },
            new SecretProperties("4__Key" ) { ContentType = "application/json", Enabled = true, ExpiresOn = new System.DateTime(),  },
            new SecretProperties("5__Key" ) { ContentType = "application/json", Enabled = true, ExpiresOn = new System.DateTime(),  },
            new SecretProperties("6__Key" ) { ContentType = "application/json", Enabled = true, ExpiresOn = new System.DateTime(),  },
            new SecretProperties("7__Key" ) { ContentType = "application/json", Enabled = true, ExpiresOn = new System.DateTime(),  },
            new SecretProperties("8__Key" ) { ContentType = "application/json", Enabled = true, ExpiresOn = new System.DateTime(),  },
            new SecretProperties("9__Key" ) { ContentType = "application/json", Enabled = true, ExpiresOn = new System.DateTime(),  },
            new SecretProperties("10__Key" ) { ContentType = "application/json", Enabled = true, ExpiresOn = new System.DateTime(),  },
            new SecretProperties("11__Key" ) { ContentType = "application/json", Enabled = true, ExpiresOn = new System.DateTime(),  },
            new SecretProperties("12__Key" ) { ContentType = "application/json", Enabled = true, ExpiresOn = new System.DateTime(),  },
            new SecretProperties("13__Key" ) { ContentType = "application/json", Enabled = true, ExpiresOn = new System.DateTime(),  },
            new SecretProperties("14__Key" ) { ContentType = "application/json", Enabled = true, ExpiresOn = new System.DateTime(),  },
            new SecretProperties("15__Key" ) { ContentType = "application/json", Enabled = true, ExpiresOn = new System.DateTime(),  },
            new SecretProperties("16__Key" ) { ContentType = "application/json", Enabled = true, ExpiresOn = new System.DateTime(),  },
            new SecretProperties("17__Key" ) { ContentType = "application/json", Enabled = true, ExpiresOn = new System.DateTime(),  },
            new SecretProperties("18__Key" ) { ContentType = "application/json", Enabled = true, ExpiresOn = new System.DateTime(),  },
            new SecretProperties("19__Key" ) { ContentType = "application/json", Enabled = true, ExpiresOn = new System.DateTime(),  },

            new SecretProperties("AmazonAlexAuthToken" ) { ContentType = "application/json", Enabled = true, ExpiresOn = new System.DateTime(),  },
        };
        _secretList = secretList.ToList();
    }

    [RelayCommand]
    private void FilterValues()
    {
        string query = SearchQuery.Trim().ToLowerInvariant();
        //if (!string.IsNullOrWhiteSpace(query))
        var list = SecretList.Where(v => v.Name.ToLowerInvariant().Contains(query));
        SecretList = new ObservableCollection<SecretProperties>(list);
    }

    partial void OnSearchQueryChanged(string searchQuery)
    {
        string query = searchQuery.Trim().ToLowerInvariant();
        if (!string.IsNullOrWhiteSpace(query))
        {
            SecretList = new ObservableCollection<SecretProperties>(_secretList);

        }
        var list = _secretList.Where(v => v.Name.ToLowerInvariant().Contains(query));
        SecretList = new ObservableCollection<SecretProperties>(list);
    }


}