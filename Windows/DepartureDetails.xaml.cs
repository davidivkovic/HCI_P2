using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.Input;
using P2.Model;
using P2.Primitives;

namespace P2.Windows;

public class GridModel
{
    public Station Station { get; set; }
    public TimeOnly DepartureTime { get; set; }
    public TimeOnly ArrivalTime { get; set; }
    public double Price { get; set; }

    public string DepartureTableTime => DepartureTime != default ? DepartureTime.ToString("HH:mm") : "";
    public string ArrivalTableTime => ArrivalTime != default ? ArrivalTime.ToString("HH:mm") : "";
    public string PriceTable => Price > 0 ? $"{Price} RSD" : "";

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
                    ArrivalTime = Data[i-1].DepartureTime.Add(stops[i].CalculateDuration(departure.Train, stops[i-1].Station)),
                    Price = stops[i].Price
                });
            }
            else
            {
                Data.Add(new GridModel()
                {
                    Station = stops[i].Station,
                    ArrivalTime = Data[i-1].DepartureTime.Add(stops[i].CalculateDuration(departure.Train, stops[i - 1].Station)),
                    DepartureTime = Data[i-1].DepartureTime.Add(stops[i].CalculateDuration(departure.Train, stops[i - 1].Station)).Add(TimeSpan.FromMinutes(1)),
                    Price = stops[i].Price
                });
            }
          
        }
        InitializeComponent();
        CloseButton.Focus();
    }

    public Departure Departure { get; set; }
    public List<GridModel> Data { get; set; } = new();

    [ICommand]
    public void CloseDetails()
    {
        Close();
    }
}
