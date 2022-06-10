
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using P2;
using P2.Views;
using P2.Windows;

namespace P2
{
    public class HelpProvider
    {
        public static string GetHelpKey(DependencyObject obj)
        {
            return obj.GetValue(HelpKeyProperty) as string;
        }

        public static void SetHelpKey(DependencyObject obj, string value)
        {
            obj.SetValue(HelpKeyProperty, value);
        }

        public static readonly DependencyProperty HelpKeyProperty=
            DependencyProperty.RegisterAttached("HelpKey", typeof(string), typeof(HelpProvider), new PropertyMetadata("index", HelpKey));
        private static void HelpKey(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //NOOP
        }

        public static void ShowHelp(string key, MainWindow originator)
        {
            HelpViewer hh = new HelpViewer(key, originator);
            hh.Show();
        }

        internal static void ShowHelp(string key, LinesView originator)
        {
            HelpViewer hh = new HelpViewer(key, originator);
            hh.Show();
        }

        internal static void ShowHelp(string key, CreateUpdateLine originator)
        {
            HelpViewer hh = new HelpViewer(key, originator);
            hh.Show();
        }

        internal static void ShowHelp(string key, BuyTicket originator)
        {
            HelpViewer hh = new HelpViewer(key, originator);
            hh.Show();
        }

        internal static void ShowHelp(string key, AddEditTrain originator)
        {
            HelpViewer hh = new HelpViewer(key, originator);
            hh.Show();
        }

        internal static void ShowHelp(string key, AddEditTimetable originator)
        {
            HelpViewer hh = new HelpViewer(key, originator);
            hh.Show();
        }



    }
    
}
