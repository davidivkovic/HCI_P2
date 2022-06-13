using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using P2.Model;
using P2.Stores;

namespace P2.Views;

public partial class CustomerTicketsView : Primitives.Component
{
    public ObservableCollection<Ticket> Tickets { get; set; }

    public CustomerTicketsView()
    {
        InitializeComponent();
        using DbContext db = new();
        Tickets = new(db.Tickets
            .Include(t => t.Seats)
            .Include(t => t.Source)
            .Include(t => t.Destination)
            .Include(t => t.Departure)
                .ThenInclude(d => d.Train)
            .Include(t => t.Departure)
                .ThenInclude(d => d.Line)
                    .ThenInclude(l => l.Stops)
            .Where(t => t.Customer.Id == UserStore.Store.User.Id)
            .ToList()
        );
    }
}
