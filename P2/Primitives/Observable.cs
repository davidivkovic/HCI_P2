using System.ComponentModel;

namespace P2.Primitives;

public class Observable : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
}