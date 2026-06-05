using Avalonia.Controls;
using Headquartz.App.ViewModels;

namespace Headquartz.App.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new RootViewModel();
    }
}