using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Headquartz.App;

public partial class ShellView : UserControl
{
    public ShellView()
    {
        InitializeComponent();
    }

    // =========================================================
    // CUSTOM TITLE BAR — DRAG TO MOVE
    // =========================================================

    private void OnTitleBarPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            if (GetWindow() is { } window)
            {
                // Double-click on the drag region toggles maximize,
                // matching standard OS title bar behavior.
                if (e.ClickCount == 2)
                {
                    ToggleMaximize(window);
                    return;
                }

                window.BeginMoveDrag(e);
            }
        }
    }

    // =========================================================
    // WINDOW CONTROL BUTTONS
    // =========================================================

    private void OnMinimizeClick(object? sender, RoutedEventArgs e)
    {
        if (GetWindow() is { } window)
            window.WindowState = WindowState.Minimized;
    }

    private void OnMaximizeClick(object? sender, RoutedEventArgs e)
    {
        if (GetWindow() is { } window)
            ToggleMaximize(window);
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        GetWindow()?.Close();
    }

    // =========================================================
    // HELPERS
    // =========================================================

    private Window? GetWindow() =>
        TopLevel.GetTopLevel(this) as Window;

    private void ToggleMaximize(Window window)
    {
        window.WindowState = window.WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;

        UpdateMaximizeIcon(window.WindowState);
    }

    private void UpdateMaximizeIcon(WindowState state)
    {
        if (this.FindControl<Avalonia.Controls.Shapes.Path>("MaximizeIcon") is { } icon)
        {
            icon.Data = state == WindowState.Maximized
                // Restore icon — overlapping squares
                ? Avalonia.Media.Geometry.Parse("M8 8h11v11H8zM5 5h11v3H8a3 3 0 00-3 3v8H5z")
                // Maximize icon — single square
                : Avalonia.Media.Geometry.Parse("M5 5h14v14H5z");
        }
    }
}