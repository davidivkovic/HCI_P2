using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using P2.Primitives;

namespace P2.Model;

public enum SeatType
{
    None,
    Override,
    Single,
    Double,
    DoubleTable
}

public enum TrainType
{
    Falcon,
    Regio,
    InterCity
}

public class Slot : Entity
{
    public int Row { get; set; }
    public int Col { get; set; }
    public int SeatNumber { get; set; }
    public SeatType SeatType { get; set; }
}

public class Seat : Entity
{
    public virtual List<Slot> Slots { get; set; }
}

public class Train : Entity
{
    public virtual List<Seat> Seating { get; set; }
    public string Number { get; set; }
    public TrainType Type { get; set; }
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
    public string FormattedPrice => Number > 1 ? $"{Price} RSD" : "";
}

public class TrainLine : Entity
{
    public virtual Station Source { get; set; }
    public virtual Station Destination { get; set; }
    public virtual List<Stop> Stops { get; set; } // Includes source and destination stops
}

public class Departure : Entity
{
    public virtual Train Train { get; set; }
    public virtual TrainLine Line { get; set; }
    public TimeOnly Time { get; set; }
}