using CraftFlow.SharedKernel.Enums;
using CraftFlow.Wpf.Pages.Profile;
using System.Windows;
using System.Windows.Controls;
using static CraftFlow.Wpf.Services.LocalizationService;

namespace CraftFlow.Wpf.Pages.Subscriptions;

public partial class ModulePreviewPage : Page
{
    private readonly AppModule _module;

    public ModulePreviewPage() : this(AppModule.CHEESE_MAKING) { }

    public ModulePreviewPage(AppModule module)
    {
        InitializeComponent();
        _module = module;
        BindContent();
    }

    private void BindContent()
    {
        var (titleKey, subtitleKey, featureKeys) = _module switch
        {
            AppModule.CHEESE_MAKING => (
                "UI_SUB_MOD_CHEESE_TITLE",
                "UI_SUB_MOD_CHEESE_SUBTITLE",
                new[]
                {
                    "UI_SUB_MOD_CHEESE_FEAT_1",
                    "UI_SUB_MOD_CHEESE_FEAT_2",
                    "UI_SUB_MOD_CHEESE_FEAT_3",
                    "UI_SUB_MOD_CHEESE_FEAT_4"
                }),
            AppModule.BREWERY => (
                "UI_SUB_MOD_BREWERY_TITLE",
                "UI_SUB_MOD_BREWERY_SUBTITLE",
                new[]
                {
                    "UI_SUB_MOD_BREWERY_FEAT_1",
                    "UI_SUB_MOD_BREWERY_FEAT_2",
                    "UI_SUB_MOD_BREWERY_FEAT_3",
                    "UI_SUB_MOD_BREWERY_FEAT_4"
                }),
            AppModule.MEAT_PROCESSING => (
                "UI_SUB_MOD_MEAT_TITLE",
                "UI_SUB_MOD_MEAT_SUBTITLE",
                new[]
                {
                    "UI_SUB_MOD_MEAT_FEAT_1",
                    "UI_SUB_MOD_MEAT_FEAT_2",
                    "UI_SUB_MOD_MEAT_FEAT_3",
                    "UI_SUB_MOD_MEAT_FEAT_4"
                }),
            AppModule.FERMENTATION => (
                "UI_SUB_MOD_FERM_TITLE",
                "UI_SUB_MOD_FERM_SUBTITLE",
                new[]
                {
                    "UI_SUB_MOD_FERM_FEAT_1",
                    "UI_SUB_MOD_FERM_FEAT_2",
                    "UI_SUB_MOD_FERM_FEAT_3",
                    "UI_SUB_MOD_FERM_FEAT_4"
                }),
            _ => ("UI_SUB_MOD_DEFAULT_TITLE", "UI_SUB_MOD_DEFAULT_SUBTITLE", Array.Empty<string>())
        };

        ModuleTitleText.Text = Get(titleKey);
        ModuleSubtitleText.Text = Get(subtitleKey);
        CapabilitiesList.ItemsSource = featureKeys.Select(Get).ToArray();
    }

    private void GoToPlans_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(new ProfilePage(_module));
    }
}