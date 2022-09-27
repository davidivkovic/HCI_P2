using System.Windows;
using P2.Model;
using P2.Primitives;

namespace P2.Stores;

public class UserStore : Observable
{
    private static readonly UserStore _instance = new ();
    private UserStore() { }

    public static UserStore Store
    {
        get
        {
            return _instance;
        }
    }

    public User User { get; set; }
    public bool IsManager => User?.Role == Role.Manager;
    public Visibility VisibleOnlyToManager => IsManager ? Visibility.Visible : Visibility.Collapsed;
    public Visibility VisibleOnlyToCustomer => !IsManager ? Visibility.Visible : Visibility.Collapsed;
}