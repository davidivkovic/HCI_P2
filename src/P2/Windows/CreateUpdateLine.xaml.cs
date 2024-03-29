﻿using System;
using System.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maps.MapControl.WPF;
using P2.Model;
using P2.Primitives;
using P2.Views;
using ThinkSharp.FeatureTouring;
using ThinkSharp.FeatureTouring.Models;
using ThinkSharp.FeatureTouring.Navigation;

namespace P2.Windows;

public partial class CreateUpdateLine : Primitives.Window
{
    public CreateUpdateLine()
    {
        using DbContext db = new();
        db.ChangeTracker.LazyLoadingEnabled = false;

        AvailableStations = db.Stations.ToList();
        FilteredStations = new(AvailableStations);

        UpdateRoute();
        
        InitializeComponent();
        Title = "Dodavanje nove vozne linije";

        GeneratePins();
    }

    public List<Station> AvailableStations { get; set; } = new();

    public string Header { get; set; }

    public TrainLine CurrentLine { get; set; }

    public ObservableCollection<Stop> CurrentStops { get; set; } = new();

    public ObservableCollection<Station> FilteredStations { get; set; }

    public bool ConfirmedSave { get; set; } = false;

    public bool Cancelled { get; set; }

    public Stop SelectedStop { get; set; }

    public string SearchInputText { get; set; }

    public Visibility IsInputClearable => SearchInputText != null && SearchInputText != "" ? Visibility.Visible : Visibility.Collapsed;

    public bool IsStopSelected => SelectedStop is not null;

    public bool IsNotFirstStop => IsStopSelected && SelectedStop.Number != 1;

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

    public bool CanBringUp => IsStopSelected && SelectedStop != CurrentStops.First();
    public bool CanBringDown => IsStopSelected && SelectedStop != CurrentStops.Last();

