using System.Windows;
using P2.Model;

namespace P2.Stores;

public static class UserStore
{
    public static User User { get; set; }
    public static bool IsManager => User?.Role == Role.Manager;
    public static Visibility VisibleOnlyToManager => IsManager ? Visibility.Visible : Visibility.Collapsed;
    public static Visibility VisibleOnlyToCustomer => !IsManager ? Visibility.Visible : Visibility.Collapsed;
}