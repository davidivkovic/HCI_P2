using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using P2.Model;
using P2.Primitives;
using P2.Stores;
using P2.Windows;

namespace P2.Views;

public partial class DepartureTicketsView : Component
{
    public DepartureTicketsView()
    {
        using DbContext db = new();
        AvailableStations = db.Stations.ToList();
        AvailableLines = db.Lines.Include(l => l.Stops)
                                       .ThenInclude(s => s.Station)
                                    .Include(l => l.Source)
                                    .Include(l => l.Destination)
                                    .ToList();

        InitializeComponent();
    }

    public List<Station> AvailableStations { get; set; } = new();
    public List<TrainLine> AvailableLines { get; set; } = new();
    public ObservableCollection<TrainLine> FilteredLines { get; set; } = new();
    public ObservableCollection<Station> SourceSuggestions { get; set; } = new();
    public ObservableCollection<Station> DestinationSuggestions { get; set; } = new();
    public ObservableCollection<Departure> Departures { get; set; } = new();
    public Departure SelectedDeparture { get; set; }
    public string SourceInputText { get; set; } = "";
    public string DestinationInputText { get; set; } = "";
    public Station SourceSearch { get; set; }
    public Station DestinationSearch { get; set; }
    public DateTime DateFrom { get; set; } = DateTime.Now;
    public string ErrorText { get; set; } = "Unesite parametre za pretragu";

    public void OnDateFromChanged(DateTime oldDate, DateTime newDate)
    {
        if (oldDate.Date == newDate.Date) return;
        if (TimetableDataGrid is not null)
        {
            TimetableDataGrid.Visibility = Visibility.Collapsed;
        }
        ErrorTextBlock.Visibility = Visibility.Visible;
        ErrorText = "Promenjeni su parametri, pokrenite pretragu";
    }

    public void OnSourceSearchChanged(Station oldStation, Station newStation)
    {
        if (oldStation is null) return;
        if (TimetableDataGrid is not null)
        {
            TimetableDataGrid.Visibility = Visibility.Collapsed;
        }
        ErrorTextBlock.Visibility = Visibility.Visible;
        ErrorText = "Promenjeni su parametri, pokrenite pretragu";
    }

    public void OnDestinationSearchChanged(Station oldStation, Station newStation)
    {
        if (oldStation is null) return;
        if (TimetableDataGrid is not null)
        {
            TimetableDataGrid.Visibility = Visibility.Collapsed;
        }
        ErrorTextBlock.Visibility = Visibility.Visible;
        ErrorText = "Promenjeni su parametri, pokrenite pretragu";
    }

    public void SourceInputGotFocus(object sender, RoutedEventArgs e)
    {
        SourceInputText = "";
        SourceHelpCharsLeft.Text = "2";
        SourceHelpCharsNoun.Text = "karaktera";
        SourceHelpPanel.Visibility = Visibility.Visible;
    }

