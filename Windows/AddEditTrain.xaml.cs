using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using P2.Extensions;
using P2.Model;
using P2.Primitives;
using P2.Views;

namespace P2.Windows;

public class Slot : Observable
{
    public int Row { get; set; }
    public int Col { get; set; }
    public int SeatNumber { get; set; }
    public SeatType PreviewSeatType { get; set; }
    public SeatType SeatType { get; set; }
    public bool Assigned => SeatType != SeatType.None;
    public string FormattedSeatNumber => SeatType == SeatType.None ? "" : SeatNumber.ToString();
    public string Cursor => SeatType == SeatType.None ? "Arrow" : "/Assets/Cursors/grab.cur";
    public Brush GetBackground()
    {
        if (PreviewSeatType != SeatType.None) return PreviewSeatType switch
        {
            SeatType.Single => "#cee4f5".ToBrush(),
            SeatType.Double => "#e2ffab".ToBrush(),
            SeatType.DoubleTable => "#ffda96".ToBrush(),
            _ or SeatType.None or SeatType.Override => "#dbdbdb".ToBrush()
        };

        return SeatType switch
        {
            SeatType.Single => Brushes.LightSkyBlue,
            SeatType.Double => Brushes.LawnGreen,
            SeatType.DoubleTable => Brushes.Orange,
            _ or SeatType.None or SeatType.Override => "#dbdbdb".ToBrush()
        };
    }

    public Brush Background => GetBackground();
}

public partial class AddEditTrain : Primitives.Window
{
    public ObservableCollection<Slot[]> Slots { get; set; }
    public List<IEnumerable<Slot>> TakenSlots { get; set; } = new();

    public bool CanDeleteSeats { get; set; }
    public Brush TrashForeground { get; set; } = Brushes.Gray;
    public Brush TrashBorder { get; set; } = Brushes.DimGray;
    public string TrashImage { get; set; } = "/Assets/Images/trashcan.png";
    public double TrashOpacity { get; set; } = 0.6;

    public AddEditTrain()
    {
        InitializeComponent();
        Slots = new(
            Enumerable.Range(0, 48)
            .Select(index => new Slot
            {
                Row = index / 4,
                Col = index % 4
            })
            .Chunk(4)
        );
    }

    private void ReorderSeatNumbers()
    {
        int i = 1;
        foreach (var s in Slots.SelectMany(s => s))
        {
            if (s.SeatType != SeatType.None && s.SeatType != SeatType.Override) s.SeatNumber = i++;
        }
    }

    private IEnumerable<Slot> GetOverlappingSlots(Slot slot, SeatType seatType, bool greedy = true)
    {
        var connntectedSlots = GetConnectedSlots(slot, seatType, greedy);
        return TakenSlots.Where(slots =>
            slots.Where(s => connntectedSlots.Any(cs => s.Row == cs.Row && s.Col == cs.Col))
                 .Any()
        ).SelectMany(s => s);
    }

    private IEnumerable<Slot> GetConnectedSlots(Slot slot, SeatType seatType, bool greedy = true)
    {
        if (seatType == SeatType.Double)
        {
            int offset = slot.Col % 2 == 0 ? 1 : -1;
            return Slots
                .SelectMany(s => s)
                .Where(s =>
                    s.Row == slot.Row &&
                    (s.Col == slot.Col || s.Col == slot.Col + offset)
                );
        }
        else if (seatType == SeatType.DoubleTable)
        {
            int colOffset = slot.Col % 2 == 0 ? 1 : -1;
            int rowOffset = slot.Row + 1 == Slots.Count ? -1 : 1;
            if (!greedy) rowOffset = 0;
            var connectedSlots = Slots
                .SelectMany(s => s)
                .Where(s =>
                    (s.Row == slot.Row ||
                     s.Row == slot.Row + rowOffset) &&
                    (s.Col == slot.Col + colOffset || s.Col == slot.Col)
                );
            return connectedSlots;
        }
        return new[] { slot };
    }

