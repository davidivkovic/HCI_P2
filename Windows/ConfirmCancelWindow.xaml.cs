using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Markup;
using CommunityToolkit.Mvvm.Input;
using Kasay;

namespace P2.Windows;

[ContentProperty("Slot")]
public partial class ConfirmCancelWindow : Primitives.Window
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    public ConfirmCancelWindow() : base(hideCaption: true)
    {
        IntPtr active = GetActiveWindow();
        Owner = Application.Current.Windows
            .OfType<Window>()
            .LastOrDefault(window => new WindowInteropHelper(window).Handle == active);
        InitializeComponent();
        CancelButton.Focus();
    }

    [Bind] public object Slot { get; set; }
    public bool Confirmed { get; set; }
    public bool Cancelled { get; set; }
    public string Message { get; set; }
    public string ConfirmButtonText { get; set; } = "Potvrdi";
    public string CancelButtonText { get; set; } = "Odustani";
    public bool ConfirmIsDanger { get; set; } = false;
    public ControlTemplate ConfirmButtonStyle => (ControlTemplate)FindResource(ConfirmIsDanger ? "RedButtonTemplate" : "DefaultButtonTemplate");
    public Visibility CancelButtonVisibility => string.IsNullOrEmpty(CancelButtonText) ? Visibility.Collapsed : Visibility.Visible;
    public Visibility MessageVisibility => Slot is null ? Visibility.Visible : Visibility.Collapsed;
    public ResizeMode CanResize => Slot is null ? ResizeMode.NoResize : ResizeMode.CanResize;

    [ICommand] public void Confirm()
    {
        Confirmed = true;
        Close();
    }

    [ICommand]
    public void Cancel()
    {
        Cancelled = true;
        Close();
    }
}