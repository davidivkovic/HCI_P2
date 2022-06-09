using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.EntityFrameworkCore;
using P2.Model;

namespace P2.Windows;

public partial class BuyTicket : Primitives.Window
{
    public Departure Departure { get; set; }
    public DateOnly DepartureDate { get; set; }
    public ObservableCollection<List<Slot>> Seats { get; set; }
    public ObservableCollection<Slot> TakenSeats { get; set; } = new();
    public string PleaseText { get; set; } = "Molimo Vas odaberite neko od sedišta";

    public BuyTicket()
    {
        InitializeComponent();
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

    private void SeatClicked(object sender, MouseButtonEventArgs e)
    {
        if (sender is Border b && b.Tag is Slot s)
        {
            if (s.SeatType == SeatType.None) return;
            //var selectedSeat = Seats.SelectMany(s => s).FirstOrDefault(seat => seat.Row == s.Row && seat.Col == s.Col);
            var takenSeat = TakenSeats.FirstOrDefault(seat => seat.Row == s.Row && seat.Col == s.Col);
            if (takenSeat is null)
            {
                var insertionPoint = TakenSeats.IndexOf(TakenSeats.FirstOrDefault(seat => seat.SeatNumber > s.SeatNumber));
                if (insertionPoint == -1) TakenSeats.Add(s);
                else TakenSeats.Insert(insertionPoint, s);
                b.BorderThickness = new Thickness(3);
                b.BorderBrush = Brushes.Black;
            }
            else
            {
                TakenSeats.Remove(s);
                b.ClearValue(BorderThicknessProperty);
                b.ClearValue(BorderBrushProperty);
            }
            PleaseText = TakenSeats.Count > 0 ? "" : "Molimo Vas odaberite neko od sedišta";
        }
    }
}