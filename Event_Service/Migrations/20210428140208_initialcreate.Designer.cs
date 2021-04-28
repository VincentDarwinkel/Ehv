﻿// <auto-generated />
using System;
using Event_Service.Dal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Event_Service.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20210428140208_initialcreate")]
    partial class initialcreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.14")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("Event_Service.Models.EventDateDto", b =>
                {
                    b.Property<Guid>("Uuid")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<DateTime>("DateTime")
                        .HasColumnType("datetime(6)");

                    b.Property<Guid?>("EventDtoUuid")
                        .HasColumnType("char(36)");

                    b.Property<Guid>("EventUuid")
                        .HasColumnType("char(36)");

                    b.HasKey("Uuid");

                    b.HasIndex("EventDtoUuid");

                    b.ToTable("EventDate");
                });

            modelBuilder.Entity("Event_Service.Models.EventDateUserDto", b =>
                {
                    b.Property<Guid>("Uuid")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<Guid?>("EventDateDtoUuid")
                        .HasColumnType("char(36)");

                    b.Property<Guid>("EventDateUuid")
                        .HasColumnType("char(36)");

                    b.Property<Guid>("UserUuid")
                        .HasColumnType("char(36)");

                    b.HasKey("Uuid");

                    b.HasIndex("EventDateDtoUuid");

                    b.ToTable("EventDateUser");
                });

            modelBuilder.Entity("Event_Service.Models.EventDto", b =>
                {
                    b.Property<Guid>("Uuid")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<Guid>("AuthorUuid")
                        .HasColumnType("char(36)");

                    b.Property<string>("Description")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("Location")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("Title")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.HasKey("Uuid");

                    b.ToTable("Event");
                });

            modelBuilder.Entity("Event_Service.Models.EventStepDto", b =>
                {
                    b.Property<Guid>("Uuid")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<string>("Description")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<Guid?>("EventDtoUuid")
                        .HasColumnType("char(36)");

                    b.Property<Guid>("EventUuid")
                        .HasColumnType("char(36)");

                    b.Property<int>("StepNr")
                        .HasColumnType("int");

                    b.HasKey("Uuid");

                    b.HasIndex("EventDtoUuid");

                    b.ToTable("EventStep");
                });

            modelBuilder.Entity("Event_Service.Models.EventStepUserDto", b =>
                {
                    b.Property<Guid>("Uuid")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<Guid?>("EventStepDtoUuid")
                        .HasColumnType("char(36)");

                    b.Property<Guid>("EventStepUuid")
                        .HasColumnType("char(36)");

                    b.Property<Guid>("UserUuid")
                        .HasColumnType("char(36)");

                    b.HasKey("Uuid");

                    b.HasIndex("EventStepDtoUuid");

                    b.ToTable("EventStepUser");
                });

            modelBuilder.Entity("Event_Service.Models.EventDateDto", b =>
                {
                    b.HasOne("Event_Service.Models.EventDto", null)
                        .WithMany("EventDates")
                        .HasForeignKey("EventDtoUuid");
                });

            modelBuilder.Entity("Event_Service.Models.EventDateUserDto", b =>
                {
                    b.HasOne("Event_Service.Models.EventDateDto", null)
                        .WithMany("EventDateUsers")
                        .HasForeignKey("EventDateDtoUuid");
                });

            modelBuilder.Entity("Event_Service.Models.EventStepDto", b =>
                {
                    b.HasOne("Event_Service.Models.EventDto", null)
                        .WithMany("EventSteps")
                        .HasForeignKey("EventDtoUuid");
                });

            modelBuilder.Entity("Event_Service.Models.EventStepUserDto", b =>
                {
                    b.HasOne("Event_Service.Models.EventStepDto", null)
                        .WithMany("EventStepUsers")
                        .HasForeignKey("EventStepDtoUuid");
                });
#pragma warning restore 612, 618
        }
    }
}
