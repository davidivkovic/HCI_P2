using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

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
}

[Owned]
public class Stop
{
    public virtual Station Station { get; set; }
    public double Price { get; set; }
    public TimeSpan Duration { get; set; }
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