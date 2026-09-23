using System.Globalization;
using System.Windows;

namespace CraftFlow.Wpf.Pages;

public partial class WriteOffDialog : Window
{
    public decimal QuantityToWriteOff { get; private set; }
    public string Reason { get; private set; } = string.Empty;

    public WriteOffDialog(string itemName, string batchNumber, decimal currentQuantity)
    {
        InitializeComponent();

        LotTitleTextBlock.Text = $"{itemName} ({batchNumber})";
        CurrentQuantityTextBlock.Text = $"Доступный остаток: {currentQuantity:N2}";
        QuantityTextBox.Text = currentQuantity.ToString("F2", CultureInfo.InvariantCulture);
        QuantityTextBox.Focus();
        QuantityTextBox.SelectAll();
    }

    private void Confirm_Click(object sender, RoutedEventArgs e)
    {
        var rawText = QuantityTextBox.Text.Replace(',', '.');
        if (!decimal.TryParse(rawText, NumberStyles.Any, CultureInfo.InvariantCulture, out var qty) || qty <= 0)
        {
            MessageBox.Show("Введите корректное количество!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        QuantityToWriteOff = qty;
        Reason = ReasonTextBox.Text.Trim();
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}