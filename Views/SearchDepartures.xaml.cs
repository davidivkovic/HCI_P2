using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using P2.Model;
using System.Windows;
using System.Linq;

namespace P2.Views;

/// <summary>
/// Interaction logic for SearchDepartures.xaml
/// </summary>
public partial class SearchDepartures : Primitives.Component
{
    public SearchDepartures()
    {
        var s1 = new Station()
        {
            Name = "Beograd Centar",
            Latitude = 44.755717,
            Longitude = 20.520289
        };

        var s2 = new Station()
        {
            Name = "Novi Sad (MAS)",
            Latitude = 45.248851,
            Longitude = 19.810473
        };

        var s3 = new Station()
        {
            Name = "Nis",
            Latitude = 43.305059,
            Longitude = 21.889582
        };

        AvailableStations.Add(s1);
        AvailableStations.Add(s2);
        AvailableStations.Add(s3);

        var stop1 = new Stop()
        {
            Number = 1,
            Station = s1,
        };

        var stop2 = new Stop()
        {
            Number = 2,
            Station = s2,
            Price = 500,
            Duration = DateTime.Parse("7/6/2022 13:00").TimeOfDay
        };

        var train1 = new Train()
        {
            Number = "741",
            Type = TrainType.Falcon
        };

        var train2 = new Train()
        {
            Number = "742",
            Type = TrainType.Regio
        };

        var tr1 = new TrainLine()
        {
            Source = s1,
            Destination = s2,
            Stops = new List<Stop>()
            {
                stop1, stop2
            }
        };

        var tr2 = new TrainLine()
        {
            Source = s2,
            Destination = s1,
            Stops = new List<Stop>()
            {
                stop2, stop1
            }
        };

        var dep1 = new Departure()
        {
            Train = train1,
            Line = tr1,
            Time = TimeOnly.FromTimeSpan(DateTime.Parse("7/6/2022 12:00").TimeOfDay)
        };

        var dep2 = new Departure()
        {
            Train = train2,
            Line = tr2,
            Time = TimeOnly.FromTimeSpan(DateTime.Parse("7/6/2022 13:00").TimeOfDay)
        };
        Departures.Add(dep1);
        Departures.Add(dep2);

        InitializeComponent();

        Search = new(OnSearch);

    }

    public RelayCommand Search { get; set; }

    public List<Station> AvailableStations { get; set; } = new();

    public List<Departure> Departures { get; set; } = new();

    public ObservableCollection<Departure> FilteredDepartures { get; set; } = new();

    public Object ShowDepartures => FilteredDepartures.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

    public string SelectedStartStation { get; set; }

    public string SelectedEndStation { get; set; }

    public DateOnly SelectedDate { get; set; }

    public void OnSearch()
    {
        FilteredDepartures.Clear();
        foreach (Departure departure in Departures)
        {
            if ((departure.Line.Source.Name.ToLower().Equals(SelectedStartStation.ToLower()))
                && (departure.Line.Source.Name.ToLower().Equals(SelectedStartStation.ToLower())))
                FilteredDepartures.Add(departure);
        }

        FilteredDepartures.ToList().Sort((x, y) => x.Time.CompareTo(y.Time));

        departuresTable.Visibility = (Visibility)ShowDepartures;
        departuresTable.ItemsSource = FilteredDepartures;
        
    }

}
