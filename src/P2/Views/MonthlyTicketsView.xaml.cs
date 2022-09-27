using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.EntityFrameworkCore;
using P2.Model;
using P2.Primitives;

namespace P2.Views;

public partial class MonthlyTicketsView : Component
{
    public ObservableCollection<Ticket> Tickets { get; set; }
    public Ticket SelectedTicket { get; set; }
    public CultureInfo Culture { get; set; } = CultureInfo.CurrentCulture;
    public DateTime SelectedDate { get; set; } = DateTime.Now;
    public string FormattedDate => Culture.TextInfo.ToTitleCase(SelectedDate.ToString("MMMM yyyy", Culture));
    public Visibility CanvasVisibility { get; set; } = Visibility.Visible;

    public MonthlyTicketsView()
    {
        var culture = (CultureInfo)new CultureInfo("sr-Latn-RS").Clone();
        culture.NumberFormat.CurrencyGroupSeparator = ",";
        culture.NumberFormat.CurrencyDecimalSeparator = ".";
        Culture = culture;

        InitializeComponent();
        Calendar.Visibility = Visibility.Hidden;
        CalendarBorder.Focus();

        Dispatcher.BeginInvoke(() =>
        {
            Calendar.DisplayMode = CalendarMode.Year;
            Calendar.SelectedDate = SelectedDate;
            Calendar.Visibility = Visibility.Visible;
            CanvasVisibility = Visibility.Collapsed;
        }, DispatcherPriority.Loaded);


        using DbContext db = new();
        Tickets = new(db.Tickets
            .Include(t => t.Seats)
            .Include(t => t.Source)
            .Include(t => t.Destination)
            .Include(t => t.Departure)
                .ThenInclude(d => d.Train)
            .Include(t => t.Departure)
                .ThenInclude(d => d.Line)
                    .ThenInclude(l => l.Stops)
            .Where(t => t.Timestamp.Month == SelectedDate.Month)
        );
    }

    private void CalendarDisplayChanged(object sender, CalendarModeChangedEventArgs e)
    {
        Calendar.DisplayMode = CalendarMode.Year;
        SelectedDate = Calendar.DisplayDate;
        CanvasVisibility = Visibility.Collapsed;
        Calendar.Visibility = CanvasVisibility;
        CalendarBorder.Focus();

        Tickets.Clear();

        using DbContext db = new();
        var tickets = db.Tickets
            .Include(t => t.Seats)
            .Include(t => t.Source)
            .Include(t => t.Destination)
            .Include(t => t.Departure)
                .ThenInclude(d => d.Train)
            .Include(t => t.Departure)
                .ThenInclude(d => d.Line)
                    .ThenInclude(l => l.Stops)
            .Where(t => t.Timestamp.Month == SelectedDate.Month)
            .ToList();
        tickets.ForEach(t => Tickets.Add(t));
    }

    private void BorderMouseDown(object sender, MouseButtonEventArgs e)
    {
        CalendarBorder.Focus();
        ToggleCanvasVisibility();
    }

    private void ToggleCanvasVisibility()
    {
        CanvasVisibility = CanvasVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        Calendar.Visibility = CanvasVisibility;
    }

    private void BorderKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            ToggleCanvasVisibility();
        }
    }

    private void PreviewGridMouseDown(object sender, MouseButtonEventArgs e)
    {
        var element = e.OriginalSource as FrameworkElement;
        if (element?.Name != "CalendarBorder" && !Calendar.IsMouseOver)
        {
            CanvasVisibility = Visibility.Collapsed;
            Calendar.Visibility = CanvasVisibility;
        }
    }
}
