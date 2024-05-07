using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KeyVaultExplorer.Views;

namespace KeyVaultExplorer.ViewModels;

public partial class AppViewModel : ViewModelBase
{

    public AppViewModel()
    {

    }
    [RelayCommand]
    public void About()
    {

    var aboutWindow = new Window()
        {
            Title = "About Key Vault Explorer",
            Width = 380,
            Height = 200,
            
        };

        var top = Avalonia.Application.Current.GetTopLevel() as MainWindow;
        aboutWindow.Show(top);
    }

}