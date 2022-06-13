using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using P2.Model;
using P2.Primitives;

namespace P2.Windows;

public partial class DepartureTickets : Window
{
    public DepartureTickets(Departure departure, DateTime selectedDate)
    {
        Departure = departure;
        SelectedDate = selectedDate;
        SearchTickets();

        InitializeComponent();
        CloseButton.Focus();
    }

    public Departure Departure { get; set; }
    public List<Ticket> Tickets { get; set; }
    public DateTime SelectedDate { get; private set; }
    public void SearchTickets()
    {
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
            .Where(t => t.Timestamp.Date.Date == SelectedDate)
            .Where(t => t.Departure == Departure)
        );
    }

    [ICommand] 
    public void CloseDetails()
    {
        Close();
    }

    private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        HelpProvider.ShowHelp("DepartureTickets", this);
    }
}
