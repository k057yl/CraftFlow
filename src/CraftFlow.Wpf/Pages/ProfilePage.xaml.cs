using CraftFlow.Api.Modules.Identity;
using CraftFlow.Api.Modules.Identity.Domain;
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
    public ProfilePage()
    {
        InitializeComponent();
        InitRoleComboBox();
        LoadUserData();
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

            RoleTextBlock.Text = GetRoleDisplayName(user.Role);

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

            if (isOwner)
            {
                await LoadTeamUsersAsync();
            }
        }
    }

    private async Task LoadTeamUsersAsync()
    {
        var users = await ApiService.Instance.GetTenantUsersAsync();
        var currentUserId = ApiService.Instance.CurrentUser?.Email;

        var viewModels = users
            .Where(u => u.Role != TenantRole.SuperAdmin)
            .Select(u => new
            {
                u.Id,
                u.FullName,
                u.Email,
                RoleDisplay = GetRoleDisplayName(u.Role),
                StatusDisplay = u.IsActive ? "Активен" : "Заблокирован",
                ActionButtonText = u.IsActive ? "Деактивировать" : "Активировать",
                CanToggle = u.Role != TenantRole.Owner && u.Email != currentUserId
            }).ToList();

        TeamUsersDataGrid.ItemsSource = viewModels;
    }

    private async void ToggleUserStatus_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Guid userId)
        {
            var (isSuccess, contentOrError) = await ApiService.Instance.ToggleUserStatusAsync(userId);
            if (isSuccess)
            {
                await LoadTeamUsersAsync();
            }
            else
            {
                MessageBox.Show(contentOrError, "Ошибка смены статуса", MessageBoxButton.OK, MessageBoxImage.Error);
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
            await LoadTeamUsersAsync();
        }
        else
        {
            MessageBox.Show(contentOrError, "Ошибка при создании", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static string GetRoleDisplayName(TenantRole role) => role switch
    {
        TenantRole.Owner => "Владелец сыроварни",
        TenantRole.Technologist => "Главный технолог",
        TenantRole.Storekeeper => "Кладовщик",
        TenantRole.SalesManager => "Менеджер по продажам",
        _ => "Сотрудник"
    };

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