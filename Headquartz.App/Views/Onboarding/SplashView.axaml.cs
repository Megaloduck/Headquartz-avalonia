using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Headquartz.App.Views.Onboarding;

public partial class SplashView : UserControl
{
    public SplashView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}