    public void SetLine(TrainLine line)
    {
        CurrentLine = line;
        CurrentStops = new();
        foreach(Stop stop in CurrentLine.Stops)
        {
            CurrentStops.Add(new()
            {
                Station = stop.Station,
                Price = stop.Price,
                Number = stop.Number
            });
        }
        GeneratePins();
        UpdateRoute();

        NoStopsTextBlock.Visibility = Visibility.Collapsed;
        Title = "Izmena vozne linije";
    }

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
        List<Pushpin> pins = new();
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
                ToolTip = stop.Station.Name,
            };
            pins.Add(pin);
            locations.Add(pin.Location);
        }

        MapPolyline polyline = new ();
        polyline.Locations = new LocationCollection();
        polyline.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0b4596"));

        locations.ForEach(l => polyline.Locations.Add(l));
        polyline.StrokeThickness = 4;
        polyline.Opacity = 0.7;

        StationMap.Children.Add(polyline);
        pins.ForEach(p => StationMap.Children.Add(p));
        
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

        FeatureTour.GetNavigator().IfCurrentStepEquals("Step6").GoNext();
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

        FeatureTour.GetNavigator().IfCurrentStepEquals("Step7").GoNext();
        CurrentStationsListView.Focus();
    }

    [ICommand]
    public void Delete()
    {
        var window = new ConfirmCancelWindow
        {
            Title = "Brisanje stanice iz linije",
            Message = "Da li ste sigurni da želite da obrišete stanicu iz linije?",
            ConfirmButtonText = "Obriši",
            ConfirmIsDanger = true,
            Image = MessageBoxImage.Error
        };
        window.ShowDialog();

        if (!window.Confirmed) return;

        if (SelectedStop is null) return;
        int currentIndex = CurrentStops.IndexOf(SelectedStop);
        CurrentStops.Remove(SelectedStop);

        if (CurrentStops.Count == 0) NoStopsTextBlock.Visibility = Visibility.Visible;

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

    [ICommand]
    public void SaveChanges()
    {
        List<string> Errors = new();
        if(CurrentStops.Count < 2)
        {
            Errors.Add("Linija mora sadržati bar 2 stanice");
        }

        var window = new ConfirmCancelWindow
        {
            Title = Errors.Count > 0 ? "Greška" : "Čuvanje izmena",
            Message = Errors.Count > 0 ? "Nije moguće sačuvati izmene zbog sledećih grešaka:" : "Da li ste sigurni da želite da sačuvate izmene?",
            Errors = Errors,
            ConfirmButtonText = Errors.Count > 0 ? "U redu" : "Sačuvaj izmene",
            ConfirmIsDanger = false,
            Image = Errors.Count > 0 ? MessageBoxImage.Error : MessageBoxImage.Question
        };
        window.ShowDialog();

        if (window.Confirmed && Errors.Count == 0)
        {
            CurrentLine ??= new TrainLine();
            CurrentLine.Stops = new(CurrentStops);
            CurrentLine.Source = CurrentStops.First().Station;
            CurrentLine.Destination = CurrentStops.Last().Station;
            ConfirmedSave = true;
            Close();
        }
    }

    [ICommand] 
    public void DiscardChanges()
    {
        var window = new ConfirmCancelWindow
        {
            Title = "Odustajanje",
            Message = "Da li ste sigurni da želite da odustanete od izmena?",
            ConfirmButtonText = "Odustani od izmena",
            CancelButtonText = "Otkaži",
            ConfirmIsDanger = true,
            Image = MessageBoxImage.Error
        };
        window.ShowDialog();

        if(window.Confirmed)
        {
            Cancelled = true;
            Close();
        }
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        if (!Cancelled && !ConfirmedSave) // Closed by pressing x
        {
            var w = new ConfirmCancelWindow
            {
                Title = "Odustajanje",
                Message = "Da li ste sigurni da želite da odustanete od izmena?",
                ConfirmButtonText = "Odustani od izmena",
                CancelButtonText = "Otkaži",
                ConfirmIsDanger = true,
                Image = MessageBoxImage.Error
            };
            w.ShowDialog();

            if (!w.Confirmed) e.Cancel = true;
        }
        base.OnClosing(e);
    }

    [ICommand]
    public void ChangePrice()
    {
        if (SelectedStop.Number == 1) return;

        var window = new ConfirmCancelWindow
        {
            Title = "Izmena cene",
            ConfirmButtonText = "Sačuvaj",
            CancelButtonText = "Odustani",
            Slot = new EditStopDialog() { Price = SelectedStop.Price.ToString(), StopName = SelectedStop.Station.Name },
            Image = MessageBoxImage.None,
            AutoClose = false,
            OnAction = w =>
            {
                if (w.Confirmed)
                {
                    bool IsValid = double.TryParse(((EditStopDialog)w.Slot).Price, out double newPrice);
                    if (!IsValid)
                    {
                        var warningWindow = new ConfirmCancelWindow
                        {
                            Title = "Neuspela izmena cene",
                            Message = "Nije moguće izmeniti cenu zbog sledećih grešaka:",
                            Errors = new() { "Cena mora biti brojčana vrednost" },
                            ConfirmButtonText = "U redu",
                            ConfirmIsDanger = false,
                            Image = MessageBoxImage.Warning,
                        };
                        warningWindow.ShowDialog();
                    }
                    else
                    {
                        SelectedStop.Price = newPrice;
                        w.Close();
                    }
                }
                else
                {
                    w.Close();
                }
            }
        };
        window.ShowDialog();
        FeatureTour.GetNavigator().IfCurrentStepEquals("Step8").GoNext();
    }

    [ICommand] public void ClearInput() => SearchInput.Text = "";


    private void ListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        Stop SelectedItem = (Stop)CurrentStationsListView.SelectedItem;

        if (SelectedItem != null)
        {
            SelectedStop = SelectedItem;
            FeatureTour.GetNavigator().IfCurrentStepEquals("Step5").GoNext();
        }
    }

    private void ListViewLostFocus(object sender, RoutedEventArgs e)
    {
        var element = FocusManager.GetFocusedElement(this);
        if(element is Button or ListView)
        {
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

        if(FilteredStations.Count == 0)
        {
            ErrorTextBlock.Visibility = Visibility.Visible;
        }
        else
        {
            ErrorTextBlock.Visibility = Visibility.Collapsed;
        }
    }

    private void AddStationToRoute(Station station)
    {
        CurrentStops.Add(new()
        {
            Station = station,
            Price = 0
        });

        NoStopsTextBlock.Visibility = Visibility.Collapsed;

        UpdateRoute();
        GeneratePins();
    }

    private void StationMouseDown(object sender, MouseEventArgs e)
    {
        StackPanel element = sender as StackPanel;
        if (element != null && e.LeftButton == MouseButtonState.Pressed && element.Tag is Station station)
        {
            IsOverlayVisible = Visibility.Visible;
            FeatureTour.GetNavigator().IfCurrentStepEquals("Step1").GoNext();
            FeatureTour.GetNavigator().IfCurrentStepEquals("Step3").GoNext();
            DragDrop.DoDragDrop(element, station, DragDropEffects.Copy);
            FeatureTour.GetNavigator().IfCurrentStepEquals("Step2").GoPrevious();
            FeatureTour.GetNavigator().IfCurrentStepEquals("Step4").GoNext();
            IsOverlayVisible = Visibility.Collapsed;
        }
    }

    private void MapDragDrop(object sender, DragEventArgs e)
    {
        Station station = (Station)e.Data.GetData(typeof(Station));
        AddStationToRoute(station);
        IsOverlayVisible = Visibility.Collapsed;
        FeatureTour.GetNavigator().IfCurrentStepEquals("Step2").GoNext();
        FeatureTour.GetNavigator().IfCurrentStepEquals("Step4").GoNext();
    }

    private void MapDropEnter(object sender, DragEventArgs e)
    {
        Mouse.SetCursor(Cursors.Hand);
        IsOverlayVisible = Visibility.Visible;
    }

    private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        HelpProvider.ShowHelp("CreateUpdateLineHelp", this);
    }

    [ICommand]
    public void StartTutorial()
    {
        TextLocalization.Next = "Dalje";
        TextLocalization.Close = "Završi";
        TextLocalization.DoIt = "Uradi korak za mene";
        var tour = new Tour()
        {
            Name = "Tutorijal za linije",
            ShowNextButtonDefault = true,
            Steps = new[]
            {
                new Step("StationsList", "Header 01", "Kliknite levim tasterom i započnite prevlačenje stanice ka mapi za dodavanje prve stanice", "Step1"),
                new Step("Map", "Header 01", "Spustite stanicu na mapu", "Step2"),
                new Step("StationsList", "Header 01", "Kliknite levim tasterom i započnite prevlačenje stanice ka mapi za dodavanje druge stanice", "Step3"),
                new Step("Map", "Header 01", "Spustite stanicu na mapu", "Step4"),
                new Step("StopsList", "Header 01", "Izaberite neku od stanica levim klikom na nju", "Step5"),
                new Step("MoveUp", "Header 01", "Pomerite izabranu stanicu jedno mesto iznad klikom na dugme \"Pomeri iznad\"", "Step6"),
                new Step("MoveDown", "Header 01", "Pomerite izabranu stanicu jedno mesto ispod klikom na dugme \"Pomeri ispod\"", "Step7"),
                new Step("EditPrice", "Header 01", "Izmenite cenu za izabranu stanicu klikom na dugme desno", "Step8"),
                new Step("ConfirmButton", "Header 01", "Sačuvajte izmene klikom na taster ispod", "Step9")
            }
        };

        tour.Start();
    }
}
