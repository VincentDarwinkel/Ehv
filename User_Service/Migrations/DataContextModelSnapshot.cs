﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using User_Service.Dal;

namespace User_Service.Migrations
{
    [DbContext(typeof(DataContext))]
    partial class DataContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 64)
                .HasAnnotation("ProductVersion", "5.0.6");

            modelBuilder.Entity("User_Service.Models.ActivationDto", b =>
                {
                    b.Property<Guid>("Uuid")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<string>("Code")
                        .HasColumnType("longtext");

                    b.Property<Guid>("UserUuid")
                        .HasColumnType("char(36)");

                    b.HasKey("Uuid");

                    b.ToTable("Activation");
                });

            modelBuilder.Entity("User_Service.Models.DisabledUserDto", b =>
                {
                    b.Property<Guid>("Uuid")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<int>("Reason")
                        .HasColumnType("int");

                    b.Property<Guid>("UserUuid")
                        .HasColumnType("char(36)");

                    b.HasKey("Uuid");

                    b.ToTable("DisabledUser");
                });

            modelBuilder.Entity("User_Service.Models.FavoriteArtistDto", b =>
                {
                    b.Property<Guid>("Uuid")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<string>("Artist")
                        .HasColumnType("longtext");

                    b.Property<Guid?>("UserDtoUuid")
                        .HasColumnType("char(36)");

                    b.Property<Guid>("UserUuid")
                        .HasColumnType("char(36)");

                    b.HasKey("Uuid");

                    b.HasIndex("UserDtoUuid");

                    b.ToTable("Artist");
                });

            modelBuilder.Entity("User_Service.Models.PasswordResetDto", b =>
                {
                    b.Property<Guid>("Uuid")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<string>("Code")
                        .HasColumnType("longtext");

                    b.Property<Guid>("UserUuid")
                        .HasColumnType("char(36)");

                    b.HasKey("Uuid");

                    b.ToTable("PasswordReset");
                });

            modelBuilder.Entity("User_Service.Models.UserDto", b =>
                {
                    b.Property<Guid>("Uuid")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<string>("About")
                        .HasColumnType("longtext");

                    b.Property<int>("AccountRole")
                        .HasColumnType("int");

                    b.Property<DateTime>("BirthDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Email")
                        .HasColumnType("longtext");

                    b.Property<int>("Gender")
                        .HasColumnType("int");

                    b.Property<string>("Username")
                        .HasColumnType("longtext");

                    b.HasKey("Uuid");

                    b.ToTable("User");
                });

            modelBuilder.Entity("User_Service.Models.UserHobbyDto", b =>
                {
                    b.Property<Guid>("Uuid")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<string>("Hobby")
                        .HasColumnType("longtext");

                    b.Property<Guid?>("UserDtoUuid")
                        .HasColumnType("char(36)");

                    b.Property<Guid>("UserUuid")
                        .HasColumnType("char(36)");

                    b.HasKey("Uuid");

                    b.HasIndex("UserDtoUuid");

                    b.ToTable("Hobby");
                });

            modelBuilder.Entity("User_Service.Models.FavoriteArtistDto", b =>
                {
                    b.HasOne("User_Service.Models.UserDto", null)
                        .WithMany("FavoriteArtists")
                        .HasForeignKey("UserDtoUuid");
                });

            modelBuilder.Entity("User_Service.Models.UserHobbyDto", b =>
                {
                    b.HasOne("User_Service.Models.UserDto", null)
                        .WithMany("Hobbies")
                        .HasForeignKey("UserDtoUuid");
                });

            modelBuilder.Entity("User_Service.Models.UserDto", b =>
                {
                    b.Navigation("FavoriteArtists");

                    b.Navigation("Hobbies");
                });
#pragma warning restore 612, 618
        }
    }
}
