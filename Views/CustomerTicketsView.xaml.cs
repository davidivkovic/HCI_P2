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
using Microsoft.EntityFrameworkCore;
using P2.Model;
using P2.Windows;

namespace P2.Views;

public partial class CustomerTicketsView : Primitives.Component
{
    public ObservableCollection<Ticket> Tickets { get; set; }

    public Departure Departure { get; set; }
    public DateOnly DepartureDate { get; set; }
    public ObservableCollection<List<Slot>> Seats { get; set; }
    public ObservableCollection<Slot> TakenSeats { get; set; } = new() 
    { 
        new() { SeatNumber = 1 },
        new() { SeatNumber = 2 },
        new() { SeatNumber = 4 },
    };
    public string PleaseText { get; set; } = "Molimo Vas odaberite neko od sedišta";
    public string TotalPrice { get; set; } = "0.00 RSD";
    public bool IsReturnTicket { get; set; }

    public void OnIsReturnTicketChanged() => TotalPrice = GetPrice();

    public string GetPrice()
    {
        var price = TakenSeats.Count * Departure?.Line?.Stops?.LastOrDefault()?.Price ?? 0;
        if (IsReturnTicket) price *= 1.5;
        var culture = (CultureInfo)new CultureInfo("sr-Latn-RS").Clone();
        culture.NumberFormat.CurrencyGroupSeparator = ",";
        culture.NumberFormat.CurrencyDecimalSeparator = ".";
        return price.ToString("C2", culture);
    }

    public CustomerTicketsView()
    {
        InitializeComponent();
        Tickets = new();
        using DbContext db = new();
        Departure = new()
        {
            Time = TimeOnly.FromDateTime(DateTime.Now),
            Train = db.Trains
                .Include(t => t.Seating)
                .ThenInclude(s => s.Seats)
                .First(),
            Line = new()
            {
                Source = db.Stations.First(),
                Destination = db.Stations.Skip(1).First(),
                Stops = new()
                {
                    new()
                    {
                        Number = 1,
                        Price = 0,
                        Station = db.Stations.First(),
                        Duration = TimeSpan.FromMinutes(0)
                    },
                    new()
                    {
                        Number = 2,
                        Price = 300,
                        Station = db.Stations.Skip(1).First(),
                        Duration = TimeSpan.FromMinutes(145)
                    }
                }
            }
        };
        DepartureDate = DateOnly.FromDateTime(DateTime.Today);

        Seats = new(GetSeats());
        var takenSeats = db.Tickets
          .Where(t => t.Departure.Line.Source.Id == Departure.Line.Source.Id)
          .Where(t => t.Departure.Line.Destination.Id == Departure.Line.Destination.Id)
          .Where(t => t.Departure.Train.Id == Departure.Train.Id)
          .Where(t => t.DepartureDate == DepartureDate)
          .SelectMany(t => t.Seats)
          .ToList();

        Seats.SelectMany(s => s)
             .Where(s => takenSeats.Any(ts => ts.Row == s.Row && ts.Col == s.Col))
             .ToList()
             .ForEach(s => s.PreviewSeatType = SeatType.Taken);
    }
    public List<List<Slot>> GetSeats()
    {
        if (Departure.Train is null) return new();

        var slots = Enumerable.Range(0, 48)
        .Select(index => new Slot
        {
            Row = index / 4,
            Col = index % 4
        })
        .Chunk(4)
        .ToDictionary(sa => sa[0]?.Row, sa => sa.ToList());

        Departure.Train.Seating
            .ForEach(sg =>
                sg.Seats.ForEach(s =>
                {
                    slots[s.Row][s.Col].SeatNumber = s.SeatNumber;
                    slots[s.Row][s.Col].SeatType = s.SeatType;
                    slots[s.Row][s.Col].Id = sg.Id;
                })
            );

        return slots.Select(s => s.Value).ToList();
    }


}
