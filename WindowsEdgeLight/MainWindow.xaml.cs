using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace WindowsEdgeLight;

public partial class MainWindow : Window
{
    private bool isLightOn = true;
    private double currentOpacity = 0.95;
    private const double OpacityStep = 0.15;
    private const double MinOpacity = 0.2;
    private const double MaxOpacity = 1.0;

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);
    private const int SM_CXSCREEN = 0;
    private const int SM_CYSCREEN = 1;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void SetupWindow()
    {
        var primaryScreen = System.Windows.Forms.Screen.PrimaryScreen;
        if (primaryScreen == null) return;
        
        var bounds = primaryScreen.Bounds;
        
        // Get DPI scale factor
        var source = PresentationSource.FromVisual(this);
        double dpiScaleX = 1.0;
        double dpiScaleY = 1.0;
        
        if (source != null)
        {
            dpiScaleX = source.CompositionTarget.TransformToDevice.M11;
            dpiScaleY = source.CompositionTarget.TransformToDevice.M22;
        }
        
        // Convert physical pixels to WPF DIPs
        this.Left = bounds.X / dpiScaleX;
        this.Top = bounds.Y / dpiScaleY;
        this.Width = bounds.Width / dpiScaleX;
        this.Height = bounds.Height / dpiScaleY;
        this.WindowState = System.Windows.WindowState.Normal;

        EdgeLightBorder.Margin = new Thickness(20);
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        SetupWindow();
        
        var hwnd = new WindowInteropHelper(this).Handle;
        int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
        SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT | WS_EX_LAYERED);
    }

    private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.L && 
            (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && 
            (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
        {
            ToggleLight();
        }
        else if (e.Key == Key.Escape)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }

    private void Toggle_Click(object sender, RoutedEventArgs e)
    {
        ToggleLight();
    }

    private void ToggleLight()
    {
        isLightOn = !isLightOn;
        EdgeLightBorder.Visibility = isLightOn ? Visibility.Visible : Visibility.Collapsed;
    }

    private void BrightnessUp_Click(object sender, RoutedEventArgs e)
    {
        currentOpacity = Math.Min(MaxOpacity, currentOpacity + OpacityStep);
        EdgeLightBorder.Opacity = currentOpacity;
    }

    private void BrightnessDown_Click(object sender, RoutedEventArgs e)
    {
        currentOpacity = Math.Max(MinOpacity, currentOpacity - OpacityStep);
        EdgeLightBorder.Opacity = currentOpacity;
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        System.Windows.Application.Current.Shutdown();
    }

    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TRANSPARENT = 0x00000020;
    private const int WS_EX_LAYERED = 0x00080000;

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hwnd, int index);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);
}