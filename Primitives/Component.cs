using System.ComponentModel;
using System.Windows.Controls;

namespace P2.Primitives;

public partial class Component : UserControl, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
}