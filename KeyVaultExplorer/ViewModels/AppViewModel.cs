using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace KeyVaultExplorer.ViewModels;

public partial class AppViewModel : ViewModelBase
{

    public AppViewModel()
    {

    }
    [RelayCommand]
    public void About()
    {
         var top = Avalonia.Application.Current.GetTopLevel();
        var aboutWindow = new Window()
        {
            Title = "About Key Vault Explorer",
            Width = 380,
            Height = 200,
            
        };
        var ownerWindow = this;
        aboutWindow.Show(ownerWindow);
    }

    public AppViewModel()
    {
    }
}