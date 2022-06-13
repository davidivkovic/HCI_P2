using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using P2.Model;
using P2.Primitives;

namespace P2.Windows;

public partial class AddEditTimetable : P2.Primitives.Window
{
    public AddEditTimetable(TrainLine trainLine, Departure departure = null)
    {
        using DbContext db = new();
        Trains = new(db.Trains
            .Include(t => t.Seating)
            .ThenInclude(s => s.Seats)
            .ToList()
        );
        FilteredTrains = new(Trains.Take(3));

        CurrentTrainLine = trainLine;
        CurrentDeparture = departure;

        if (CurrentDeparture is not null)
        {
            int hour = CurrentDeparture.Time.Hour;
            int minutes = CurrentDeparture.Time.Minute;
            SelectedHours = hour < 10 ? $"0{hour}" : hour.ToString();
            SelectedMinutes = minutes < 10 ? $"0{minutes}" : minutes.ToString();
            SelectedTrain = CurrentDeparture.Train;

            HeadingText = "Izmena polaska linije";
            Title = "Izmena polaska linije";
        }
        else
        {
            SelectedHours = "12";
            SelectedMinutes = "00";
            HeadingText = "Dodavanje novog polaska linije";
            Title = "Dodavanje novog polaska linije";
        }

        InitializeComponent();
    }

    public void SetData(TrainLine tl, Departure d = null)
    {
        CurrentTrainLine = tl;
        CurrentDeparture = d;
    }

    public string HeadingText { get; set; }
    public string Filter { get; set; } = string.Empty;
    public Visibility ClearFilterVisible => string.IsNullOrEmpty(Filter) ? Visibility.Collapsed : Visibility.Visible;

    [ICommand]
    public void ClearFilter() => Filter = string.Empty;

    public IEnumerable<string> Hours { get; set; } = Enumerable.Range(0, 24).Select(n => n < 10 ? $"0{n}" : n.ToString());

    public IEnumerable<string> Minutes { get; set; } = Enumerable.Range(0, 60).Select(n => n < 10 ? $"0{n}" : n.ToString());

    public string SelectedHours { get; set; }

    public string SelectedMinutes { get; set; }

    public ObservableCollection<Train> FilteredTrains { get; set; }

    public List<Train> Trains { get; set; } = new();
    
    public Train SelectedTrain { get; set; }

    public TrainLine CurrentTrainLine { get; set; }

    public Departure CurrentDeparture { get; set; }

    public bool IsTrainSelected => SelectedTrain is not null;

    public Visibility AreTrainDetailsVisible => IsTrainSelected ? Visibility.Visible : Visibility.Hidden;

    public string PleaseText => IsTrainSelected ? "" : "Molimo Vas izaberite voz";

    public bool Saved { get; set; }

    public bool Cancelled { get; set; }

    public void OnFilterChanged()
    {
        List<Train> TempFiltered = Trains.Where(t =>
            ("Voz " + t.Number).Contains(Filter, StringComparison.InvariantCultureIgnoreCase) ||
            ("Tip " + t.Name).Contains(Filter, StringComparison.InvariantCultureIgnoreCase) ||
            (t.NumberOfSeats.ToString() + " Sedišta").Contains(Filter, StringComparison.InvariantCultureIgnoreCase) ||
            (t.NumberOfSeats.ToString() + " Sedista").Contains(Filter, StringComparison.InvariantCultureIgnoreCase))
            .ToList();

        for (int i = FilteredTrains.Count - 1; i >= 0; i--)
        {
            var item = FilteredTrains[i];
            if (!TempFiltered.Contains(item))
            {
                FilteredTrains.Remove(item);
            }
        }

        foreach (var item in TempFiltered)
        {
            if (!FilteredTrains.Contains(item))
            {
                FilteredTrains.Add(item);
            }
        }

        FilteredTrains = new(FilteredTrains.Take(3));
    }

    [ICommand] public void SaveChanges()
    {
        List<string> Errors = new();

        if (!IsTrainSelected) Errors.Add("Voz nije izabran");

        var window = new ConfirmCancelWindow()
        {
            Title = Errors.Count > 0 ? "Greška" : "Čuvanje izmena",
            Message = Errors.Count > 0 ? "Nije moguće sačuvati izmene zbog sledećih grešaka:" : "Da li ste sigurni da želite da sačuvate izmene polaska?",
            Errors = Errors,
            ConfirmButtonText = Errors.Count > 0 ? "U redu" : "Sačuvaj izmene",
            Image = Errors.Count > 0 ? MessageBoxImage.Warning : MessageBoxImage.Information
        };
        window.ShowDialog();

        if(Errors.Count == 0 && window.Confirmed)
        {
            TimeOnly time = TimeOnly.Parse($"{SelectedHours}:{SelectedMinutes}:00");
            CurrentDeparture ??= new();
            CurrentDeparture.Train = SelectedTrain;
            CurrentDeparture.Time = time;
            CurrentDeparture.Line = CurrentTrainLine;

            using DbContext db = new();
            db.Update(CurrentDeparture);
            db.SaveChanges();

            Saved = true;
            Close();
        }
    }

    [ICommand] public void Cancel()
    {
        var w = new ConfirmCancelWindow
        {
            Title = "Odustajanje",
            Message = "Da li ste sigurni da želite da odustanete od izmene polaska?",
            ConfirmButtonText = "Odustani od izmena",
            CancelButtonText = "Otkaži",
            ConfirmIsDanger = true,
            Image = MessageBoxImage.Stop
        };
        w.ShowDialog();

        if(w.Confirmed)
        {
            Cancelled = true;
            Close();
        }    
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        if (!Cancelled && !Saved) // Closed by pressing x
        {
            var w = new ConfirmCancelWindow
            {
                Title = "Odustajanje",
                Message = "Da li ste sigurni da želite da odustanete od izmene polaska?",
                ConfirmButtonText = "Odustani od izmena",
                CancelButtonText = "Otkaži",
                ConfirmIsDanger = true,
                Image = MessageBoxImage.Stop
            };
            w.ShowDialog();

            if (!w.Confirmed) e.Cancel = true;
        }
        base.OnClosing(e);
    }

    private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        HelpProvider.ShowHelp("AddEditTimetable", this);
    }
}
