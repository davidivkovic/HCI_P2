using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.EntityFrameworkCore;
using P2.Model;
using P2.Stores;
using P2.Windows;

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
                        .ThenInclude(s => s.Station)
            .Where(t => t.Customer.Id == UserStore.User.Id)
        );
    }
}
