﻿using avalonia.kvexplorer.ViewModels;
using avalonia.kvexplorer.Views.Pages;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Azure.ResourceManager.KeyVault;
using FluentAvalonia.UI.Controls;
using kvexplorer.shared;
using kvexplorer.shared.Models;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

namespace avalonia.kvexplorer.Views.CustomControls;

public partial class KeyVaultTreeList : UserControl
{
    private readonly TabViewPageViewModel _tabViewViewModel;

    public KeyVaultTreeList()
    {
        InitializeComponent();

        var model = new KeyVaultTreeListViewModel();
        DataContext = model;
        _tabViewViewModel = Defaults.Locator.GetRequiredService<TabViewPageViewModel>();
        // TODO: Figure out why this breaks NativeAOT, possibly due to DI using reflection? idk FIX:
        /* System.TypeInitializationException: A type initializer threw an exception. To determine which type, inspect the InnerException's StackTrace property.
        ---> System.MissingMethodException: No parameterless constructor defined for type 'System.Diagnostics.ActivitySource'.*/
        Dispatcher.UIThread.Post(async () =>
        {
            await model.GetAvailableKeyVaultsCommand.ExecuteAsync(null);
        }, DispatcherPriority.ApplicationIdle);
    }


    private void OnDoubleClicked(object sender, TappedEventArgs args)
    {
        var sx = (TreeView)sender;

        if (sx.SelectedItem is not null)
        {
            Dispatcher.UIThread.Post(() =>
            {
                var model = (KeyVaultResource)sx.SelectedItem;
                var tab = new TabViewItem
                {
                    Header = model.Data.Name,
                    IconSource = new SymbolIconSource { Symbol = Symbol.Document },
                    Content = new VaultPage()
                };

                _tabViewViewModel.AddDocumentCommand.Execute(null);
            }, DispatcherPriority.Background);
        }
    }

    private void OnTreeListSelectionChangedTest(object sender, SelectionChangedEventArgs e)
    {
        var s = (TreeView)sender;

        if (s.SelectedItem is not null)
        {
            var model = (KeyVaultModel)s.SelectedItem;
        }
    }
}