using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.Wpf.Models;
using CraftFlow.Wpf.Services;

namespace CraftFlow.Wpf.Pages;

public partial class MrpPage : Page
{
    public ObservableCollection<MaterialRequirementDto> Requirements { get; } = [];

    public MrpPage()
    {
        InitializeComponent();
        MrpDataGrid.ItemsSource = Requirements;
    }

    private async void CalculateMrp_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var report = await ApiService.Instance.GetMrpRequirementsAsync();

            Requirements.Clear();
            if (report.IsSuccess && report.Value != null)
            {
                report.Value.Requirements.ForEach(Requirements.Add);
                SetStatus(UiConstants.Messages.DATA_LOADED_SUCCESS, Brushes.Green);
            }
            else
            {
                SetStatus(UiConstants.Messages.DATA_LOAD_ERROR, Brushes.Red);
            }
        }
        catch (Exception ex)
        {
            SetStatus($"{UiConstants.Messages.DATA_LOAD_ERROR}: {ex.Message}", Brushes.Red);
        }
    }

    private void SetStatus(string msg, Brush color)
    {
        StatusTextBlock.Foreground = color;
        StatusTextBlock.Text = LocalizationService.Get(msg);
    }
}