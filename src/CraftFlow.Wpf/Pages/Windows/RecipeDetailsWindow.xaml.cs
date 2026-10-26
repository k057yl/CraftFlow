using System.Windows;
using CraftFlow.SharedKernel.Dtos.Catalog.Nomenclature;

namespace CraftFlow.Wpf.Windows;

public partial class RecipeDetailsWindow : Window
{
    public RecipeDetailsWindow(RecipeDto recipe)
    {
        InitializeComponent();

        RecipeNameTextBlock.Text = recipe.Name;
        RecipeInfoTextBlock.Text = $"Выход: {recipe.TargetOutputQuantity} | Созревание: {(recipe.IsAgingRequired ? $"{recipe.DefaultMinAgingDays} дн." : "Не требуется")}";
        IngredientsDataGrid.ItemsSource = recipe.Ingredients;
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}