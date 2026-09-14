using System.Windows;

namespace CraftFlow.Wpf.Windows;

public partial class ConfirmDeleteAccountWindow : Window
{
    private readonly string _expectedEmail;
    public string EnteredEmail { get; private set; } = string.Empty;

    public ConfirmDeleteAccountWindow(string expectedEmail)
    {
        InitializeComponent();
        _expectedEmail = expectedEmail;
        RequiredEmailTextBlock.Text = expectedEmail;
    }

    private void Confirm_Click(object sender, RoutedEventArgs e)
    {
        EnteredEmail = EmailInputTextBox.Text.Trim();

        if (!string.Equals(EnteredEmail, _expectedEmail, StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show("Введенный Email не совпадает с вашим текущим адресом!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}