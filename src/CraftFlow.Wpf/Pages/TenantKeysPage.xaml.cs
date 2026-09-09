using CraftFlow.SharedKernel.Constants;
using CraftFlow.Wpf.Models.Auth;
using CraftFlow.Wpf.Services;
using System.Windows;
using System.Windows.Navigation;

namespace CraftFlow.Wpf.Pages;

public partial class TenantKeysPage : PageFunction<string>
{
    private const int OTP_CODE_LENGTH = 6;

    public TenantKeysPage()
    {
        InitializeComponent();
        _ = LoadMyKeysAsync();
    }

    private async void ActivateOtp_Click(object sender, RoutedEventArgs e)
    {
        var otpCode = OtpInputTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(otpCode) || otpCode.Length != OTP_CODE_LENGTH)
        {
            MessageBox.Show(LocalizationService.Get(UiConstants.Messages.INVALID_OTP_FORMAT), LocalizationService.Get(UiConstants.Titles.WARNING), MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        (bool isSuccess, string contentOrError) = await ApiService.Instance.PostAndReadAsync(Endpoints.ACTIVATE_KEY_ENDPOINT, new
        {
            OtpCode = otpCode
        });

        if (isSuccess)
        {
            MessageBox.Show(LocalizationService.Get(UiConstants.Messages.KEY_ACTIVATED_SUCCESS), LocalizationService.Get(UiConstants.Titles.SUCCESS), MessageBoxButton.OK, MessageBoxImage.Information);
            OtpInputTextBox.Clear();
            await LoadMyKeysAsync();
        }
        else
        {
            MessageBox.Show(contentOrError, LocalizationService.Get(UiConstants.Titles.ERROR), MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task LoadMyKeysAsync()
    {
        var keys = await ApiService.Instance.GetAsync<List<AccessKeyDto>>(Endpoints.SUBSCRIPTION_KEYS);
        if (keys != null)
        {
            MyKeysDataGrid.ItemsSource = keys;
        }
    }
}