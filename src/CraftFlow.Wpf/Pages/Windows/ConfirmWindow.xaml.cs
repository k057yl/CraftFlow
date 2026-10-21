using System.Windows;
using CraftFlow.Wpf.Services;

namespace CraftFlow.Wpf.Windows;

public partial class ConfirmWindow : Window
{
    public ConfirmWindow(string title, string message)
    {
        InitializeComponent();
        TitleTextBlock.Text = title;
        MessageTextBlock.Text = message;
    }

    private void Confirm_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    public static bool Show(Window owner, string messageKeyOrRaw, string? titleKeyOrRaw = null)
    {
        var title = titleKeyOrRaw != null ? LocalizationService.Get(titleKeyOrRaw) : LocalizationService.Get("CONFIRM_ACTION_TITLE");
        var message = LocalizationService.Get(messageKeyOrRaw);

        var win = new ConfirmWindow(title, message) { Owner = owner };
        return win.ShowDialog() == true;
    }
}