    private IEnumerable<Slot> ExecuteOnSlots(Slot slot, SeatType seatType, Action<Slot> action, IEnumerable<Slot> connectedSlots = null)
    {
        if (seatType == SeatType.Single)
        {
            action(slot);
            return new[] { slot };
        }
        else
        {
            connectedSlots ??= GetConnectedSlots(slot, seatType);
            foreach (var s in connectedSlots)
            {
                action(s);
            }
            return connectedSlots;
        }
    }

    private void SeatDrag(object sender, MouseEventArgs e)
    {
        if (sender is Border element && e.LeftButton == MouseButtonState.Pressed)
        {
            if (element.Tag is string type)
            {
                SeatType seatType = Enum.Parse<SeatType>(type);
                DragDrop.DoDragDrop(element, seatType, DragDropEffects.Copy);
            }
            else if (element.Tag is Slot slot && slot.SeatType != SeatType.None)
            {
                DragDrop.DoDragDrop(element, element.Tag, DragDropEffects.Move);
            }
            Slots.SelectMany(s => s).ToList().ForEach(s => s.PreviewSeatType = SeatType.None);
        }
    }

    private void SeatDragLeave(object sender, DragEventArgs e)
    {
        var element = sender as Border;
        var slot = (Slot)element.Tag;
        SeatType seatType;
        Slot sourceSlot = null;

        if (e.Effects == DragDropEffects.Copy)
        {
            seatType = (SeatType)e.Data.GetData(typeof(SeatType));
        }
        else if (e.Effects == DragDropEffects.Move)
        {
            seatType = ((Slot)e.Data.GetData(typeof(Slot))).SeatType;
        }
        else return;

        var overlappingSlots = GetOverlappingSlots(slot, seatType);

        if (overlappingSlots is not null)
        {
            ExecuteOnSlots(
                slot,
                slot.SeatType,
                s => s.PreviewSeatType = SeatType.None,
                overlappingSlots
            );
        }

        ExecuteOnSlots(slot, seatType, s =>
        {
            s.PreviewSeatType = SeatType.None;
        });

        if (e.Effects == DragDropEffects.Move)
        {
            sourceSlot = e.Data.GetData(typeof(Slot)) as Slot;
            var overlapingSourceSlots = GetOverlappingSlots(sourceSlot, sourceSlot.SeatType, sourceSlot.SeatType != SeatType.DoubleTable);
            ExecuteOnSlots(
                sourceSlot,
                sourceSlot.SeatType,
                s => s.PreviewSeatType = sourceSlot.SeatType,
                overlapingSourceSlots
            );
        }
        ReorderSeatNumbers();
    }

    private void SeatDragEnter(object sender, DragEventArgs e)
    {
        var element = sender as Border;
        var slot = (Slot)element.Tag;
        SeatType seatType;
        Slot sourceSlot = null;
        if (e.Effects == DragDropEffects.Copy)
        {
            seatType = (SeatType)e.Data.GetData(typeof(SeatType));
        }
        else if (e.Effects == DragDropEffects.Move)
        {
            seatType = ((Slot)e.Data.GetData(typeof(Slot))).SeatType;
            sourceSlot = e.Data.GetData(typeof(Slot)) as Slot;
        }
        else return;

        var overlappingSlots = GetOverlappingSlots(slot, seatType);

        if (overlappingSlots is not null)
        {
            //if (overlappingSlots.Count() == 4 && overlappingSlots.Last().Row == sourceSlot?.Row) return;
            ExecuteOnSlots(
                slot,
                slot.SeatType,
                s => s.PreviewSeatType = SeatType.Override,
                overlappingSlots
            );
        }

        ExecuteOnSlots(slot, seatType, s =>
        {
            s.PreviewSeatType = seatType;
        });
        ReorderSeatNumbers();
    }

