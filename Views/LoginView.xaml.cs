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

            using DbContext db = new();
            db.Add(new User
            {
                Username = "admin",
                Password = "admin",
                FirstName = "John",
                LastName = "Admin"
            });
            db.SaveChanges();
        }

        public string Username { get; set; }
        public string Password { get; set; }

        public bool IsButtonEnabled => !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password);

        public string Error { get; set; }

        [ICommand] public void Login()
        {
            using DbContext db = new();
            var user = db.Users.Where(u => u.Username == Username).FirstOrDefault();
            if (user is null)
                Error = "Korisnik ne postoji. Pokušajte ponovo.";
            else if (user.Password != Password)
                Error = "Neispravna lozinka. Pokušajte ponovo.";
            else
                Error = "";
                // TODO navigacija
        }

        private void PasswordChanged(object sender, RoutedEventArgs e) => Password = ((PasswordBox)sender).Password;

    }
}
