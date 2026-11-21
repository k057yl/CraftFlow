using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Constants.Endpoints;
using CraftFlow.SharedKernel.Dtos.Supplier;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.Wpf.Models.WarehouseAndSuppliers;
using CraftFlow.Wpf.Services;
using CraftFlow.Wpf.Windows;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CraftFlow.Wpf.Pages.WarehouseAndSuppliers.Controls;

public partial class SuppliersControl : UserControl
{
    private const string UI_SUPPLIERS_MANAGE_REQUIRED = "UI_ERR_PERMISSION_SUPPLIERS_MANAGE_REQUIRED";
    private const string UI_SUPPLIER_UPDATED_SUCCESS = "UI_MSG_SUPPLIER_UPDATED_SUCCESS";
    private const string UI_HEADER_SUPPLIER_EDIT = "UI_HEADER_SUPPLIER_EDIT";
    private const string UI_BTN_SAVE_CHANGES = "UI_BTN_SAVE_CHANGES";

    private Guid? _editingSupplierId = null;

    public bool IncludeInactive =>
        ShowInactiveSuppliersCheckBox.Visibility == Visibility.Visible &&
        (ShowInactiveSuppliersCheckBox.IsChecked ?? false);

    public event Action<string, Brush>? StatusUpdated;
    public event Func<Task>? SuppliersChanged;

    public SuppliersControl()
    {
        InitializeComponent();
    }

    public void BindSources(ObservableCollection<SupplierGridDto> suppliersList)
    {
        SuppliersDataGrid.ItemsSource = suppliersList;
    }

    public void ApplyPermissions(bool canManageSuppliers)
    {
        SupplierFormGroupBox.IsEnabled = canManageSuppliers;
    }

    private async void SaveSupplier_Click(object sender, RoutedEventArgs e)
    {
        if (!PermissionService.Instance.HasPermission(AppPermission.SUPPLIERS_MANAGE))
        {
            AlertWindow.ShowError(Window.GetWindow(this), LocalizationService.Get(UI_SUPPLIERS_MANAGE_REQUIRED));
            return;
        }

        var name = NameTextBox.Text.Trim();
        var legalName = string.IsNullOrWhiteSpace(LegalNameTextBox.Text) ? null : LegalNameTextBox.Text.Trim();
        var taxId = string.IsNullOrWhiteSpace(TaxIdTextBox.Text) ? null : TaxIdTextBox.Text.Trim();
        var phone = string.IsNullOrWhiteSpace(PhoneTextBox.Text) ? null : PhoneTextBox.Text.Trim();
        var email = string.IsNullOrWhiteSpace(EmailTextBox.Text) ? null : EmailTextBox.Text.Trim();
        var address = string.IsNullOrWhiteSpace(AddressTextBox.Text) ? null : AddressTextBox.Text.Trim();
        var notes = string.IsNullOrWhiteSpace(NotesTextBox.Text) ? null : NotesTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            var err = LocalizationService.Get(UiConstants.Messages.INVALID_INPUT_FIELDS);
            StatusUpdated?.Invoke(err, Brushes.Red);
            AlertWindow.ShowError(Window.GetWindow(this), err);
            return;
        }

        bool isSuccess;
        string responseStr;

        if (_editingSupplierId.HasValue)
        {
            var updateRequest = new UpdateSupplierRequest(name, legalName, taxId, phone, email, address, notes);
            (isSuccess, responseStr) = await ApiService.Instance.PutAndReadAsync($"{ProcurementConstants.SUPPLIERS}/{_editingSupplierId.Value}", updateRequest);
        }
        else
        {
            var createRequest = new CreateSupplierRequest(name, legalName, taxId, phone, email, address, notes);
            (isSuccess, responseStr) = await ApiService.Instance.PostAndReadAsync(ProcurementConstants.SUPPLIERS, createRequest);
        }

        if (isSuccess)
        {
            var okMsg = _editingSupplierId.HasValue
                ? LocalizationService.Get(UI_SUPPLIER_UPDATED_SUCCESS)
                : LocalizationService.Get(UiConstants.Messages.SUPPLIER_CREATED_SUCCESS);

            StatusUpdated?.Invoke(LocalizationService.Get(UiConstants.Messages.SUPPLIER_CREATED_SUCCESS), Brushes.Green);
            AlertWindow.ShowSuccess(Window.GetWindow(this), okMsg);

            ResetSupplierForm();
            if (SuppliersChanged != null)
                await SuppliersChanged.Invoke();
        }
        else
        {
            StatusUpdated?.Invoke(responseStr, Brushes.Red);
            AlertWindow.ShowError(Window.GetWindow(this), responseStr);
        }
    }

    private void EditSupplierRow_Click(object sender, RoutedEventArgs e)
    {
        if (!PermissionService.Instance.HasPermission(AppPermission.SUPPLIERS_MANAGE)) return;
        if (sender is Button btn && btn.Tag is SupplierGridDto supplier)
            PopulateFormForEdit(supplier);
    }

    private void SuppliersDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (!PermissionService.Instance.HasPermission(AppPermission.SUPPLIERS_MANAGE)) return;
        if (SuppliersDataGrid.SelectedItem is SupplierGridDto supplier)
            PopulateFormForEdit(supplier);
    }

    private void PopulateFormForEdit(SupplierGridDto supplier)
    {
        _editingSupplierId = supplier.Id;
        NameTextBox.Text = supplier.Name;
        LegalNameTextBox.Text = supplier.LegalName;
        TaxIdTextBox.Text = supplier.TaxId;
        PhoneTextBox.Text = supplier.Phone;
        EmailTextBox.Text = supplier.Email;
        AddressTextBox.Text = supplier.Address;
        NotesTextBox.Text = supplier.Notes;

        SupplierFormGroupBox.Header = LocalizationService.Get(UI_HEADER_SUPPLIER_EDIT);
        SaveSupplierButton.Content = LocalizationService.Get(UI_BTN_SAVE_CHANGES);
        ResetFormButton.Visibility = Visibility.Visible;
    }

    private void ResetSupplierForm_Click(object sender, RoutedEventArgs e) => ResetSupplierForm();

    private void ResetSupplierForm()
    {
        _editingSupplierId = null;
        NameTextBox.Clear();
        LegalNameTextBox.Clear();
        TaxIdTextBox.Clear();
        PhoneTextBox.Clear();
        EmailTextBox.Clear();
        AddressTextBox.Clear();
        NotesTextBox.Clear();

        SupplierFormGroupBox.Header = LocalizationService.Get("HEADER_SUPPLIERS");
        SaveSupplierButton.Content = LocalizationService.Get("CREATE_SUPPLIER");
        ResetFormButton.Visibility = Visibility.Collapsed;
    }

    private async void ToggleSupplierActive_Click(object sender, RoutedEventArgs e)
    {
        if (!PermissionService.Instance.HasPermission(AppPermission.SUPPLIERS_MANAGE)) return;

        if (sender is Button btn && btn.Tag is SupplierGridDto supplier)
        {
            var (isSuccess, responseStr) = await ApiService.Instance.PutAndReadAsync($"{ProcurementConstants.SUPPLIERS}/{supplier.Id}/toggle-active", new { });
            if (isSuccess)
            {
                if (SuppliersChanged != null)
                    await SuppliersChanged.Invoke();
            }
            else
            {
                AlertWindow.ShowError(Window.GetWindow(this), responseStr);
            }
        }
    }

    private async void ShowInactiveSuppliers_Click(object sender, RoutedEventArgs e)
    {
        if (SuppliersChanged != null)
            await SuppliersChanged.Invoke();
    }
}