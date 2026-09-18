using CraftFlow.Api.Modules.Identity;
using CraftFlow.Api.Modules.Identity.Domain;
using CraftFlow.Api.Modules.Subscriptions;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Dtos;
using CraftFlow.SharedKernel.Dtos.Auth;
using CraftFlow.Wpf.Models.Auth;
using CraftFlow.Wpf.Services;
using CraftFlow.Wpf.Windows;
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
        InitRoleComboBox();
        LoadUserData();
        _ = LoadMyKeysAsync();
    }

    private void InitRoleComboBox()
    {
        NewUserRoleComboBox.ItemsSource = new[]
        {
            new { Role = TenantRole.Technologist, Display = "Технолог" },
            new { Role = TenantRole.Storekeeper, Display = "Кладовщик" },
            new { Role = TenantRole.SalesManager, Display = "Менеджер / Продажи" }
        };
        NewUserRoleComboBox.DisplayMemberPath = "Display";
        NewUserRoleComboBox.SelectedValuePath = "Role";
        NewUserRoleComboBox.SelectedIndex = 0;
    }

    private async void LoadUserData()
    {
        var user = ApiService.Instance.CurrentUser;
        if (user != null)
        {
            NameTextBlock.Text = user.FullName;
            EmailTextBlock.Text = user.Email;

            RoleTextBlock.Text = user.Role switch
            {
                TenantRole.SuperAdmin => "Системный администратор (Big Boss)",
                TenantRole.Owner => "Владелец сыроварни",
                TenantRole.Technologist => "Главный технолог",
                TenantRole.Storekeeper => "Кладовщик",
                TenantRole.SalesManager => "Менеджер по продажам",
                _ => "Сотрудник"
            };

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                var profile = await ApiService.Instance.GetAsync<UserProfileDto>("api/identity/profile");
                if (profile != null && !string.IsNullOrWhiteSpace(profile.Email))
                {
                    user.Email = profile.Email;
                    user.Role = profile.Role;
                    EmailTextBlock.Text = profile.Email;
                }
            }

            bool isOwner = user.Role == TenantRole.Owner;

            TeamManagementBorder.Visibility = isOwner ? Visibility.Visible : Visibility.Collapsed;
            DangerZoneBorder.Visibility = isOwner ? Visibility.Visible : Visibility.Collapsed;

            if (user.Role == TenantRole.SuperAdmin)
            {
                TenantKeysBorder.Visibility = Visibility.Collapsed;
            }
        }
    }

    private async void CreateTeamUser_Click(object sender, RoutedEventArgs e)
    {
        var fullName = NewUserFullNameTextBox.Text.Trim();
        var email = NewUserEmailTextBox.Text.Trim();
        var password = NewUserPasswordBox.Password;

        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            MessageBox.Show("Заполните все поля для создания нового сотрудника!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var selectedRole = (TenantRole)(NewUserRoleComboBox.SelectedValue ?? TenantRole.Technologist);

        var requestPayload = new CreateTenantUserRequest(email, password, fullName, selectedRole);

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(IdentityConstants.USERS, requestPayload);

        if (isSuccess)
        {
            MessageBox.Show($"Сотрудник {fullName} успешно создан!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            NewUserFullNameTextBox.Clear();
            NewUserEmailTextBox.Clear();
            NewUserPasswordBox.Clear();
        }
        else
        {
            MessageBox.Show(contentOrError, "Ошибка при создании", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task LoadMyKeysAsync()
    {
        var user = ApiService.Instance.CurrentUser;
        if (user?.Role == TenantRole.SuperAdmin) return;

        var keys = await ApiService.Instance.GetAsync<List<AccessKeyDto>>(SubscriptionsConstants.SUBSCRIPTION_KEYS);
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

        (bool isSuccess, string contentOrError) = await ApiService.Instance.PostAndReadAsync(SubscriptionsConstants.ACTIVATE_KEY_ENDPOINT, new
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
        var user = ApiService.Instance.CurrentUser;
        if (user == null) return;

        var dialog = new ConfirmDeleteAccountWindow(user.Email)
        {
            Owner = Window.GetWindow(this)
        };

        if (dialog.ShowDialog() != true) return;

        string targetEndpoint = dialog.ActionType == AccountDeleteAction.Deactivate
            ? $"{IdentityConstants.DELETE_ACCOUNT}/deactivate"
            : $"{IdentityConstants.DELETE_ACCOUNT}/confirm";

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(targetEndpoint, new
        {
            ConfirmationEmail = dialog.EnteredEmail
        });

        if (isSuccess)
        {
            string message = dialog.ActionType == AccountDeleteAction.Deactivate
                ? "Ваш аккаунт деактивирован. Вы можете восстановить доступ в любое время."
                : "Ваш аккаунт и все данные сыроварни были полностью стерты.";

            MessageBox.Show(message, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);

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