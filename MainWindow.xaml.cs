using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using P2.Model;
using P2.Primitives;
using P2.Stores;
using P2.Views;

namespace P2;

public partial class MainWindow : Primitives.Window
{
    public Component CurrentView { get; set; } = new LoginView();
    public Visibility NavbarVisible => CurrentView.GetType() == typeof(LoginView) ? Visibility.Collapsed : Visibility.Visible;
    public double ViewMinWidth => CurrentView.MinWidth + 20; // Compensate for scrollbar
    public double ViewMinHeight => CurrentView.MinHeight + 20;

    public MainWindow()
    {
        InitializeComponent();
        if (CurrentView is LoginView lv)
        {
            lv.OnLoginSuccess += OnLogin;
        }
    }

    public void OnLogin(User user)
    {
        UserStore.Store.User = user;
        //if (UserStore.User.Role == Role.Manager)
        //{
        //    CurrentView = new TimetableView();
        //}
        //else
        //{
        //    CurrentView = new TimetableView();
        //}
        CurrentView = new TimetableView();
    }

    [ICommand]
    public void LogOut()
    {
        UserStore.Store.User = null;
        CurrentView = new LoginView();
        if (CurrentView is LoginView lv)
        {
            lv.OnLoginSuccess += OnLogin;
        }
    }

    [ICommand] public void ShowTrains() => CurrentView = new TrainsView();
    [ICommand] public void ShowLines() => CurrentView = new LinesView();
    [ICommand] public void ShowTimetable() => CurrentView = new TimetableView();
    [ICommand] public void ShowDepartureTickets() => CurrentView = new DepartureTicketsView();
    [ICommand] public void ShowMonthlyTickets() => CurrentView = new MonthlyTicketsView();
    [ICommand] public void ShowCustomerTickets() => CurrentView = new CustomerTicketsView();
    [ICommand] public void ShowGeneralHelp() => HelpProvider.ShowHelp("GeneralHelp" + (UserStore.Store.User.Role == Role.Customer ? "Customer" : "Manager"), this);
    

    private void ShowHelp(object sender, ExecutedRoutedEventArgs e)
    {
        if (CurrentView.GetType().Name.Equals("LoginView"))
            HelpProvider.ShowHelp(CurrentView.GetType().Name, this);
        else
            HelpProvider.ShowHelp(CurrentView.GetType().Name + (UserStore.Store.User.Role == Role.Customer ? "Customer" : "Manager"), this);
    }
}
