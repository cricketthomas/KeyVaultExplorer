using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace KeyVaultExplorer.ViewModels;

public partial class AppViewModel : ViewModelBase
{
    [RelayCommand]
    public void About()
    {
        var aboutWindow = new Window()
        {
            Title = "About Key Vault Explorer",
            Width = 380,
            Height = 200,
            CanResize  = false,
            
            
        };
        aboutWindow.Show();
    }

    public AppViewModel()
    {
    }
}