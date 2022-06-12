using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
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

    public MonthlyTicketsView()
    {
        var culture = (CultureInfo)new CultureInfo("sr-Latn-RS").Clone();
        culture.NumberFormat.CurrencyGroupSeparator = ",";
        culture.NumberFormat.CurrencyDecimalSeparator = ".";
        Culture = culture;

        InitializeComponent();
        Calendar.Visibility = Visibility.Hidden;

        Dispatcher.BeginInvoke(() =>
        {
            Calendar.DisplayMode = CalendarMode.Year;
            Calendar.SelectedDate = SelectedDate;
            Calendar.Visibility = Visibility.Visible;
        }, DispatcherPriority.Loaded);


        using DbContext db = new();
        Tickets = new();
        Ticket t = new()
        {
            Customer = db.Users.First(),
            Id = 4,
            Source = db.Stations.First(),
            Destination = db.Stations.Skip(1).First(),
            Price = 300,
            DepartureDate = DateOnly.FromDateTime(DateTime.Now),
            Departure = new()
            {
                Train = db.Trains.First(),
                Time = TimeOnly.FromDateTime(DateTime.Now),
                Line = db.Lines.Include(l => l.Stops).ThenInclude(s => s.Station).First()
            }
        };

        Tickets.Add(t);
        Tickets.Add(t);
        Tickets.Add(t);
        Tickets.Add(t);
        Tickets.Add(t);
        Tickets.Add(t);
        Tickets.Add(t);
        Tickets.Add(t);
        Tickets.Add(t);
        Tickets.Add(t);
        Tickets.Add(t);
        Tickets.Add(t);
        Tickets.Add(t);
        Tickets.Add(t);
        Tickets.Add(t);
        Tickets.Add(t);
        Tickets.Add(t);
        Tickets.Add(t);
        Tickets.Add(t);
        Tickets.Add(t);
        Tickets.Add(t);
        Tickets.Add(t);
        Tickets.Add(t);
    }

    private void CalendarDisplayChanged(object sender, CalendarModeChangedEventArgs e)
    {
        Calendar.DisplayMode = CalendarMode.Year;
        SelectedDate = Calendar.DisplayDate;
    }
}
