﻿using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace P2.Primitives;

public partial class Window : System.Windows.Window, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    [DllImport("uxtheme.dll")]
    private static extern int SetWindowThemeAttribute(IntPtr hWnd, WindowThemeAttributeType wtype, ref WTA_OPTIONS attributes, uint size);

    public enum WindowThemeAttributeType : uint
    {
        WTA_NONCLIENT = 1,
    }

    public struct WTA_OPTIONS
    {
        public uint Flags;
        public uint Mask;
    }

    static readonly uint WTNCA_NODRAWCAPTION = 0x00000001;
    readonly static uint WTNCA_NODRAWICON = 0x00000002;
    private WTA_OPTIONS wta = new()
    {
        Flags = WTNCA_NODRAWCAPTION | WTNCA_NODRAWICON,
        Mask = WTNCA_NODRAWCAPTION | WTNCA_NODRAWICON
    };

    public Window(bool hideCaption = false)
    {
        if (hideCaption)
        {
            SourceInitialized += (s, ea) => _ = SetWindowThemeAttribute(new WindowInteropHelper(this).Handle, WindowThemeAttributeType.WTA_NONCLIENT, ref wta, (uint)Marshal.SizeOf(typeof(WTA_OPTIONS)));
        }
    }
}