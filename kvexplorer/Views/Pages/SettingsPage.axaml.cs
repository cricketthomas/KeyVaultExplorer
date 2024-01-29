﻿using Avalonia.Controls;
using Avalonia.Interactivity;
using kvexplorer.ViewModels;

namespace kvexplorer.Views.Pages;

public partial class SettingsPage : UserControl
{
    public SettingsPage()
    {
        InitializeComponent();
        DataContext = Defaults.Locator.GetRequiredService<SettingsPageViewModel>();
        var bgCheckbox = this.FindControl<CheckBox>("BackgroundTransparencyCheckbox");
        bgCheckbox.IsCheckedChanged += BackgroundTransparency_ChangedEvent;
    }

    private void BackgroundTransparency_ChangedEvent(object? sender, RoutedEventArgs e)
    {
        Control control = (Control)sender!;
        control.RaiseEvent(new RoutedEventArgs(MainWindow.TransparencyChangedEvent));
    }

    private void FetchUserInfoSettingsExpanderItem_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        (DataContext as SettingsPageViewModel).SignInOrRefreshTokenCommand.Execute(null);
    }


    private void NumericUpDown_Spinned(object? sender, Avalonia.Controls.SpinEventArgs e)
    {
        var x = (sender as NumericUpDown);
        (DataContext as SettingsPageViewModel).SetClearClipboardTimeoutCommand.Execute(null);

    }
}