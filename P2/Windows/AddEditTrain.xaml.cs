using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using P2.Extensions;
using P2.Model;
using P2.Primitives;
using ThinkSharp.FeatureTouring;
using ThinkSharp.FeatureTouring.Models;
using ThinkSharp.FeatureTouring.Navigation;

namespace P2.Windows;

public record class TrainViewModel(
    string Name,
    TrainType Type,
    string Image
);

public class Slot : Observable
{
    public int Id { get; set; }
    public int Row { get; set; }
    public int Col { get; set; }
    public int SeatNumber { get; set; }
    public SeatType PreviewSeatType { get; set; }
    public SeatType SeatType { get; set; }
    public bool Assigned => SeatType != SeatType.None;
    public string FormattedSeatNumber => SeatType == SeatType.None ? "" : SeatNumber.ToString();
    public string Cursor => SeatType == SeatType.None ? "Arrow" : "Hand";
    public string PurchaseCursor => SeatType == SeatType.None || PreviewSeatType == SeatType.Taken ? "Arrow" : "Hand";
    public Brush GetBackground()
    {
        if (PreviewSeatType != SeatType.None) return PreviewSeatType switch
        {
            SeatType.Single => "#cee4f5".ToBrush(),
            SeatType.Double => "#e2ffab".ToBrush(),
            SeatType.DoubleTable => "#ffda96".ToBrush(),
            SeatType.Taken => Brushes.DimGray,
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
    public ObservableCollection<List<Slot>> Slots { get; set; }
    public List<IEnumerable<Slot>> TakenSlots { get; set; } = new();

    public static List<TrainViewModel> TrainTypes { get; set; } = new() 
    {
        new ("Soko", TrainType.Falcon, "/Assets/Images/soko.png"),
        new ("Regio", TrainType.Regio, "/Assets/Images/regio.jpg"),
        new ("Inter City", TrainType.InterCity, "/Assets/Images/intercity.jpg")
    };

    public Train CurrentTrain { get; set; }
    public TrainViewModel SelectedTrainType { get; set; } = TrainTypes.First();
    public string HeaderText => CurrentTrain is null ? "Dodavanje novog voza" : "Izmena voza " + CurrentTrain.Number;
    public string TrainNumber { get; set; }
    public bool AllowTrainNumberEdit { get; set; }
    public string TrainNumberEditText => AllowTrainNumberEdit ? "Gotovo" : "Izmeni";
    public bool CanDeleteSeats { get; set; }
    public Brush TrashForeground { get; set; } = Brushes.Gray;
    public Brush TrashBorder { get; set; } = Brushes.DimGray;
    public string TrashImage { get; set; } = "/Assets/Images/trashcan.png";
    public double TrashOpacity { get; set; } = 0.6;
    public bool Confirmed { get; set; }
    public bool Cancelled { get; set; }

    public void OnTrainNumberChanged() => TrainNumberTextBox.ClearValue(BorderBrushProperty);

    public AddEditTrain(Train train, List<List<Slot>> slots)
    {
        InitializeComponent();
        InitializeFocus();
        CurrentTrain = train;
        Slots = new(slots);
        TakenSlots = new(
             Slots.SelectMany(s => s)
             .Where(s => s.Id != 0)
             .GroupBy(
             s => s.Id,
             s => s,
             (key, g) => new
             {
                 Slots = g
             })
             .Select(s => s.Slots)
             .ToList()
        );

        SelectedTrainType = TrainTypes.Find(t => t.Type == train.Type);
        TrainNumber = train.Number;
        CanDeleteSeats = TakenSlots.Count > 0;
        Title = "Izmena voza";
    }

    public AddEditTrain()
    {
        InitializeComponent();
        InitializeFocus();
        Slots = new(
            Enumerable.Range(0, 48)
            .Select(index => new Slot
            {
                Row = index / 4,
                Col = index % 4
            })
            .Chunk(4)
            .Select(c => c.ToList())
        );
        Title = "Dodavanje novog voza";
    }

    public void InitializeFocus()
    {
        MouseDown += (s, e) =>
        {
            if (s is not TextBox)
            {
                AllowTrainNumberEdit = false;
            }
        };
        TrainNumberTextBox.LostKeyboardFocus += (s, e) =>
        {
            if (e.NewFocus != TrainNumberButton)
            {
                AllowTrainNumberEdit = false;
            }
        };
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

        //return connntectedSlots.Where(s => TakenSlots.Any(g => g.Any(gg => s.Row == gg.Row && s.Col == gg.Col)));

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
        connectedSlots ??= GetConnectedSlots(slot, seatType);
        foreach (var s in connectedSlots)
        {
            action(s);
        }
        return connectedSlots;
    }

    private void SeatDrag(object sender, MouseEventArgs e)
    {
        if (sender is Border element && e.LeftButton == MouseButtonState.Pressed)
        {
            if (element.Tag is string type)
            {
                FeatureTour.GetNavigator().IfCurrentStepEquals("Step1").GoNext();
                SeatType seatType = Enum.Parse<SeatType>(type);
                DragDrop.DoDragDrop(element, seatType, DragDropEffects.Copy);
                FeatureTour.GetNavigator().IfCurrentStepEquals("Step2").GoPrevious();
            }
            else if (element.Tag is Slot slot && slot.SeatType != SeatType.None)
            {
                FeatureTour.GetNavigator().IfCurrentStepEquals("Step3").GoNext();
                DragDrop.DoDragDrop(element, element.Tag, DragDropEffects.Move);
                FeatureTour.GetNavigator().IfCurrentStepEquals("Step4").GoPrevious();
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
            FeatureTour.GetNavigator().IfCurrentStepEquals("Step2").GoNext();
            seatType = (SeatType)e.Data.GetData(typeof(SeatType));
        }
        else if (e.Effects == DragDropEffects.Move)
        {
            seatType = ((Slot)e.Data.GetData(typeof(Slot))).SeatType;
            sourceSlot = e.Data.GetData(typeof(Slot)) as Slot;

            //if (sourceSlot.SeatType == slot.SeatType) return;

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
                    !slots.Any(s => sourceSlotsToRemove.Any(ss => ss.Row == s.Row && ss.Col == s.Col))
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
                    !slots.Any(s => slotsToRemove.Any(ss => ss.Row == s.Row && ss.Col == s.Col))
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
        SlotsPanel.ClearValue(BorderBrushProperty);
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
                    !slots.Any(s => sourceSlotsToRemove.Any(ss => ss.Row == s.Row && ss.Col == s.Col))
                ).ToList();
            }
            ReorderSeatNumbers();
        }
        CanDeleteSeats = TakenSlots.Count > 0;
        FeatureTour.GetNavigator().IfCurrentStepEquals("Step4").GoNext();
        LeaveTrash(sender, e);
    }

    [ICommand] 
    public void DeleteAllSeats()
    {
        var window = new ConfirmCancelWindow
        {
            Title = "Uklanjanje svih sedišta",
            Message = "Da li ste sigurni da želite da uklonite sva sedišta iz voza?",
            ConfirmButtonText = "Ukloni",
            ConfirmIsDanger = true,
            Image = MessageBoxImage.Stop
        };
        window.ShowDialog();

        if (window.Confirmed)
        {
            Slots.SelectMany(s => s).ToList().ForEach(s => s.SeatType = SeatType.None);
            TakenSlots.Clear();
            CanDeleteSeats = false;
        }
    }

    [ICommand]
    public void EditTrainNumber()
    {
        if (AllowTrainNumberEdit)
        {
            AllowTrainNumberEdit = false;
        }
        else
        {
            AllowTrainNumberEdit = true;
            TrainNumberTextBox.Focus();
        }
    }

    [ICommand]
    public void Confrim()
    {
        List<string> Errors = new();
        if (TakenSlots.Count == 0)
        {
            Errors.Add("Raspored voza mora sadržati bar jedno sedište");
            SlotsPanel.BorderBrush = Brushes.Red;
        }
        if (string.IsNullOrEmpty(TrainNumber))
        {
            Errors.Add("Broj voza ne sme biti prazan");
            TrainNumberTextBox.BorderBrush = Brushes.Red;
        }

        var w = new ConfirmCancelWindow
        {
            Title = Errors.Count > 0 ? "Greška" : "Čuvanje izmena",
            Message = Errors.Count > 0 ? "Nije moguće sačuvati izmene zbog sledećih grešaka:" : "Da li ste sigurni da želite da sačuvate izmene voza?",
            ConfirmButtonText = Errors.Count > 0 ? "U redu" : "Sačuvaj izmene",
            Errors = Errors,
            Image = MessageBoxImage.Question
        };
        w.ShowDialog();

        if (Errors.Count == 0 && w.Confirmed)
        {
            using DbContext db = new();
            if (CurrentTrain is null)
            {
                CurrentTrain = new();
            }
            else
            {
                db.Attach(CurrentTrain);
            }
            CurrentTrain.Number = TrainNumber;
            CurrentTrain.Type = SelectedTrainType.Type;
            CurrentTrain.Seating = TakenSlots.Select(s => new SeatGroup
            {
                SeatType = s.First().SeatType,
                Seats = s.Select(slot => new Seat()
                {
                    SeatNumber = slot.SeatNumber,
                    SeatType = slot.SeatType,
                    Row = slot.Row,
                    Col = slot.Col
                }).ToList()
            }).ToList();
            
            db.Update(CurrentTrain);
            db.SaveChanges();
            Confirmed = true;
            Close();
        }
    }

    [ICommand]
    public void Cancel()
    {
        var w = new ConfirmCancelWindow
        {
            Message = "Da li ste sigurni da želite da odustanete od izmene voza?",
            ConfirmButtonText = "Odustani od izmena",
            CancelButtonText = "Otkaži",
            ConfirmIsDanger = true,
            Image = MessageBoxImage.Stop
        };
        w.ShowDialog();

        if (w.Confirmed)
        {
            Cancelled = true;
            Close();
        }
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        if (!Cancelled && !Confirmed) // Closed by pressing x
        {
            var w = new ConfirmCancelWindow
            {
                Message = "Da li ste sigurni da želite da odustanete od izmene voza?",
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
        HelpProvider.ShowHelp("AddEditTrain", this);
    }

    [ICommand]
    public void StartTutorial()
    {
        TextLocalization.Next = "Dalje";
        TextLocalization.Close = "Završi";
        TextLocalization.DoIt = "Uradi korak za mene";
        var tour = new Tour()
        {
            Name = "My Demo Tour",
            ShowNextButtonDefault = true,
            Steps = new []
            {
                new Step("DobuleSeatBorder", "Header 01", "Kliknite levim tasterom i započnite prevlačenje sedišta ka rasporedu", "Step1"),
                new Step("SlotsPanel", "Header 01", "Spustite sedište na mrežu rasporeda", "Step2"),
                new Step("SlotsPanel", "Header 01", "Kliknite levim tasterom i započnite prevlačenje sedišta ka kanti za brisanje", "Step3"),
                new Step("Trashcan", "Header 01", "Spustite sedište na kantu za brisanje", "Step4"),
                new Step("TrainTypeComboBox", "Header 01", "Odaberite tip voza", "Step5"),
                new Step("TrainNameTextBox", "Header 01", "Izmenite broj voza klikom na dugme \"Izmeni\", unosom teksta u polje i klikom na dugme \"Gotovo\"", "Step6"),
                new Step("ConfirmButton", "Header 01", "Sačuvajte izmene klikom na taster ispod", "Step7")
            }
        };

        tour.Start();
    }

    private void WrapPanelMouseDown(object sender, MouseButtonEventArgs e)
    {
        FeatureTour.GetNavigator().IfCurrentStepEquals("Step5").GoNext();
    }
    public void OnTrainNumberEditTextChanged(string oldText, string newText)
    {
        if (oldText == "Gotovo" && newText == "Izmeni")
        {
            FeatureTour.GetNavigator().IfCurrentStepEquals("Step6").GoNext();
        }
    }
}