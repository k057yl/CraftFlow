using CraftFlow.SharedKernel.Constants;
using CraftFlow.Wpf.Models;
using CraftFlow.Wpf.Services;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CraftFlow.Wpf.Pages;

public partial class AuthPage : Page
{
    public AuthPage()
    {
        InitializeComponent();
    }

    private async void Login_Click(object sender, RoutedEventArgs e)
    {
        await ExecuteWithLockAsync(async () =>
        {
            bool rememberMe = RememberMeCheckBox.IsChecked ?? false;

            var response = await ApiService.Instance.PostAsync(Endpoints.LOGIN, new
            {
                Email = LoginEmailTextBox.Text,
                Password = LoginPasswordBox.Password,
                RememberMe = rememberMe
            });

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
                if (result != null)
                {
                    ProcessSuccessfulAuth(result, rememberMe);
                }
            }
            else
            {
                var rawError = await response.Content.ReadAsStringAsync();
                SetStatus(rawError.Trim('"').Trim(), Brushes.Red);
            }
        });
    }

    private async void Register_Click(object sender, RoutedEventArgs e)
    {
        await ExecuteWithLockAsync(async () =>
        {
            (bool isSuccess, string contentOrError) = await ApiService.Instance.PostAndReadAsync(Endpoints.REGISTER, new
            {
                CompanyName = RegisterCompanyNameTextBox.Text,
                OwnerEmail = RegisterEmailTextBox.Text,
                OwnerPassword = RegisterPasswordBox.Password,
                OwnerFullName = RegisterNameTextBox.Text
            });

            if (isSuccess)
            {
                SetStatus(UiConstants.Messages.REGISTER_SUCCESS, Brushes.Green);
                OtpEmailTextBox.Text = RegisterEmailTextBox.Text;
                SwitchToPanel(OtpPanel);
            }
            else
            {
                SetStatus(contentOrError, Brushes.Red);
            }
        });
    }

    private async void VerifyOtp_Click(object sender, RoutedEventArgs e)
    {
        await ExecuteWithLockAsync(async () =>
        {
            var response = await ApiService.Instance.PostAsync(Endpoints.VERIFY_OTP, new
            {
                Email = OtpEmailTextBox.Text,
                OtpCode = OtpCodeTextBox.Text
            });

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
                if (result != null && !string.IsNullOrEmpty(result.Token))
                {
                    ProcessSuccessfulAuth(result, true);
                }
                else
                {
                    SetStatus(UiConstants.Messages.EMPTY_TOKEN_ERROR, Brushes.Red);
                }
            }
            else
            {
                var rawError = await response.Content.ReadAsStringAsync();
                SetStatus(rawError.Trim('"').Trim(), Brushes.Red);
            }
        });
    }

    private async void ResendOtp_Click(object sender, RoutedEventArgs e)
    {
        await ExecuteWithLockAsync(async () =>
        {
            (bool isSuccess, string contentOrError) = await ApiService.Instance.PostAndReadAsync(Endpoints.RESEND_OTP, new
            {
                Email = OtpEmailTextBox.Text
            });

            if (isSuccess)
            {
                SetStatus(UiConstants.Messages.OTP_RESENT_SUCCESS, Brushes.Green);
            }
            else
            {
                SetStatus(contentOrError, Brushes.Red);
            }
        });
    }

    private void ShowRegister_Click(object sender, RoutedEventArgs e) => SwitchToPanel(RegisterPanel);
    private void ShowLogin_Click(object sender, RoutedEventArgs e) => SwitchToPanel(LoginPanel);

    private void SwitchToPanel(StackPanel targetPanel)
    {
        LoginPanel.Visibility = Visibility.Collapsed;
        RegisterPanel.Visibility = Visibility.Collapsed;
        OtpPanel.Visibility = Visibility.Collapsed;

        targetPanel.Visibility = Visibility.Visible;
        StatusTextBlock.Text = string.Empty;
    }

    private void ProcessSuccessfulAuth(LoginResponseDto result, bool rememberMe)
    {
        if (result.TenantId != Guid.Empty)
        {
            ApiService.Instance.SetTenantHeader(result.TenantId);
        }

        ApiService.Instance.SetAuthToken(result.Token, rememberMe);

        NavigationService?.Navigate(new DashboardPage());
    }

    private void SetStatus(string msg, Brush color)
    {
        StatusTextBlock.Foreground = color;
        StatusTextBlock.Text = LocalizationService.Get(msg);
    }

    private async Task ExecuteWithLockAsync(Func<Task> action)
    {
        try
        {
            IsEnabled = false;
            await action();
        }
        finally
        {
            IsEnabled = true;
        }
    }
}