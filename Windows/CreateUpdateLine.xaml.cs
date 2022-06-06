using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maps.MapControl.WPF;
using P2.Model;
using P2.Primitives;

namespace P2.Windows;

public partial class CreateUpdateLine : Primitives.Window
{
    public CreateUpdateLine()
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
            Price = 500
        };

       
        CurrentStops.Add(stop2);
        CurrentStops.Add(stop1);

        FilteredStations = new(AvailableStations);

        UpdateRoute();

        InitializeComponent();

        GeneratePins();
    }

    public List<Station> AvailableStations { get; set; } = new();

    public ObservableCollection<Stop> CurrentStops { get; set; } = new();

    public ObservableCollection<Station> FilteredStations { get; set; }

    public Stop SelectedStop { get; set; }

    public string SearchInputText { get; set; }

    public Visibility IsInputClearable => SearchInputText != null && SearchInputText != "" ? Visibility.Visible : Visibility.Collapsed;

    public bool IsStationSelected => SelectedStop is not null;

    public Visibility IsOverlayVisible { get; set; } = Visibility.Collapsed;

    public void UpdateRoute()
    {
        for (int i = 0; i < CurrentStops.Count; i++)
        {
            CurrentStops[i].Number = i+1;
            if(i > 0)
            {
                Station previousStation = CurrentStops[i - 1].Station;
                CurrentStops[i].Duration = TimeSpan.FromHours(CurrentStops[i].Station.DistanceTo(previousStation) / 80);
            }
            else
            {
                CurrentStops[i].Duration = TimeSpan.FromMinutes(0);
                CurrentStops[i].Price = 0;
            }
        }
    }

    public bool CanBringUp => IsStationSelected && SelectedStop != CurrentStops.First();
    public bool CanBringDown => IsStationSelected && SelectedStop != CurrentStops.Last();

    public void GeneratePins()
    {
        List<UIElement> childrenToDelete = new();
        foreach (UIElement child in StationMap.Children)
        {
            if (child is Pushpin || child is MapPolyline)
            {
                childrenToDelete.Add(child);
            }
        }

        childrenToDelete.ForEach(c => StationMap.Children.Remove(c));
                
        List<Location> locations = new();
        foreach(var stop in CurrentStops)
        {
            TextBlock tb = new ()
            {
                Text = stop.Number.ToString()
            };
            Pushpin pin = new ()
            {
                Location = new Location(stop.Station.Latitude, stop.Station.Longitude),
                Content = stop.Number.ToString(),
                Template = (ControlTemplate)FindResource("PinTemplate"),
                Background = Brushes.Transparent,
                Height = 70,
                Width = 35,
                ToolTip = stop.Station.Name
            };
            StationMap.Children.Add(pin);
            locations.Add(pin.Location);
        }

        MapPolyline polyline = new ();
        polyline.Locations = new LocationCollection();
        polyline.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0b4596"));

        locations.ForEach(l => polyline.Locations.Add(l));
        polyline.StrokeThickness = 4;
        polyline.Opacity = 0.7;

        StationMap.Children.Add(polyline);
    }


    [ICommand] public void BringUp()
    {
        if (!CanBringUp) return;

        int selected = CurrentStops.IndexOf(SelectedStop);
        int previous = selected - 1;

        (CurrentStops[selected], CurrentStops[previous]) = (CurrentStops[previous], CurrentStops[selected]);
        SelectedStop = CurrentStops[previous];
        CurrentStationsListView.SelectedItem = SelectedStop;

        if (selected == 0 || previous == 0)
        {
            (CurrentStops[selected].Price, CurrentStops[previous].Price) = (CurrentStops[previous].Price, CurrentStops[selected].Price);
        }

        UpdateRoute();
        GeneratePins();

        CurrentStationsListView.Focus();

    }

    [ICommand] public void BringDown()
    {
        if (!CanBringDown) return;

        int selected = CurrentStops.IndexOf(SelectedStop);
        int next = selected + 1;

        (CurrentStops[selected], CurrentStops[next]) = (CurrentStops[next], CurrentStops[selected]);
        SelectedStop = CurrentStops[next];
        CurrentStationsListView.SelectedItem = SelectedStop;

        if (selected == 0 || next == 0)
        {
            (CurrentStops[selected].Price, CurrentStops[next].Price) = (CurrentStops[next].Price, CurrentStops[selected].Price);
        }

        UpdateRoute();
        GeneratePins();

        CurrentStationsListView.Focus();

    }

    [ICommand]
    public void Delete()
    {
        if (SelectedStop is null) return;
        int currentIndex = CurrentStops.IndexOf(SelectedStop);
        CurrentStops.Remove(SelectedStop);

        if (currentIndex == CurrentStops.Count && currentIndex != 0)
        {
            SelectedStop = CurrentStops[currentIndex - 1];
            CurrentStationsListView.SelectedItem = SelectedStop;
        }
        else if (CurrentStops.Count != 0)
        {
            SelectedStop = CurrentStops[currentIndex];
            CurrentStationsListView.SelectedItem = SelectedStop;
        }

        UpdateRoute();
        GeneratePins();

        CurrentStationsListView.Focus();
        CurrentStationsListView.SelectedItem = SelectedStop;

    }

    [ICommand] public void ClearInput() => SearchInput.Text = "";


    private void ListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        Stop SelectedItem = (Stop)CurrentStationsListView.SelectedItem;

        if (SelectedItem != null)
            SelectedStop = SelectedItem;
    }

    private void ListViewLostFocus(object sender, RoutedEventArgs e)
    {
        var element = FocusManager.GetFocusedElement(this);
        if(element is Button or ListView)
        {
            if (element is Button button)
            {
                if (button.Name == "BringUpButton" || button.Name == "BringDownButton" || button.Name == "DeleteButton") return;
            }
            return;
        }
        SelectedStop = null;
        CurrentStationsListView.SelectedItem = null;
    }

    private void SearchTextChanged(object sender, TextChangedEventArgs e)
    {
        List<Station> TempFiltered;
        TempFiltered = AvailableStations.Where(station => station.Name.Contains(SearchInputText, StringComparison.InvariantCultureIgnoreCase)).ToList();

        for (int i = FilteredStations.Count - 1; i >= 0; i--)
        {
            var item = FilteredStations[i];
            if (!TempFiltered.Contains(item))
            {
                FilteredStations.Remove(item);
            }
        }

        foreach(var item in TempFiltered)
        {
            if (!FilteredStations.Contains(item))
            {
                FilteredStations.Add(item);
            }
        }
    }

    private void AddStationToRoute(Station station)
    {
        CurrentStops.Add(new()
        {
            Station = station,
            Price = 0
        });


        UpdateRoute();
        GeneratePins();
    }

    private void StationMouseDown(object sender, MouseEventArgs e)
    {
        StackPanel station = sender as StackPanel;
        if (station != null && e.LeftButton == MouseButtonState.Pressed)
        {
            DragDrop.DoDragDrop(station, station.DataContext, DragDropEffects.Copy);
        }
    }

    private void MapDragDrop(object sender, DragEventArgs e)
    {
        Station station = (Station)e.Data.GetData(typeof(Station));
        AddStationToRoute(station);
        IsOverlayVisible = Visibility.Collapsed;
    }

    private void PreviewMapDropEnter(object sender, DragEventArgs e)
    {
        IsOverlayVisible = Visibility.Visible;
    }

    private void PreviewMapDropLeave(object sender, DragEventArgs e)
    {
        IsOverlayVisible = Visibility.Collapsed;
    }


}
