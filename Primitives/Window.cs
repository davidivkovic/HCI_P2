using System.ComponentModel;

namespace P2.Primitives;

public partial class Window : System.Windows.Window, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
}