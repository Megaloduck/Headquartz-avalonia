using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Headquartz.App.Models.Onboarding;
using Headquartz.App.ViewModels;

namespace Headquartz.App;

public partial class LobbyView : UserControl
{
    public LobbyView()
    {
        InitializeComponent();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private void OnCardPointerEntered(object? sender, PointerEventArgs e)
    {
        if (sender is Control { DataContext: DepartmentSelectionCard card } &&
            DataContext is LobbyViewModel vm)
        {
            vm.SetPreviewCommand.Execute(card);
        }
    }
}