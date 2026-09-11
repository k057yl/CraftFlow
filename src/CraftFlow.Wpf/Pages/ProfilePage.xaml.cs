using CraftFlow.SharedKernel.Constants;
using CraftFlow.Wpf.Models.Auth;
using CraftFlow.Wpf.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CraftFlow.Wpf.Pages;

public partial class ProfilePage : Page
{
    private const int OTP_CODE_LENGTH = 6;

    public ProfilePage()
    {
        InitializeComponent();
        LoadUserData();
        _ = LoadMyKeysAsync();
    }

    private void LoadUserData()
    {
        var user = ApiService.Instance.CurrentUser;
        if (user != null)
        {
            NameTextBlock.Text = user.FullName;
            EmailTextBlock.Text = user.Email;
            RoleTextBlock.Text = user.IsAdmin ? "Системный администратор (Big Boss)" : "Владелец сыроварни";

            if (user.IsAdmin)
            {
                DangerZoneBorder.Visibility = Visibility.Collapsed;
                TenantKeysBorder.Visibility = Visibility.Collapsed;
            }
        }
    }

    private async Task LoadMyKeysAsync()
    {
        var user = ApiService.Instance.CurrentUser;
        if (user?.IsAdmin == true) return;

        var keys = await ApiService.Instance.GetAsync<List<AccessKeyDto>>(Endpoints.SUBSCRIPTION_KEYS);
        if (keys != null)
        {
            MyKeysDataGrid.ItemsSource = keys;
        }
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

    private async void DeleteAccount_Click(object sender, RoutedEventArgs e)
    {
        var confirm = MessageBox.Show(
            "Вы уверены, что хотите навсегда удалить свой аккаунт? Это действие нельзя отменить.",
            "Подтверждение удаления",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes) return;

        (bool isSuccess, string contentOrError) = await ApiService.Instance.DeleteAndReadAsync(Endpoints.DELETE_ACCOUNT);

        if (isSuccess)
        {
            MessageBox.Show("Ваш аккаунт был успешно удален.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            ApiService.Instance.ClearAuthToken();

            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.NavigateToAuth();
            }
        }
        else
        {
            StatusTextBlock.Foreground = Brushes.Red;
            StatusTextBlock.Text = contentOrError.Trim('"');
        }
    }
}