using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.Input;
using P2.Model;
using P2.Primitives;

namespace P2.Views
{
    public partial class LoginView : Component
    {
        public LoginView()
        {
            InitializeComponent();
        }

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
        }

        private void PasswordChanged(object sender, RoutedEventArgs e) => Password = ((PasswordBox)sender).Password;

    }
}
