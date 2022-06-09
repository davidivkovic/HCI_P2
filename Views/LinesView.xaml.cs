using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maps.MapControl.WPF;
using P2.Model;
using P2.Primitives;
using P2.Windows;

namespace P2.Views;

/// <summary>
/// Interaction logic for LinesView.xaml
/// </summary>
public partial class LinesView : Component
{
    public LinesView()
    {
        using DbContext db = new();

        AvailableLines = db.Lines
                           .AsNoTrackingWithIdentityResolution()
                           .Include(l => l.Stops)
                               .ThenInclude(s => s.Station)
                           .Include(l => l.Source)
                           .Include(l => l.Destination)
                           .ToList();
        foreach (TrainLine line in AvailableLines) { line.UpdateRoute(); }

        FilteredLines = new(AvailableLines);

        InitializeComponent();
    }

    public List<TrainLine> AvailableLines { get; set; } = new();
    public ObservableCollection<TrainLine> FilteredLines { get; set; }

    public TrainLine SelectedTrainLine { get; set; }

    public bool IsEditable => SelectedTrainLine is not null;

    public Visibility IsInputClearable => SearchInputText != null && SearchInputText != "" ? Visibility.Visible : Visibility.Collapsed;

    public string SearchInputText { get; set; }

    private void ClearMap()
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
    }

    private void UpdateMap()
    {
        ClearMap();

        List<Location> locations = new();
        foreach (var stop in SelectedTrainLine.Stops)
        {
            TextBlock tb = new()
            {
                Text = stop.Number.ToString()
            };
            Pushpin pin = new()
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

        MapPolyline polyline = new();
        polyline.Locations = new LocationCollection();
        polyline.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0b4596"));

        locations.ForEach(l => polyline.Locations.Add(l));
        polyline.StrokeThickness = 4;
        polyline.Opacity = 0.7;

        StationMap.Children.Add(polyline);
    }

    public void LinesListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        TrainLine SelectedItem = (TrainLine)LinesListView.SelectedItem;

        if (SelectedItem != null)
        {
            SelectedTrainLine = SelectedItem;
            UpdateMap();
        }

    }

    public void LinesListViewLostFocus(object sender, RoutedEventArgs e)
    {
        var element = FocusManager.GetFocusedElement(this);
        if (element is Button or ListView) return;
        
        SelectedTrainLine = null;
        LinesListView.SelectedItem = null;
        ClearMap();
    }

    [ICommand] public void DeleteLine()
    {
        var window = new ConfirmCancelWindow
        {
            Title = "Brisanje vozne linije",
            Message = "Da li ste sigurni da želite da obrišete izabranu voznu liniju?",
            ConfirmButtonText = "Obriši",
            ConfirmIsDanger = true
        };
        window.ShowDialog();

        if (window.Confirmed)
        {
            using DbContext db = new();
            db.Remove(SelectedTrainLine);
            db.SaveChanges();

            AvailableLines.Remove(SelectedTrainLine);
            FilteredLines.Remove(SelectedTrainLine);
            SelectedTrainLine = null;

            ClearMap();
        }
    }

    [ICommand] public void EditLine()
    {
        CreateUpdateLine window = new()
        {
            Header = $"Izmena linije {SelectedTrainLine.FormattedLine}"
        };
        window.SetLine(SelectedTrainLine);
        window.ShowDialog();

        if(window.ConfirmedSave)
        {
            using DbContext db = new();
            
            var tl = db.Lines.Include(l => l.Stops)
                            .Include(l => l.Source)
                            .Include(l => l.Destination)
                            .FirstOrDefault(l => l.Id == SelectedTrainLine.Id);

            SelectedTrainLine.Stops.ForEach(s => s.Station = db.Stations.Find(s.Station.Id));
            SelectedTrainLine.Source = db.Stations.Find(SelectedTrainLine.Source.Id);
            SelectedTrainLine.Destination = db.Stations.Find(SelectedTrainLine.Destination.Id);

            tl.Source = SelectedTrainLine.Source;
            tl.Destination = SelectedTrainLine.Destination;
            tl.Stops = SelectedTrainLine.Stops;
            db.SaveChanges();
            
            var successWindow = new ConfirmCancelWindow
            {
                Title = "Uspeh",
                Message = "Vaše izmene su uspešno sačuvane",
                ConfirmButtonText = "U redu",
                CancelButtonText = "",
                Image = MessageBoxImage.Information
            };
            successWindow.ShowDialog();
            UpdateMap();
        }
        LinesListView.Focus();
    }

    [ICommand] public void CreateNewLine()
    {
        CreateUpdateLine window = new()
        {
            Header = "Kreiranje nove linije"
        };
        window.ShowDialog();
        if (window.ConfirmedSave)
        {
            AvailableLines.Add(window.CurrentLine);
            if(SearchInputText is null)
            {
                FilteredLines.Add(window.CurrentLine);
            }
            else
            {
                OnSearchInputTextChanged();
            }
            SelectedTrainLine = window.CurrentLine;
            LinesListView.SelectedItem = SelectedTrainLine;

            using DbContext db = new();
            db.Update(window.CurrentLine);
            db.SaveChanges();

            UpdateMap();
        }
        LinesListView.Focus();
    }

    [ICommand] public void ClearInput() => SearchInput.Text = "";

    private void OnSearchInputTextChanged()
    {
        List<TrainLine> TempFiltered;
        TempFiltered = AvailableLines.Where(line => line.Source.Name.Contains(SearchInputText, StringComparison.InvariantCultureIgnoreCase) ||
                                                    line.Destination.Name.Contains(SearchInputText, StringComparison.InvariantCultureIgnoreCase))
                                      .ToList();

        for (int i = FilteredLines.Count - 1; i >= 0; i--)
        {
            var item = FilteredLines[i];
            if (!TempFiltered.Contains(item))
            {
                FilteredLines.Remove(item);
            }
        }

        foreach (var item in TempFiltered)
        {
            if (!FilteredLines.Contains(item))
            {
                FilteredLines.Add(item);
            }
        }
    }

}
