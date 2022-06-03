using System.Windows;
using Microsoft.EntityFrameworkCore;

namespace P2;

public partial class App : Application
{
    public App()
    {
        using DbContext db = new();
        db.Database.Migrate();
    }
}