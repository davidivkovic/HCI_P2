using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using P2.Model;
using P2.Primitives;
using CommunityToolkit.Mvvm.Input;
using P2.Windows;
using System.Windows.Media;

namespace P2.Views
{
    public partial class TimetableView : Component
    {
        public TimetableView()
        {
            using DbContext db = new();

            AvailableLines = db.Lines.Include(l => l.Stops)
                                        .ThenInclude(s => s.Station)
                                     .Include(l => l.Source)
                                     .Include(l => l.Destination)
                                     .ToList();
            foreach (TrainLine line in AvailableLines) { line.UpdateRoute(); }

            FilteredLines = new(AvailableLines);

            AvailableStations = db.Stations.ToList();
            SourceSuggestions = new(AvailableStations.Take(3));
            DestinationSuggestions = new(AvailableStations.Take(3));

            InitializeComponent();
        }

        public List<TrainLine> AvailableLines { get; set; } = new();

        public List<Station> AvailableStations { get; set; } = new();

        public ObservableCollection<TrainLine> FilteredLines { get; set; }

        public ObservableCollection<Departure> Departures { get; set; }

        public ObservableCollection<Station> SourceSuggestions { get; set; }
        public ObservableCollection<Station> DestinationSuggestions { get; set; }

        public TrainLine SelectedTrainLine { get; set; }
        public Departure SelectedDeparture { get; set; }

        public bool IsEditable => SelectedDeparture is not null;

        public bool IsLineSelected => SelectedTrainLine is not null || IsEditable;

        public Visibility IsSourceInputClearable => SourceInputText != null && SourceInputText != "" ? Visibility.Visible : Visibility.Collapsed;
        public Visibility IsDestinationInputClearable => DestinationInputText != null && DestinationInputText != "" ? Visibility.Visible : Visibility.Collapsed;

        public string SourceInputText { get; set; } = "";
        public string DestinationInputText { get; set; } = "";

        public Station SourceSearch { get; set; }
        public Station DestinationSearch { get; set; }

        public void FindTimetable()
        {
            using DbContext db = new();
            Departures = new(db.Departures
                .Where(d => d.Line.Id == SelectedTrainLine.Id)
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

        public void LinesListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TrainLine SelectedItem = (TrainLine)LinesListView.SelectedItem;

            if (SelectedItem != null)
            {
                SelectedTrainLine = SelectedItem;
                FindTimetable();
            }
        }

            //public void LinesListViewLostFocus(object sender, RoutedEventArgs e)
            //{
            //    var element = FocusManager.GetFocusedElement(this);
            //    if (element is Button or ListView) return;

            //    SelectedTrainLine = null;
            //    LinesListView.SelectedItem = null;
            //}

            public void SourceInputLostFocus(object sender, RoutedEventArgs e)
        {
            var element = FocusManager.GetFocusedElement(this);

            if (element is not ListBoxItem && SourceSuggestions.Count != 0)
            {
                SourceSearch = SourceSuggestions[0];
                SourceSuggestionsListBox.SelectedItem = SelectedTrainLine;
                SourceInputText = SourceSearch.Name;
                SourceSuggestionsListBox.Visibility = Visibility.Collapsed;
            }
        }

        public void DestinationInputLostFocus(object sender, RoutedEventArgs e)
        {
            var element = FocusManager.GetFocusedElement(this);

            if (element is not ListBoxItem && DestinationSuggestions.Count != 0)
            {
                DestinationSearch = DestinationSuggestions[0];
                DestinationSuggestionsListBox.SelectedItem = SelectedTrainLine;
                DestinationInputText = DestinationSearch.Name;
                DestinationSuggestionsListBox.Visibility = Visibility.Collapsed;
            }
        }

        //[ICommand]
        //public void ClearSourceInput() => SourceInput.Text = "";

        //[ICommand]
        //public void ClearDestionationInput() => DestinationInput.Text = "";

        public void SourceListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Station SelectedItem = (Station)SourceSuggestionsListBox.SelectedItem;
            if (SelectedItem is not null)
            {
                SourceSearch = SelectedItem;
                SourceInputText = SourceSearch.Name;
                SourceInput.BorderBrush = Brushes.Black;
                SourceSuggestionsListBox.Visibility = Visibility.Collapsed;
            }
        }

