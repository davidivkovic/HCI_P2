using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using P2.Model;
using P2.Primitives;

namespace P2.Views;

public partial class TestView : Component
{
    public TestView()
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

        Users = new(db.Users.ToList());
    }

    public int Counter { get; set; }
    [ICommand] public void Increment() => Counter++;

    public ObservableCollection<User> Users { get; set; }
}
