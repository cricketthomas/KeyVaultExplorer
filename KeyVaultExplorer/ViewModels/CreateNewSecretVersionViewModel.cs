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

public partial class CreateNewSecretVersionViewModel : ViewModelBase
{
    public CreateNewSecretVersionViewModel()
    {
    }
}