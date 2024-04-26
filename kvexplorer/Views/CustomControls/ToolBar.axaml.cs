﻿using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Azure.ResourceManager.KeyVault;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using kvexplorer.shared.Models;
using kvexplorer.ViewModels;
using kvexplorer.Views.Pages;
using System.Linq;

namespace kvexplorer.Views.CustomControls;

public partial class ToolBar : UserControl
{
    public ToolBar()
    {
        InitializeComponent();
        DataContext = Defaults.Locator.GetRequiredService<ToolBarViewModel>();

    }


    private void SettingsButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Control control = (Control)sender!;
        control.RaiseEvent(new RoutedEventArgs(MainView.NavigateSettingsEvent));
    }

    private void SubscriptionsButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Control control = (Control)sender!;
        control.RaiseEvent(new RoutedEventArgs(MainView.NavigateSubscriptionsEvent));
    }

  

    private void IsPaneToggledButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Control control = (Control)sender!;
        control.RaiseEvent(new RoutedEventArgs(TabViewPage.PaneToggledRoutedEvent));
    }
}