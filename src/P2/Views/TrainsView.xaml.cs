﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using P2.Model;
using P2.Primitives;
using P2.Windows;

namespace P2.Views;

public partial class TrainsView : Component
{
    public TrainsView()
    {
        InitializeComponent();
        using DbContext db = new();
        Trains = new(db.Trains
            .Where(t => !t.IsDeleted)
            .Include(t => t.Seating)
            .ThenInclude(s => s.Seats)
            .ToList()
        );
        FilteredTrains = new(Trains);

        if (FilteredTrains.Count == 0) ErrorTextBlock.Visibility = Visibility.Visible;
    }

    public List<Train> Trains { get; set; }
    public ObservableCollection<Train> FilteredTrains { get; set; }
    public Train SelectedTrain { get; set; }
    public string Filter { get; set; } = string.Empty;
    public Visibility ClearFilterVisible => string.IsNullOrEmpty(Filter) ? Visibility.Collapsed : Visibility.Visible;
    public List<List<Slot>> Seats => GetSeats();
    public bool IsEditable => SelectedTrain is not null;
    public string PleaseText => IsEditable ? "" : "Molimo Vas izaberite voz";
    public Visibility DetailsVisible => SelectedTrain is null ? Visibility.Collapsed : Visibility.Visible;

    public List<List<Slot>> GetSeats()
    {
        if (SelectedTrain is null) return new();

        var slots = Enumerable.Range(0, 48)
        .Select(index => new Slot
        {
            Row = index / 4,
            Col = index % 4
        })
        .Chunk(4)
        .ToDictionary(sa => sa[0]?.Row, sa => sa.ToList());

        SelectedTrain.Seating
            .ForEach(sg =>
                sg.Seats.ForEach(s =>
                {
                    slots[s.Row][s.Col].SeatNumber = s.SeatNumber;
                    slots[s.Row][s.Col].SeatType = s.SeatType;
                    slots[s.Row][s.Col].Id = sg.Id;
                })
            );

        return slots.Select(s => s.Value).ToList();
    }

    private void ListViewLostFocus(object sender, RoutedEventArgs e)
    {
        //var element = FocusManager.GetFocusedElement(this);
        //if (element is Button or ListView)
        //{
        //    return;
        //}
        //SelectedTrain = null;
        //TrainsListView.SelectedItem = null;
    }

    public void SearchTextBoxKeyDown(object sender, KeyEventArgs e)
    {
        if(e.Key == Key.Enter || e.Key == Key.Tab || e.Key == Key.Down)
        {
            if(FilteredTrains.Count > 0)
            {
                TrainsListView.Focus();
                TrainsListView.SelectedIndex = 0;
            }
        }
    }

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

        if (FilteredTrains.Count == 0)
        {
            ErrorTextBlock.Visibility = Visibility.Visible;
        }
        else
        {
            ErrorTextBlock.Visibility = Visibility.Collapsed;
        }
    }

    [ICommand] 
    public void ClearFilter() => Filter = string.Empty;

    [ICommand]
    public void AddTrain()
    {
        AddEditTrain w = new();
        w.ShowDialog();
        if (w.Confirmed)
        {
            Trains.Add(w.CurrentTrain);
            OnFilterChanged();
            new ConfirmCancelWindow()
            {
                Title = "Uspeh",
                Message = "Vaše izmene su uspešno sačuvane",
                ConfirmButtonText = "U redu",
                CancelButtonText = "",
                Image = MessageBoxImage.Information
            }.ShowDialog();

            if (FilteredTrains.Count == 0)
            {
                ErrorTextBlock.Visibility = Visibility.Visible;
            }
            else
            {
                ErrorTextBlock.Visibility = Visibility.Collapsed;
            }
        }
    }

    [ICommand]
    public void DeleteTrain()
    {
        if (!IsEditable) return;

        ConfirmCancelWindow w = new()
        {
            Title = "Brisanje voza",
            Message = "Da li ste sigurni da želite da obrišete voz " + SelectedTrain.Number,
            ConfirmIsDanger = true,
            ConfirmButtonText = "Obriši",
            Image = MessageBoxImage.Stop
        };
        w.ShowDialog();

        if (w.Confirmed)
        {
            using DbContext db = new();
            SelectedTrain.IsDeleted = true;
            db.Update(SelectedTrain);
            db.SaveChanges();

            Trains.Remove(SelectedTrain);
            SelectedTrain = null;
            OnFilterChanged();

            if (FilteredTrains.Count == 0)
            {
                ErrorTextBlock.Visibility = Visibility.Visible;
            }
            else
            {
                ErrorTextBlock.Visibility = Visibility.Collapsed;
            }

            new ConfirmCancelWindow()
            {
                Title = "Uspeh",
                Message = "Vaše izmene su uspešno sačuvane",
                ConfirmButtonText = "U redu",
                CancelButtonText = "",
                Image = MessageBoxImage.Information
            }.ShowDialog();
        }
    }

    [ICommand]
    public void EditTrain()
    {
        if (!IsEditable) return;

        AddEditTrain w = new(SelectedTrain, Seats);
        w.ShowDialog();
        if (w.Confirmed)
        {
            int idx = FilteredTrains.IndexOf(SelectedTrain);
            FilteredTrains.Remove(SelectedTrain);
            SelectedTrain = w.CurrentTrain;
            FilteredTrains.Insert(idx, SelectedTrain);
            OnFilterChanged();
            new ConfirmCancelWindow()
            {
                Title = "Uspeh",
                Message = "Vaše izmene su uspešno sačuvane",
                ConfirmButtonText = "U redu",
                CancelButtonText = "",
                Image = MessageBoxImage.Information
            }.ShowDialog();
        }
    }
}