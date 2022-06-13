using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.EntityFrameworkCore;
using CommunityToolkit.Mvvm.Input;
using P2.Model;
using P2.Stores;
using System.Diagnostics;

namespace P2.Windows;

public partial class BuyTicket : Primitives.Window
{
    public Departure Departure { get; set; }
    public DateOnly DepartureDate { get; set; }
    public Station StartStation { get; set; }
    public Station EndStation { get; set; }
    public ObservableCollection<List<Slot>> Seats { get; set; }
    public ObservableCollection<Slot> TakenSeats { get; set; } = new();
    public string PleaseText { get; set; } = "Molimo Vas odaberite neko od sedišta";
    public string TotalPrice { get; set; } = "0 RSD";
    public bool IsReturnTicket { get; set; }
    public bool Saved { get; set; }
    public bool Cancelled { get; set; }

    public void OnIsReturnTicketChanged() => TotalPrice = CalculatePrice().ToString("C", new CultureInfo("sr-Latn-RS"));

    public double CalculatePrice()
    {
        double stopPrice = Departure?.Line?.Stops.Where(s => s.Station.Name == EndStation.Name).FirstOrDefault().Price ?? 0;
        var price = TakenSeats.Count * stopPrice;
        if (IsReturnTicket) price *= 1.5;
        return price;
    }

    public BuyTicket(Departure departure, DateOnly departureDate, Station startStation, Station endStation)
    {
        InitializeComponent();
        using DbContext db = new();
        Departure = departure;
        DepartureDate = departureDate;
        StartStation = startStation;
        EndStation = endStation;

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
            if (s.PreviewSeatType == SeatType.Taken || s.SeatType == SeatType.None) return;
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
            TotalPrice = CalculatePrice().ToString("C", new CultureInfo("sr-Latn-RS"));
        }
    }

    [ICommand]
    public void Buy()
    {
        List<string> errors = new();
        if(TakenSeats.Count == 0)
        {
            errors.Add("Morate odabrati bar jedno sedište");
        }

        var w = new ConfirmCancelWindow
        {
            Message = errors.Count > 0 ? "Nije moguće kupiti kartu zbog sledećeg:" : "Da li ste sigurni da želite da kupite kartu?",
            ConfirmButtonText = errors.Count > 0 ? "U redu" : "Kupi",
            Errors = errors,
            Image = errors.Count > 0 ? MessageBoxImage.Error : MessageBoxImage.Question
        };
        w.ShowDialog();

        if(errors.Count == 0 && w.Confirmed)
        {
            List<Seat> seats = TakenSeats.Select(s => new Seat()
            {
                Col = s.Col,
                Row = s.Row,
                SeatNumber = s.SeatNumber,
            }).ToList();

            Ticket ticket = new()
            {
                Departure = Departure,
                DepartureDate = DepartureDate,
                Source = StartStation,
                Destination = EndStation,
                IsReturn = IsReturnTicket,
                Price = CalculatePrice(),
                Seats = seats,
                Timestamp = DateTime.Now,
                Customer = UserStore.Store.User
            };

            using DbContext db = new();
            db.Update(ticket);
            db.SaveChanges();

            Saved = true;
            Close();
            
        }
    }


    [ICommand]
    public void Cancel()
    {
        var w = new ConfirmCancelWindow
        {
            Message = "Da li ste sigurni da želite da odustanete od kupovine karte?",
            ConfirmButtonText = "Odustani",
            CancelButtonText = "Otkaži",
            ConfirmIsDanger = true,
            Image = MessageBoxImage.Stop
        };
        w.ShowDialog();

        if (w.Confirmed)
        {
            Cancelled = true;
            Close();
        }
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        if (!Cancelled && !Saved) // Closed by pressing x
        {
            var w = new ConfirmCancelWindow
            {
                Message = "Da li ste sigurni da želite da odustanete od kupovine karte?",
                ConfirmButtonText = "Odustani",
                CancelButtonText = "Otkaži",
                ConfirmIsDanger = true,
                Image = MessageBoxImage.Stop
            };
            w.ShowDialog();

            if (!w.Confirmed) e.Cancel = true;
        }
        base.OnClosing(e);
    }

    private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        HelpProvider.ShowHelp("BuyTicket", this);
    }
}