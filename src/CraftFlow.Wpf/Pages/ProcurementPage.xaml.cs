using System.Windows;
using System.Windows.Controls;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.Wpf.Models;
using CraftFlow.Wpf.Services;

namespace CraftFlow.Wpf.Pages;

public partial class ProcurementPage : Page
{
    public ProcurementPage()
    {
        InitializeComponent();
    }

    private async void CreateSupplier_Click(object sender, RoutedEventArgs e)
    {
        var name = NameTextBox.Text.Trim();
        var phone = PhoneTextBox.Text.Trim();
        var email = EmailTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show(UiConstants.Messages.INVALID_INPUT_FIELDS, ErrorCodes.General.VALUE_REQUIRED, MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var request = new CreateSupplierRequest(name, phone, email);
        var (isSuccess, responseStr) = await ApiService.Instance.CreateSupplierAsync(request);

        if (isSuccess)
        {
            MessageBox.Show(UiConstants.Messages.SUPPLIER_CREATED_SUCCESS, UiConstants.Messages.DATA_LOADED_SUCCESS, MessageBoxButton.OK, MessageBoxImage.Information);
            NameTextBox.Clear();
            PhoneTextBox.Clear();
            EmailTextBox.Clear();
        }
        else
        {
            MessageBox.Show($"{UiConstants.Messages.API_ERROR_PREFIX}: {responseStr}", ErrorCodes.General.NOT_FOUND, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}