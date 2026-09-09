using CraftFlow.SharedKernel.Constants;
using CraftFlow.Wpf.Models.Auth;
using CraftFlow.Wpf.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace CraftFlow.Wpf.Pages;

public partial class AdminKeysPage : PageFunction<string>
{
    public AdminKeysPage()
    {
        InitializeComponent();
        _ = LoadAllKeysAsync();
    }

    private async void SendOtpToTenant_Click(object sender, RoutedEventArgs e)
    {
        (bool isSuccess, string contentOrError) = await ApiService.Instance.PostAndReadAsync(Endpoints.ACTIVATE_OTP_ENDPOINT, new
        {
            TargetEmail = TargetEmailTextBox.Text,
            DeviceName = KeyNameTextBox.Text
        });

        if (isSuccess)
        {
            MessageBox.Show(LocalizationService.Get(UiConstants.Messages.OTP_SENT_SUCCESS), LocalizationService.Get(UiConstants.Titles.SUCCESS), MessageBoxButton.OK, MessageBoxImage.Information);
            await LoadAllKeysAsync();
        }
        else
        {
            MessageBox.Show(contentOrError, LocalizationService.Get(UiConstants.Titles.ERROR), MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void AdminRevokeKey_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Guid keyId)
        {
            (bool isSuccess, string contentOrError) = await ApiService.Instance.DeleteAndReadAsync($"{Endpoints.SUBSCRIPTION_KEYS}/{keyId}");
            if (isSuccess)
            {
                MessageBox.Show(LocalizationService.Get(UiConstants.Messages.KEY_REVOKED_SUCCESS), LocalizationService.Get(UiConstants.Titles.SUCCESS), MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadAllKeysAsync();
            }
            else
            {
                MessageBox.Show(contentOrError, LocalizationService.Get(UiConstants.Titles.ERROR), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private async Task LoadAllKeysAsync()
    {
        var keys = await ApiService.Instance.GetAsync<List<AccessKeyDto>>(Endpoints.SUBSCRIPTION_KEYS);
        if (keys != null)
        {
            AllKeysDataGrid.ItemsSource = keys;
        }
    }
}