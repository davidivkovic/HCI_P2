using System;
using System.Collections.Generic;
using P2.Model;
using P2.Primitives;

namespace P2.Windows;

/// <summary>
/// Interaction logic for DepartureDetails.xaml
/// </summary>
/// 
public class GridModel
{
    public Station Station { get; set; }
    public TimeOnly DepartureTime { get; set; }
    public TimeOnly ArrivalTime { get; set; }

    public string DepartureTableTime => DepartureTime != default ? DepartureTime.ToString("HH:mm") : "";
    public string ArrivalTableTime => ArrivalTime != default ? ArrivalTime.ToString("HH:mm") : "";


}
public partial class DepartureDetails : Window
{
    public DepartureDetails(Departure departure)
    {
        Departure = departure;

        List<Stop> stops = Departure.Line.Stops;
        int length = stops.Count;

       for(int i = 0; i < length; i++)
        {
            if(i == 0)
            {
                Data.Add(new()
                {
                    Station = stops[i].Station,
                    DepartureTime = Departure.Time
                });
            }
            else if(i == length-1)
            {
                Data.Add(new()
                {
                    Station = stops[i].Station,
                    ArrivalTime = Data[i-1].DepartureTime.Add(stops[i].CalculateDuration(departure.Train, stops[i-1].Station))
                });
            }
            else
            {
                Data.Add(new GridModel()
                {
                    Station = stops[i].Station,
                    ArrivalTime = Data[i-1].DepartureTime.Add(stops[i].CalculateDuration(departure.Train, stops[i - 1].Station)),
                    DepartureTime = Data[i-1].DepartureTime.Add(stops[i].CalculateDuration(departure.Train, stops[i - 1].Station)).Add(TimeSpan.FromMinutes(1))
            });
            }
          
        }
        InitializeComponent();
    }

    public Departure Departure { get; set; }

    public List<GridModel> Data { get; set; } = new();
}