    public void SourceInputLostFocus(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(SourceInputText) && SourceSuggestions.Count != 0 && SourceInputText.Length >= 2)
        {
            SourceSearch = SourceSuggestions[0];
            SourceSuggestionsListBox.SelectedItem = SourceSearch;
            SourceInputText = SourceSearch.Name;
        }
        else if (SourceSearch is not null)
        {
            SourceInputText = SourceSearch.Name;

        }
        SourceSuggestionsListBox.Visibility = Visibility.Collapsed;
        SourceHelpPanel.Visibility = Visibility.Collapsed;
    }

    public void DestinationInputGotFocus(object sender, RoutedEventArgs e)
    {
        DestinationInputText = "";
        DestinationHelpCharsLeft.Text = "2";
        DestinationHelpCharsNoun.Text = "karaktera";
        DestinationHelpPanel.Visibility = Visibility.Visible;
    }

    public void DestinationInputLostFocus(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(DestinationInputText) && DestinationSuggestions.Count != 0 && DestinationInputText.Length >= 2)
        {
            DestinationSearch = DestinationSuggestions[0];
            DestinationSuggestionsListBox.SelectedItem = DestinationSearch;
            DestinationInputText = DestinationSearch.Name;
        }
        else if (DestinationSearch is not null)
        {
            DestinationInputText = DestinationSearch.Name;

        }
        DestinationSuggestionsListBox.Visibility = Visibility.Collapsed;
        DestinationHelpPanel.Visibility = Visibility.Collapsed;
    }

    public void SourceListBoxKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Down)
        {
            if (SourceSuggestionsListBox.SelectedIndex < SourceSuggestionsListBox.Items.Count - 1)
            {
                SourceSuggestionsListBox.SelectedIndex += 1;
            }
            e.Handled = true;
        }
        else if (e.Key == Key.Up)
        {
            if (SourceSuggestionsListBox.SelectedIndex > 0)
            {
                SourceSuggestionsListBox.SelectedIndex -= 1;
            }
            e.Handled = true;
        }
        else if (e.Key == Key.Enter || e.Key == Key.Tab)
        {
            if (SourceSuggestionsListBox.SelectedIndex > -1)
            {
                SourceSearch = SourceSuggestions.ElementAt(SourceSuggestionsListBox.SelectedIndex);
                SourceInputText = SourceSearch.Name;
                SourceSuggestionsListBox.Visibility = Visibility.Collapsed;
                DestinationInput.Focus();
            }
            e.Handled = true;
        }
    }

    public void DestinationListBoxKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Down)
        {
            if (DestinationSuggestionsListBox.SelectedIndex < DestinationSuggestionsListBox.Items.Count - 1)
            {
                DestinationSuggestionsListBox.SelectedIndex += 1;
            }
            e.Handled = true;
        }
        else if (e.Key == Key.Up)
        {
            if (DestinationSuggestionsListBox.SelectedIndex > 0)
            {
                DestinationSuggestionsListBox.SelectedIndex -= 1;
            }
            e.Handled = true;
        }
        else if (e.Key == Key.Enter || e.Key == Key.Tab)
        {
            if (DestinationSuggestionsListBox.SelectedIndex > -1)
            {
                DestinationSearch = DestinationSuggestions.ElementAt(DestinationSuggestionsListBox.SelectedIndex);
                DestinationInputText = DestinationSearch.Name;
                DestinationSuggestionsListBox.Visibility = Visibility.Collapsed;
                gDPickDate.IsDropDownOpen = true;
            }
            e.Handled = true;
        }

    }

    private void OnSourceInputTextChanged()
    {
        DestinationInput.BorderBrush = Brushes.DimGray;

        if (SourceInputText.Length < 2)
        {
            var charsLeft = 2 - SourceInputText.Length;
            SourceHelpCharsLeft.Text = charsLeft.ToString();
            SourceHelpCharsNoun.Text = charsLeft == 2 ? "karaktera" : "karakter";
            SourceSuggestionsListBox.Visibility = Visibility.Collapsed;
            SourceHelpPanel.Visibility = Visibility.Visible;
            return;
        }

        SourceHelpPanel.Visibility = Visibility.Collapsed;

        List<Station> TempFiltered;
        TempFiltered = AvailableStations.Where(s => s.Name.Contains(SourceInputText, StringComparison.InvariantCultureIgnoreCase))
                                     .ToList();

        for (int i = SourceSuggestions.Count - 1; i >= 0; i--)
        {
            var item = SourceSuggestions[i];
            if (!TempFiltered.Contains(item))
            {
                SourceSuggestions.Remove(item);
            }
        }

        foreach (var item in TempFiltered)
        {
            if (!SourceSuggestions.Contains(item))
            {
                SourceSuggestions.Add(item);
            }
        }

        SourceSuggestions = new(SourceSuggestions.Take(3));

        if (SourceSuggestions.Count > 0)
        {
            SourceSuggestionsListBox.ItemsSource = SourceSuggestions;
            SourceSuggestionsListBox.Visibility = Visibility.Visible;
            SourceSuggestionsListBox.SelectedIndex = 0;
        }
        else
        {
            SourceSuggestionsListBox.Visibility = Visibility.Collapsed;
            SourceSuggestionsListBox.ItemsSource = null;
        }

    }
    private void OnDestinationInputTextChanged()
    {
        DestinationInput.BorderBrush = Brushes.DimGray;

        if (DestinationInputText.Length < 2)
        {
            var charsLeft = 2 - DestinationInputText.Length;
            DestinationHelpCharsLeft.Text = charsLeft.ToString();
            DestinationHelpCharsNoun.Text = charsLeft == 2 ? "karaktera" : "karakter";
            DestinationSuggestionsListBox.Visibility = Visibility.Collapsed;
            DestinationHelpPanel.Visibility = Visibility.Visible;
            return;
        }

        DestinationHelpPanel.Visibility = Visibility.Collapsed;

        List<Station> TempFiltered;
        TempFiltered = AvailableStations.Where(s => s.Name.Contains(DestinationInputText, StringComparison.InvariantCultureIgnoreCase))
                                     .ToList();

        for (int i = DestinationSuggestions.Count - 1; i >= 0; i--)
        {
            var item = DestinationSuggestions[i];
            if (!TempFiltered.Contains(item))
            {
                DestinationSuggestions.Remove(item);
            }
        }

        foreach (var item in TempFiltered)
        {
            if (!DestinationSuggestions.Contains(item))
            {
                DestinationSuggestions.Add(item);
            }
        }

        DestinationSuggestions = new(DestinationSuggestions.Take(3));

        if (SourceSuggestions.Count > 0)
        {
            DestinationSuggestionsListBox.ItemsSource = DestinationSuggestions;
            DestinationSuggestionsListBox.Visibility = Visibility.Visible;
            DestinationSuggestionsListBox.SelectedIndex = 0;
        }
        else
        {
            DestinationSuggestionsListBox.Visibility = Visibility.Collapsed;
            DestinationSuggestionsListBox.ItemsSource = null;
        }
    }

    [ICommand]
    public void SearchLines()
    {

        if (SourceSearch is null || DestinationSearch is null)
        {
            List<string> errors = new();
            errors.Add("Polazište i odredište moraju biti odabrani");
        }
        else
        {
            Search();
        }
    }

    public void SearchTimetable()
    {
        using DbContext db = new();
        List<Departure> allDepartures = new();
        foreach (var line in FilteredLines)
        {
            allDepartures.AddRange(db.Departures
            .Where(d => d.Line.Id == line.Id)
            .Include(d => d.Train)
                .ThenInclude(t => t.Seating)
                    .ThenInclude(s => s.Seats)
            .Include(d => d.Line)
                .ThenInclude(l => l.Stops)
                    .ThenInclude(s => s.Station)
             .Include(d => d.Line)
                .ThenInclude(l => l.Source)
            .Include(d => d.Line)
                .ThenInclude(l => l.Destination)
            .OrderBy(d => d.Time)
            .ToList());
        }

        allDepartures = allDepartures.OrderBy(d => d.Time).ToList();
        Departures = new(allDepartures);

    }

    public void Search()
    {
        List<TrainLine> TempFiltered;
        TempFiltered = AvailableLines.Where(line => line.Stops.Any(s => s.Station.Name == SourceInputText))
                                     .Where(line => line.Stops.Any(s => s.Station.Name == DestinationInputText))
                                     .ToList();

        List<TrainLine> toDelete = new();
        foreach (var line in TempFiltered)
        {
            int startNum = line.Stops.Where(s => s.Station.Name == SourceInputText).Select(stop => stop.Number).FirstOrDefault();
            int endNum = line.Stops.Where(s => s.Station.Name == DestinationInputText).Select(stop => stop.Number).FirstOrDefault();
            if (startNum > endNum) toDelete.Add(line);
        }

        toDelete.ForEach(l => TempFiltered.Remove(l));

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

        TimetableDataGrid.Visibility = Visibility.Visible;
        ErrorTextBlock.Visibility = Visibility.Collapsed;

        SearchTimetable();
    }

    public void TimetableGridSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        Departure SelectedItem = (Departure)TimetableDataGrid.SelectedItem;

        if (SelectedItem != null)
        {
            SelectedDeparture = SelectedItem;
        }
    }

    [ICommand]
    public void ShowTickets()
    {
        var window = new DepartureTickets(SelectedDeparture, DateFrom);
        window.ShowDialog();

    }

    private void DatePickerMouseLeftButtonUp(object sender, RoutedEventArgs e)
    {
        gDPickDate.IsDropDownOpen = true;
    }

    private void DatePickerCalendarClosed(object sender, RoutedEventArgs e)
    {
        SearchButton.Focus();
    }




}
