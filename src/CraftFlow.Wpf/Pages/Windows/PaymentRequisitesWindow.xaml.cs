using System.Windows;

namespace CraftFlow.Wpf.Windows;

public partial class PaymentRequisitesWindow : Window
{
    public PaymentRequisitesWindow(string planCode, string orgName)
    {
        InitializeComponent();
        PaymentCommentTextBox.Text = $"Оплата {planCode} ({orgName})";
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }
}