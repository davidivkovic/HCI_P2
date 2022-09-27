using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

    public ConfirmCancelWindow() : base(hideCaption: false)
    {
        IntPtr active = GetActiveWindow();
        Owner = Application.Current.Windows
            .OfType<Window>()
            .LastOrDefault(window => new WindowInteropHelper(window).Handle == active);
        InitializeComponent();
        CancelButton.Focus();
    }

    public ControlTemplate GetConfirmButtonStyle()
    {
        if (Errors?.Count > 0) return (ControlTemplate)FindResource("NeutralButtonTemplate");
        else if (ConfirmIsDanger) return (ControlTemplate)FindResource("RedButtonTemplate");
        return (ControlTemplate)FindResource("DefaultButtonTemplate");
    }

    public string GetImageURL()
    {
        if (Errors?.Count > 0) return "/Assets/Icons/warning.png";
        return "/Assets/Icons/" + Image switch
        {
            MessageBoxImage.Question => "question.png",
            MessageBoxImage.Warning or MessageBoxImage.Exclamation => "warning.png",
            MessageBoxImage.Stop or MessageBoxImage.Error => "danger.png",
            MessageBoxImage.Information or _ => "info.jpg"
        };
    }

    [Bind] public object Slot { get; set; }
    public List<string> Errors { get; set; }
    public MessageBoxImage Image { get; set; }
    public string ImageURL => GetImageURL();
    public bool Confirmed { get; set; }
    public bool Cancelled { get; set; }
    public string Message { get; set; }
    public string ConfirmButtonText { get; set; } = "Potvrdi";
    public string CancelButtonText { get; set; } = "Odustani";
    public bool ConfirmIsDanger { get; set; } = false;
    public bool AutoClose { get; set; } = true;
    public ControlTemplate ConfirmButtonStyle => GetConfirmButtonStyle();
    public Visibility CancelButtonVisibility => string.IsNullOrEmpty(CancelButtonText) || Errors?.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
    public Visibility MessageVisibility => Slot is null ? Visibility.Visible : Visibility.Collapsed;
    public ResizeMode CanResize => Slot is null ? ResizeMode.NoResize : ResizeMode.CanResize;
    public Visibility IsImageVisible => Image == MessageBoxImage.None ? Visibility.Collapsed : Visibility.Visible;
    public Action<ConfirmCancelWindow> OnAction { get; set; }

    [ICommand]
    public void Confirm()
    {
        Confirmed = true;
        Cancelled = false;
        OnAction?.Invoke(this);
        if (AutoClose) Close();
    }

    [ICommand]
    public void Cancel()
    {
        Confirmed = false;
        Cancelled = true;
        OnAction?.Invoke(this);
        if (AutoClose) Close();
    }
}