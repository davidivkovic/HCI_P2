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

namespace P2.Views
{
    public partial class TimetableView : Component
    {
        public TimetableView()
        {
            using DbContext db = new();

            AvailableLines = db.Lines.Include(l => l.Stops)
                                     .Include(l => l.Source)
                                     .Include(l => l.Destination)
                                     .ToList();
            foreach (TrainLine line in AvailableLines) { line.UpdateRoute(); }

            FilteredLines = new(AvailableLines);

            InitializeComponent();
        }

        public List<TrainLine> AvailableLines { get; set; } = new();

        public ObservableCollection<TrainLine> FilteredLines { get; set; }

        public ObservableCollection<Departure> Departures { get; set; }

        public TrainLine SelectedTrainLine { get; set; }

        public bool IsEditable => SelectedTrainLine is not null;

        public Visibility IsInputClearable => SearchInputText != null && SearchInputText != "" ? Visibility.Visible : Visibility.Collapsed;

        public string SearchInputText { get; set; }

        public void FindTimetable()
        {
            using DbContext db = new();
            Departures = new(db.Departures
                .Where(d => d.Line.Id == SelectedTrainLine.Id)
                .Include(d => d.Train)
                .Include(d => d.Line)
                    .ThenInclude(l => l.Stops)
                    .Include(d => d.Line)
                    .ThenInclude(l => l.Source)
                .Include(d => d.Line)
                    .ThenInclude(l => l.Destination)
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

        public void LinesListViewLostFocus(object sender, RoutedEventArgs e)
        {
            var element = FocusManager.GetFocusedElement(this);
            if (element is Button or ListView) return;

            SelectedTrainLine = null;
            LinesListView.SelectedItem = null;
        }

        [ICommand]
        public void ClearInput() => SearchInput.Text = "";

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
}
