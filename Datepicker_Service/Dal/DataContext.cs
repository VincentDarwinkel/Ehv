﻿using Datepicker_Service.Models;
using Microsoft.EntityFrameworkCore;

namespace Datepicker_Service.Dal
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }
        public virtual DbSet<DatepickerDto> Datepicker { get; set; }
        public virtual DbSet<DatepickerDateDto> DatepickerDate { get; set; }
        public virtual DbSet<DatepickerAvailabilityDto> DatepickerAvailability { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DatepickerDto>(entity =>
            {
                entity.HasKey(e => e.Uuid);
                entity.HasMany(e => e.Dates)
                    .WithOne()
                    .HasForeignKey(e => e.DatePickerUuid);
            });
            modelBuilder.Entity<DatepickerDateDto>(entity =>
            {
                entity.HasKey(e => e.Uuid);
                entity.HasMany(e => e.UserAvailabilities)
                    .WithOne()
                    .HasForeignKey(e => e.DateUuid);
            });
            modelBuilder.Entity<DatepickerAvailabilityDto>(entity =>
            {
                entity.HasKey(e => e.Uuid);
            });
        }
    }
}