    private void SeatDrop(object sender, DragEventArgs e)
    {
        var element = sender as Border;
        var slot = (Slot)element.Tag;
        SeatType seatType;
        Slot sourceSlot = null;
        if (e.Effects == DragDropEffects.Copy)
        {
            seatType = (SeatType)e.Data.GetData(typeof(SeatType));
        }
        else if (e.Effects == DragDropEffects.Move)
        {
            seatType = ((Slot)e.Data.GetData(typeof(Slot))).SeatType;
            sourceSlot = e.Data.GetData(typeof(Slot)) as Slot;

            if (sourceSlot.SeatType == slot.SeatType) return;

            var overlapingSourceSlots = GetOverlappingSlots(sourceSlot, sourceSlot.SeatType, sourceSlot.SeatType != SeatType.DoubleTable);

            var sourceSlotsToRemove = ExecuteOnSlots(
                sourceSlot,
                sourceSlot.SeatType,
                s => s.PreviewSeatType = s.SeatType = SeatType.None,
                overlapingSourceSlots
            );
            if (sourceSlotsToRemove.Any())
            {
                TakenSlots = TakenSlots.Where(slots =>
                    !slots.Any(s => s.Row == sourceSlotsToRemove.First().Row && s.Col == sourceSlotsToRemove.First().Col)
                ).ToList();
            }
        }
        else return;

        var overlappingSlots = GetOverlappingSlots(slot, seatType);

        if (overlappingSlots is not null)
        {
            //if (overlappingSlots.Count() == 4 && overlappingSlots.Last().Row == sourceSlot?.Row) return;
            var slotsToRemove = ExecuteOnSlots(
                slot,
                slot.SeatType,
                s => s.PreviewSeatType = s.SeatType = SeatType.None,
                overlappingSlots
            );

            if (slotsToRemove.Any())
            {
                TakenSlots = TakenSlots.Where(slots =>
                    !slots.Any(s => s.Row == slotsToRemove.First().Row && s.Col == slotsToRemove.First().Col)
                ).ToList();
            }
        }

        var connectedSlots = ExecuteOnSlots(
            slot,
            seatType,
            s =>
            {
                s.PreviewSeatType = SeatType.None;
                s.SeatType = seatType;
            }
        );
        TakenSlots.Add(connectedSlots);
        CanDeleteSeats = TakenSlots.Count > 0;
        ReorderSeatNumbers();
    }

    private void EnterTrash(object sender, DragEventArgs e)
    {
        if (e.Effects == DragDropEffects.Move)
        {
            TrashForeground = TrashBorder = Brushes.Black;
            TrashImage = "/Assets/Images/trashcan_open.png";
            TrashOpacity = 1;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
    }

    private void LeaveTrash(object sender, DragEventArgs e)
    {
        if (e.Effects == DragDropEffects.Move)
        {
            TrashForeground = Brushes.Gray;
            TrashBorder = Brushes.DarkGray;
            TrashImage = "/Assets/Images/trashcan.png";
            TrashOpacity = 0.6;
        }
    }

    private void TrashOver(object sender, DragEventArgs e) => e.Effects = DragDropEffects.None;

    private void DeleteSeats(object sender, DragEventArgs e)
    {
        if (e.Effects == DragDropEffects.Move)
        {
            SeatType seatType = ((Slot)e.Data.GetData(typeof(Slot))).SeatType;
            var sourceSlot = e.Data.GetData(typeof(Slot)) as Slot;

            var overlapingSourceSlots = GetOverlappingSlots(sourceSlot, sourceSlot.SeatType);
            var sourceSlotsToRemove = ExecuteOnSlots(
                sourceSlot,
                sourceSlot.SeatType,
                s => s.PreviewSeatType = s.SeatType = SeatType.None,
                overlapingSourceSlots
            );
            if (sourceSlotsToRemove.Any())
            {
                TakenSlots = TakenSlots.Where(slots =>
                    !slots.Any(s => s.Row == sourceSlotsToRemove.First().Row && s.Col == sourceSlotsToRemove.First().Col)
                ).ToList();
            }
            ReorderSeatNumbers();
        }
        CanDeleteSeats = TakenSlots.Count > 0;
        LeaveTrash(sender, e);
    }

    [ICommand] public void DeleteAllSeats()
    {
        var window = new ConfirmCancelWindow
        {
            Title = "Uklanjanje svih sedišta",
            Message = "Da li ste sigurni da želite da uklonite sva sedišta iz voza?",
            ConfirmButtonText = "Ukloni",
            ConfirmIsDanger = true
        };
        window.ShowDialog();

        if (window.Confirmed)
        {
            Slots.SelectMany(s => s).ToList().ForEach(s => s.SeatType = SeatType.None);
            TakenSlots.Clear();
            CanDeleteSeats = false;
        }
    }
}