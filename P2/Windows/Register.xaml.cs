using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace P2.Windows;

public partial class Register : Primitives.Window
{
    public bool Confirmed { get; set; }

    public Register()
    {
        InitializeComponent();
    }

    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }

    public bool IsButtonEnabled => !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password);

    public string UsernameError { get; set; }
    public string PasswordError { get; set; }

    public Visibility UsernameErrorVisible => string.IsNullOrEmpty(UsernameError) ? Visibility.Collapsed : Visibility.Visible;
    public Visibility PasswordErrorVisible => string.IsNullOrEmpty(PasswordError) ? Visibility.Collapsed : Visibility.Visible;

    [ICommand]
    public void Submit()
    {
        UsernameError = PasswordError = string.Empty;
        using DbContext db = new();

        if (db.Users.FirstOrDefault(u => u.Username == Username) != null)
        {
            UsernameError = "Nalog sa ovim korisničkim imenom već postoji.";
        }
        if (Password.Length < 6)
        {
            PasswordError = "Lozinka mora sadržati bar 6 karaktera.";
        }
        
        if (UsernameError == string.Empty && PasswordError == string.Empty)
        {
            db.Users.Add(new()
            {
                FirstName = FirstName,
                LastName = LastName,
                Username = Username,
                Password = Password,
                Role = Model.Role.Customer
            });
            db.SaveChanges();

            Confirmed = true;
            Close();
        }
    }

    private void PasswordChanged(object sender, RoutedEventArgs e) => Password = ((PasswordBox)sender).Password;

    [ICommand]
    public void Cancel() => Close();

    private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        HelpProvider.ShowHelp("Registration", this);
    }
}