using System.Net.Http.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.Wpf.Models;
using CraftFlow.Wpf.Services;

namespace CraftFlow.Wpf.Pages;

public partial class AuthPage : Page
{
    private const string DEFAULT_TENANT_ID = "00000000-0000-0000-0000-000000000001";

    public AuthPage()
    {
        InitializeComponent();
    }

    private async void Login_Click(object sender, RoutedEventArgs e)
    {
        var response = await ApiService.Instance.PostAsync(Endpoints.LOGIN, new
        {
            Email = LoginEmailTextBox.Text,
            Password = LoginPasswordBox.Password
        });

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            if (result != null)
            {
                ApiService.Instance.SetAuthToken(result.Token);
                SetStatus($"{UiConstants.Messages.LOGIN_SUCCESS} LOGGED AS: {result.FullName}", Brushes.Green);
            }
        }
        else
        {
            var rawError = await response.Content.ReadAsStringAsync();
            SetStatus(rawError.Trim('"').Trim(), Brushes.Red);
        }
    }

    private async void Register_Click(object sender, RoutedEventArgs e)
    {
        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(Endpoints.REGISTER, new
        {
            TenantId = Guid.Parse(DEFAULT_TENANT_ID),
            Email = RegisterEmailTextBox.Text,
            Password = RegisterPasswordBox.Password,
            ConfirmPassword = RegisterConfirmPasswordBox.Password,
            FullName = RegisterNameTextBox.Text
        });

        if (isSuccess)
        {
            SetStatus(UiConstants.Messages.REGISTER_SUCCESS, Brushes.Green);

            // Автоматом перебрасываем на подгрузку OTP
            OtpEmailTextBox.Text = RegisterEmailTextBox.Text;
            AuthTabControl.SelectedIndex = 2;
        }
        else
        {
            SetStatus(contentOrError, Brushes.Red);
        }
    }

    private async void VerifyOtp_Click(object sender, RoutedEventArgs e)
    {
        var response = await ApiService.Instance.PostAsync(Endpoints.LOGIN, new
        {
            Email = OtpEmailTextBox.Text,
            OtpCode = OtpCodeTextBox.Text
        });

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            if (result != null)
            {
                ApiService.Instance.SetAuthToken(result.Token);
                SetStatus($"{UiConstants.Messages.LOGIN_SUCCESS} LOGGED AS: {result.FullName}", Brushes.Green);
            }
        }
        else
        {
            var rawError = await response.Content.ReadAsStringAsync();
            SetStatus(rawError.Trim('"').Trim(), Brushes.Red);
        }
    }

    private void SetStatus(string msg, Brush color)
    {
        StatusTextBlock.Foreground = color;
        StatusTextBlock.Text = LocalizationService.Get(msg);
    }
}