using System.Linq;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using P2.Model;

namespace P2;

public partial class App : Application
{
    public App()
    {
        using DbContext db = new();
        db.Database.Migrate();

        if (db.Users.FirstOrDefault(u => u.Username == "m") == null)
        {
            db.Add(new User
            {
                Username = "m",
                Password = "m",
                FirstName = "Gospodin",
                LastName = "Menadzer",
                Role = Role.Manager
            });
        }
        if (db.Users.FirstOrDefault(u => u.Username == "k") == null)
        {
            db.Add(new User
            {
                Username = "k",
                Password = "k",
                FirstName = "Gospodin",
                LastName = "Kupac",
                Role = Role.Customer
            });
        }

        if(!db.Stations.Any())
        {
            db.Add(new Station
            {
                Name = "Beograd Centar",
                Latitude = 44.755717,
                Longitude = 20.520289
            });

            db.Add(new Station
            {
                Name = "Novi Sad (MAS)",
                Latitude = 45.248851,
                Longitude = 19.810473
            });

            db.Add(new Station
            {
                Name = "Nis",
                Latitude = 43.305059,
                Longitude = 21.889582
            });

        }

        db.SaveChanges();
    }
}