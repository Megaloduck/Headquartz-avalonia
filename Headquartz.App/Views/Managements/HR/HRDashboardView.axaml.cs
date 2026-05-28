using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Headquartz.App;

public partial class HRDashboardView : UserControl
{
    public HRDashboardView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}