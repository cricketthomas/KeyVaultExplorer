using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using KeyVaultExplorer.ViewModels;
namespace KeyVaultExplorer;

public partial class CreateNewSecretVersion : UserControl
{
    public CreateNewSecretVersion()
    {
        InitializeComponent();
        DataContext = new CreateNewSecretVersionViewModel();
    }

    private void InputField_OnAttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e)
    {
        // We will set the focus into our input field just after it got attached to the visual tree.
        if (sender is InputElement inputElement)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                inputElement.Focus(NavigationMethod.Unspecified, KeyModifiers.None);
            });
        }
    }
}