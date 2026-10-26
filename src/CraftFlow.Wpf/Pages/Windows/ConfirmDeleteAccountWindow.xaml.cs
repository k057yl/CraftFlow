using System.Windows;

namespace CraftFlow.Wpf.Windows;

public enum AccountDeleteAction
{
    None,
    Deactivate,
    HardDelete
}

public partial class ConfirmDeleteAccountWindow : Window
{
    private readonly string _expectedEmail;
    public string EnteredEmail { get; private set; } = string.Empty;
    public AccountDeleteAction ActionType { get; private set; } = AccountDeleteAction.None;

    public ConfirmDeleteAccountWindow(string expectedEmail)
    {
        InitializeComponent();
        _expectedEmail = expectedEmail;
        RequiredEmailTextBlock.Text = expectedEmail;
    }

    private bool ValidateEmail()
    {
        EnteredEmail = EmailInputTextBox.Text.Trim();

        if (!string.Equals(EnteredEmail, _expectedEmail, StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show("Введенный Email не совпадает с вашим текущим адресом!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        return true;
    }

    private void Deactivate_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidateEmail()) return;

        ActionType = AccountDeleteAction.Deactivate;
        DialogResult = true;
        Close();
    }

    private void Purge_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidateEmail()) return;

        ActionType = AccountDeleteAction.HardDelete;
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        ActionType = AccountDeleteAction.None;
        DialogResult = false;
        Close();
    }
}