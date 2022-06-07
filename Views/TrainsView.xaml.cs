using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
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
            .Include(t => t.Seating)
            .ThenInclude(s => s.Seats)
            .ToList()
        );
        FilteredTrains = new(Trains);
    }

    public List<Train> Trains { get; set; }
    public ObservableCollection<Train> FilteredTrains { get; set; }
    public Train SelectedTrain { get; set; }
    public string Filter { get; set; }
    public Visibility ClearFilterVisible => string.IsNullOrEmpty(Filter) ? Visibility.Collapsed : Visibility.Visible;
    public List<Slot[]> Seats => GetSeats();

    public List<Slot[]> GetSeats()
    {
        if (SelectedTrain is null) return new();

        var slots = Enumerable.Range(0, 48)
        .Select(index => new Slot
        {
            Row = index / 4,
            Col = index % 4
        })
        .Chunk(4)
        .ToDictionary(sa => sa[0]?.Row, sa => sa);

        SelectedTrain.Seating
            .Select(s => s.Seats)
            .SelectMany(s => s)
            .ToList()
            .ForEach(s => 
            {
                slots[s.Row][s.Col].SeatNumber = s.SeatNumber;
                slots[s.Row][s.Col].SeatType = s.SeatType;
            });

         return slots.Select(s => s.Value).ToList();
    }

    public void OnFilterChanged()
    {
        List<Train> TempFiltered = Trains.Where(t => t.Number.Contains(Filter, StringComparison.InvariantCultureIgnoreCase)).ToList();

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
    }

    [ICommand] 
    public void ClearFilter() => Filter = string.Empty;

    [ICommand]
    public void DeleteTrain()
    {

    }

    [ICommand]
    public void EditTrain()
    {

    }
}