        public void DestinationListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Station SelectedItem = (Station)DestinationSuggestionsListBox.SelectedItem;
            if (SelectedItem is not null)
            {
                DestinationSearch = SelectedItem;
                DestinationInputText = DestinationSearch.Name;
                DestinationInput.BorderBrush = Brushes.Black;
                DestinationSuggestionsListBox.Visibility = Visibility.Collapsed;
            }
        }

        public void SourceListBoxKeyDown(object sender, KeyEventArgs e)
        {
            var list = sender as ListBox;
            if (e.Key == Key.Down)
            {
                list.Items.MoveCurrentToNext();
            }
            else if (e.Key == Key.Up)
            {
                list.Items.MoveCurrentToPrevious();
            }
        }

        private void SortDepartures()
        {
            Departures = new(Departures.OrderBy(d => d.Time).ToList());
        }

        private void OnSourceInputTextChanged()
        {
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
            }
            else
            {
                SourceSuggestionsListBox.Visibility = Visibility.Collapsed;
                SourceSuggestionsListBox.ItemsSource = null;
            }

        }
        private void OnDestinationInputTextChanged()
        {
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
            if (SourceSearch is not null || DestinationSearch is not null)
            {
                List<TrainLine> TempFiltered;
                TempFiltered = AvailableLines.Where(line => line.Source.Name == SourceInputText &&
                                                            line.Destination.Name == DestinationInputText)
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
            else
            {
                SourceInput.BorderBrush = Brushes.Red;
                DestinationInput.BorderBrush = Brushes.Red;

                List<string> Errors = new();
                Errors.Add("Polazište ili odredište ne smeju biti prazni");
                var window = new ConfirmCancelWindow()
                {
                    Message = "Nije moguće pretražiti linije zbog sledećih grešaka:",
                    ConfirmButtonText = "U redu",
                    Errors = Errors,
                    Image = MessageBoxImage.Question
                };
                window.ShowDialog();
            }
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
        public void AddNewDeparture()
        {
            var window = new AddEditTimetable(SelectedTrainLine);

            window.ShowDialog();

            if (window.Saved)
            {
                Departures.Add(window.CurrentDeparture);
                SortDepartures();

                var successWindow = new ConfirmCancelWindow
                {
                    Title = "Uspeh",
                    Message = "Vaše izmene su uspešno sačuvane",
                    ConfirmButtonText = "U redu",
                    CancelButtonText = "",
                    Image = MessageBoxImage.Information
                };
                successWindow.ShowDialog();
            }
        }

        [ICommand]
        public void ShowDetails()
        {
            var window = new DepartureDetails(SelectedDeparture);
            window.ShowDialog();
        }

        [ICommand]
        public void DeleteDeparture()
        {
            ConfirmCancelWindow w = new()
            {
                Title = "Brisanje polaska",
                Message = "Da li ste sigurni da želite da obrišete polazak",
                ConfirmIsDanger = true,
                ConfirmButtonText = "Obriši",
                Image = MessageBoxImage.Stop
            };
            w.ShowDialog();

            if (w.Confirmed)
            {
                using DbContext db = new();
                db.Remove(SelectedDeparture);
                db.SaveChanges();

                Departures.Remove(SelectedDeparture);

                new ConfirmCancelWindow()
                {
                    Title = "Uspeh",
                    Message = "Vaše izmene su usešno sačuvane",
                    ConfirmButtonText = "U redu",
                    CancelButtonText = "",
                    Image = MessageBoxImage.Information
                }.ShowDialog();
            }
        }

        [ICommand]
        public void EditDeparture()
        {
            var window = new AddEditTimetable(SelectedTrainLine, SelectedDeparture);
            window.ShowDialog();

            if (window.Saved)
            {

                SortDepartures();
                var successWindow = new ConfirmCancelWindow
                {
                    Title = "Uspeh",
                    Message = "Vaše izmene su uspešno sačuvane",
                    ConfirmButtonText = "U redu",
                    CancelButtonText = "",
                    Image = MessageBoxImage.Information
                };
                successWindow.ShowDialog();
            }
        }
    }
}
