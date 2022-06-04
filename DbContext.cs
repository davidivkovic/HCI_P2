﻿using Microsoft.EntityFrameworkCore;
using P2.Model;

namespace P2;

public class DbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Train> Trains { get; set; }
    public DbSet<Station> Stations { get; set; }
    public DbSet<TrainLine> Lines { get; set; }
    public DbSet<Departure> Departures { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseLazyLoadingProxies().UseSqlite("Data Source=HCI.db;");
    }
}