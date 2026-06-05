using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Headquartz.App;

public partial class ProfileCreationView : UserControl
{
    public ProfileCreationView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}