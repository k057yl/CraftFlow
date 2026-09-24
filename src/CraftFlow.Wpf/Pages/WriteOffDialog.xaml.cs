using System.Globalization;
using System.Windows;

namespace CraftFlow.Wpf.Pages;

public partial class WriteOffDialog : Window
{
    private readonly decimal _maxQuantity;
    private readonly int _maxUnits;

    public decimal QuantityToWriteOff { get; private set; }
    public int UnitsToRemove { get; private set; }
    public string Reason { get; private set; } = string.Empty;

    public WriteOffDialog(string batchNumber, string chamberName, decimal currentQuantity, int currentUnits = 0)
    {
        InitializeComponent();

        _maxQuantity = currentQuantity;
        _maxUnits = currentUnits;

        LotTitleTextBlock.Text = $"{batchNumber} ({chamberName})";

        CurrentQuantityTextBlock.Text = _maxUnits > 0
            ? $"Доступно: {currentQuantity:N2} кг | {_maxUnits} шт."
            : $"Доступный остаток: {currentQuantity:N2} кг";

        if (_maxUnits <= 0)
        {
            UnitsStackPanel.Visibility = Visibility.Collapsed;
        }

        QuantityTextBox.Text = "0.00";
        UnitsTextBox.Text = "0";

        QuantityTextBox.Focus();
        QuantityTextBox.SelectAll();
    }

    private static bool TryParseDecimal(string text, out decimal result)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            result = 0m;
            return false;
        }

        var normalized = text.Trim().Replace(',', '.');
        return decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
    }

    private void Confirm_Click(object sender, RoutedEventArgs e)
    {
        if (!TryParseDecimal(QuantityTextBox.Text, out var qty) || qty < 0)
        {
            MessageBox.Show("Введите корректный вес для списания!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        int units = 0;
        if (_maxUnits > 0)
        {
            if (!int.TryParse(UnitsTextBox.Text.Trim(), out units) || units < 0)
            {
                MessageBox.Show("Введите корректное число штук (0 или больше)!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (units > _maxUnits)
            {
                MessageBox.Show($"Нельзя списать больше штук, чем есть в наличии ({_maxUnits} шт.)!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        if (qty <= 0 && units <= 0)
        {
            MessageBox.Show("Укажите списываемый вес или количество штук!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (qty > _maxQuantity)
        {
            MessageBox.Show($"Списываемый вес ({qty:N2} кг) превышает доступный остаток ({_maxQuantity:N2} кг)!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (qty == _maxQuantity && _maxUnits > 0 && units == 0)
        {
            units = _maxUnits;
        }

        QuantityToWriteOff = qty;
        UnitsToRemove = units;
        Reason = string.IsNullOrWhiteSpace(ReasonTextBox.Text) ? "Технические потери / Усушка" : ReasonTextBox.Text.Trim();

        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}