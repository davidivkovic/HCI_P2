using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using P2.Model;
using P2.Primitives;
using P2.Windows;

namespace P2.Views
{
    public partial class LoginView : Component
    {
        public LoginView()
        {
            InitializeComponent();
            Dispatcher.BeginInvoke(() =>
            {
                username.Focus();
            }, DispatcherPriority.Loaded);
        }

        public delegate void LoginSuccess(User user);
        public event LoginSuccess OnLoginSuccess;

        public string Username { get; set; }
        public string Password { get; set; }

        public bool IsButtonEnabled => !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password);

        public string UsernameError { get; set; }
        public string PasswordError { get; set; }

        public Visibility UsernameErrorVisible => string.IsNullOrEmpty(UsernameError) ? Visibility.Collapsed : Visibility.Visible;
        public Visibility PasswordErrorVisible => string.IsNullOrEmpty(PasswordError) ? Visibility.Collapsed : Visibility.Visible;

        [ICommand] public void Login()
        {
            UsernameError = PasswordError = "";
            using DbContext db = new();
            var user = db.Users.Where(u => u.Username == Username).FirstOrDefault();
            if (user is null)
                UsernameError = "Korisnik ne postoji. Pokušajte ponovo.";
            else if (user.Password != Password)
                PasswordError = "Neispravna lozinka. Pokušajte ponovo.";
            else
            {
                OnLoginSuccess(user);
            }
        }

        private void PasswordChanged(object sender, RoutedEventArgs e) => Password = ((PasswordBox)sender).Password;

        [ICommand]
        public static void Register()
        {
            Register w = new();

            w.ShowDialog();

            if (w.Confirmed)
            {
                ConfirmCancelWindow cw = new()
                {
                    Title = "Uspeh",
                    Message = $"Uspešno ste registrovani.\nVaše korisničko ime je {w.Username}.",
                    ConfirmButtonText = "U redu",
                    CancelButtonText = "",
                    Image = MessageBoxImage.Information
                };
                cw.ShowDialog();
            }
        }

    }
}
