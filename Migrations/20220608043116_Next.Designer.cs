﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using P2;

#nullable disable

namespace P2.Migrations
{
    [DbContext(typeof(DbContext))]
    [Migration("20220608043116_Next")]
    partial class Next
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.5");

            modelBuilder.Entity("P2.Model.Departure", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("LineId")
                        .HasColumnType("INTEGER");

                    b.Property<TimeOnly>("Time")
                        .HasColumnType("TEXT");

                    b.Property<int?>("TrainId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("LineId");

                    b.HasIndex("TrainId");

                    b.ToTable("Departures");
                });

            modelBuilder.Entity("P2.Model.Seat", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("Col")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Row")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("SeatGroupId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("SeatNumber")
                        .HasColumnType("INTEGER");

                    b.Property<int>("SeatType")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("SeatGroupId");

                    b.ToTable("Seat");
                });

            modelBuilder.Entity("P2.Model.SeatGroup", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("SeatType")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("TrainId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("TrainId");

                    b.ToTable("SeatGroup");
                });

            modelBuilder.Entity("P2.Model.Station", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<double>("Latitude")
                        .HasColumnType("REAL");

                    b.Property<double>("Longitude")
                        .HasColumnType("REAL");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Stations");
                });

            modelBuilder.Entity("P2.Model.Stop", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<TimeSpan>("Duration")
                        .HasColumnType("TEXT");

                    b.Property<int>("Number")
                        .HasColumnType("INTEGER");

                    b.Property<double>("Price")
                        .HasColumnType("REAL");

                    b.Property<int?>("StationId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("TrainLineId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("StationId");

                    b.HasIndex("TrainLineId");

                    b.ToTable("Stops");
                });

            modelBuilder.Entity("P2.Model.Train", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Number")
                        .HasColumnType("TEXT");

                    b.Property<int>("Type")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Trains");
                });

            modelBuilder.Entity("P2.Model.TrainLine", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("DestinationId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("SourceId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("DestinationId");

                    b.HasIndex("SourceId");

                    b.ToTable("Lines");
                });

            modelBuilder.Entity("P2.Model.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("FirstName")
                        .HasColumnType("TEXT");

                    b.Property<string>("LastName")
                        .HasColumnType("TEXT");

                    b.Property<string>("Password")
                        .HasColumnType("TEXT");

                    b.Property<int>("Role")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Username")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("P2.Model.Departure", b =>
                {
                    b.HasOne("P2.Model.TrainLine", "Line")
                        .WithMany()
                        .HasForeignKey("LineId");

                    b.HasOne("P2.Model.Train", "Train")
                        .WithMany()
                        .HasForeignKey("TrainId");

                    b.Navigation("Line");

                    b.Navigation("Train");
                });

            modelBuilder.Entity("P2.Model.Seat", b =>
                {
                    b.HasOne("P2.Model.SeatGroup", null)
                        .WithMany("Seats")
                        .HasForeignKey("SeatGroupId");
                });

            modelBuilder.Entity("P2.Model.SeatGroup", b =>
                {
                    b.HasOne("P2.Model.Train", null)
                        .WithMany("Seating")
                        .HasForeignKey("TrainId");
                });

            modelBuilder.Entity("P2.Model.Stop", b =>
                {
                    b.HasOne("P2.Model.Station", "Station")
                        .WithMany()
                        .HasForeignKey("StationId");

                    b.HasOne("P2.Model.TrainLine", null)
                        .WithMany("Stops")
                        .HasForeignKey("TrainLineId");

                    b.Navigation("Station");
                });

            modelBuilder.Entity("P2.Model.TrainLine", b =>
                {
                    b.HasOne("P2.Model.Station", "Destination")
                        .WithMany()
                        .HasForeignKey("DestinationId");

                    b.HasOne("P2.Model.Station", "Source")
                        .WithMany()
                        .HasForeignKey("SourceId");

                    b.Navigation("Destination");

                    b.Navigation("Source");
                });

            modelBuilder.Entity("P2.Model.SeatGroup", b =>
                {
                    b.Navigation("Seats");
                });

            modelBuilder.Entity("P2.Model.Train", b =>
                {
                    b.Navigation("Seating");
                });

            modelBuilder.Entity("P2.Model.TrainLine", b =>
                {
                    b.Navigation("Stops");
                });
#pragma warning restore 612, 618
        }
    }
}
