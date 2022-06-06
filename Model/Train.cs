using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using P2.Primitives;

namespace P2.Model;

[Owned]
public class Seating
{
    public int Rows { get; set; }
    public int SeatsPerRow { get; set; }
}

public class Train : Entity
{
    public string Name { get; set; }
    public List<Seating> Seating { get; set; }
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

[Owned]
public class Stop : Observable
{
    public int Number { get; set; }
    public virtual Station Station { get; set; }
    public double Price { get; set; }
    public TimeSpan Duration { get; set; }
    //public string FormattedDuration => Duration.Minutes > 0 ? ((Duration.TotalHours >= 1) ? 
    //    $"{Duration.Hours} {Duration.Minutes}m od prethodne stanice" : 
    //    $"{Duration.Minutes}m od prethodne stanice") : "početna stanica";

    public string FormattedDuration => Number > 1 ? $"{Duration:hh}h {Duration:mm}m od prethodne stanice" : "početna stanica";
    public string FormattedPrice => Price > 0 ? $"{Price} RSD" : "";
}

public class TrainLine : Entity
{
    public virtual Station Source { get; set; }
    public virtual Station Destination { get; set; }
    public virtual List<Stop> Stops { get; set; } // Includes source and destination stops
}

public class Departure : Entity
{
    public virtual TrainLine Line { get; set; }
    public TimeOnly Time { get; set; }
}