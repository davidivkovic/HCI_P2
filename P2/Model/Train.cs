using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace P2.Model;

public enum SeatType
{
    None,
    Override,
    Single,
    Double,
    DoubleTable,
    Taken
}

public enum TrainType
{
    Falcon,
    Regio,
    InterCity
}

public class Seat : Entity
{
    public int Row { get; set; }
    public int Col { get; set; }
    public int SeatNumber { get; set; }
    public SeatType SeatType { get; set; }
}

public class SeatGroup : Entity
{
    public virtual List<Seat> Seats { get; set; }
    public SeatType SeatType { get; set; }
}

public class Train : Entity
{
    public virtual List<SeatGroup> Seating { get; set; }
    public string Number { get; set; }
    public TrainType Type { get; set; }

    public string Image => Type switch
    {
        TrainType.Falcon => "/Assets/Images/soko.png",
        TrainType.Regio => "/Assets/Images/regio.jpg",
        TrainType.InterCity or _ => "/Assets/Images/intercity.jpg"
    };

    public string Name => Type switch
    {
        TrainType.Falcon => "Soko",
        TrainType.Regio => "Regio",
        TrainType.InterCity or _ => "Inter City"
    };

    public double Speed => Type switch
    {
        TrainType.Falcon => 120,
        TrainType.Regio => 80,
        TrainType.InterCity or _ => 100
    };

    public int NumberOfSeats => Seating.Select(sg => sg).SelectMany(s => s.Seats).Count();
}

public class Station : Entity
{
    public string Name { get; set; }
    public double Longitude { get; set; }
    public double Latitude { get; set; }
    public double DistanceTo(Station s)
    {
        var p = 0.017453292519943295;    // Math.PI / 180
        var a = 0.5 - Math.Cos((s.Latitude - Latitude) * p) / 2 +
                Math.Cos(Latitude * p) * Math.Cos(s.Latitude * p) *
                (1 - Math.Cos((s.Longitude - Longitude) * p)) / 2;

        return 12742 * Math.Asin(Math.Sqrt(a)); // 2 * R; R = 6371 km
    }
}

public class Stop : Entity
{
    public int Number { get; set; }
    public virtual Station Station { get; set; }
    public double Price { get; set; }
    public TimeSpan Duration { get; set; }
    public string FormattedDuration => Number > 1 ? $"{Duration:hh}h {Duration:mm}m od prethodne stanice" : "početna stanica";
    public string FormattedDistance=> Number > 1 ? $"{Math.Round(80 * Duration.TotalMinutes / 60)} km od prethodne stanice" : "početna stanica";
    public string FormattedPrice => Number > 1 ? $"{Price} RSD" : "";
    public TimeSpan CalculateDuration(Train train, Station station) => TimeSpan.FromHours(Station.DistanceTo(station) / train.Speed);
}

public class TrainLine : Entity
{
    public virtual Station Source { get; set; }
    public virtual Station Destination { get; set; }
    public virtual List<Stop> Stops { get; set; } // Includes source and destination stops
    public string FormattedLine => $"{Source?.Name} \u2192 {Destination?.Name}";
    public void UpdateRoute()
    {
        for (int i = 0; i < Stops.Count; i++)
        {
            Stops[i].Number = i + 1;
            if (i > 0)
            {
                Station previousStation = Stops[i - 1].Station;
                Stops[i].Duration = TimeSpan.FromHours(Stops[i].Station.DistanceTo(previousStation) / 80);
            }
            else
            {
                Stops[i].Duration = TimeSpan.FromMinutes(0);
                Stops[i].Price = 0;
            }
        }
    }
}

public class Departure : Entity
{
    public virtual Train Train { get; set; }
    public virtual TrainLine Line { get; set; }
    public TimeOnly Time { get; set; }
    public TimeSpan TravelTime()
    {
        TimeSpan totalDurations = Line.Stops.First().CalculateDuration(Train, Line.Stops.First().Station);
        for(int i = 1; i < Line.Stops.Count; i++)
        {
            totalDurations = totalDurations.Add(Line.Stops[i].CalculateDuration(Train, Line.Stops[i - 1].Station));
        }
        totalDurations = totalDurations.Add(TimeSpan.FromMinutes(Line.Stops.Count - 2));
        return totalDurations;
    }

    public string TableTime => Time.ToString("HH:mm");
    public string ArrivalTableTime => Time.Add(TravelTime()).ToString("HH:mm");
    public string TravelTableTime => $"{TravelTime():hh}h {TravelTime():mm}m";
}

public class Ticket : Entity
{
    public User Customer { get; set; }
    public Departure Departure { get; set; }
    public Station Source { get; set; }
    public Station Destination { get; set; }
    public List<Seat> Seats { get; set; }
    public DateOnly DepartureDate { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsReturn { get; set; }
    public bool IsConfirmed { get; set; }
    public bool IsReservation { get; set; }
    public double Price { get; set; }

    public bool IsUnconfirmedUnexpiredReservation => !ReservationExpired && !IsConfirmed && IsReservation; 
    public bool ReservationExpired => DepartureDate < DateOnly.FromDateTime(DateTime.Now);
    public string FormattedPrice => Price.ToString("C", new CultureInfo("sr-Latn-RS"));
    public string FormattedPriceNoSymbol => Price.ToString("N0", new CultureInfo("sr-Latn-RS"));
}