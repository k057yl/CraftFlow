using System.Windows;
using static CraftFlow.Wpf.Services.LocalizationService;

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
        var title = titleKeyOrRaw != null ? Get(titleKeyOrRaw) : Get("UI_CONFIRM_ACTION_TITLE");
        var message = Get(messageKeyOrRaw);

        var win = new ConfirmWindow(title, message) { Owner = owner };
        return win.ShowDialog() == true;
    }
}