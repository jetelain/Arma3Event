﻿// <auto-generated />
using System;
using Arma3Event.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Arma3Event.Migrations
{
    [DbContext(typeof(Arma3EventContext))]
    [Migration("20200606150307_ValidationAndState")]
    partial class ValidationAndState
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.2");

            modelBuilder.Entity("Arma3Event.Entities.Faction", b =>
                {
                    b.Property<int>("FactionID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Flag")
                        .HasColumnType("TEXT");

                    b.Property<int?>("GameMarker")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<int>("UsualSide")
                        .HasColumnType("INTEGER");

                    b.HasKey("FactionID");

                    b.ToTable("Faction");
                });

            modelBuilder.Entity("Arma3Event.Entities.GameMap", b =>
                {
                    b.Property<int>("GameMapID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Image")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("WebMap")
                        .HasColumnType("TEXT");

                    b.Property<string>("WorkshopLink")
                        .HasColumnType("TEXT");

                    b.HasKey("GameMapID");

                    b.ToTable("Maps");
                });

            modelBuilder.Entity("Arma3Event.Entities.MapMarker", b =>
                {
                    b.Property<int>("MapMarkerID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("MarkerData")
                        .HasColumnType("TEXT");

                    b.Property<int>("MatchID")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("MatchUserID")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("RoundSideID")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("RoundSquadID")
                        .HasColumnType("INTEGER");

                    b.HasKey("MapMarkerID");

                    b.HasIndex("MatchID");

                    b.HasIndex("MatchUserID");

                    b.HasIndex("RoundSideID");

                    b.HasIndex("RoundSquadID");

                    b.ToTable("MapMarkers");
                });

            modelBuilder.Entity("Arma3Event.Entities.Match", b =>
                {
                    b.Property<int>("MatchID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<string>("DiscordLink")
                        .HasColumnType("TEXT");

                    b.Property<int?>("GameMapID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Image")
                        .HasColumnType("TEXT");

                    b.Property<bool>("InviteOnly")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("RulesLink")
                        .HasColumnType("TEXT");

                    b.Property<string>("ShortDescription")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("TEXT");

                    b.Property<int>("State")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Template")
                        .HasColumnType("INTEGER");

                    b.HasKey("MatchID");

                    b.HasIndex("GameMapID");

                    b.ToTable("Match");
                });

            modelBuilder.Entity("Arma3Event.Entities.MatchSide", b =>
                {
                    b.Property<int>("MatchSideID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("MatchID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("MaxUsersCount")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<int>("Number")
                        .HasColumnType("INTEGER");

                    b.Property<int>("SquadsPolicy")
                        .HasColumnType("INTEGER");

                    b.HasKey("MatchSideID");

                    b.HasIndex("MatchID");

                    b.ToTable("MatchSide");
                });

            modelBuilder.Entity("Arma3Event.Entities.MatchTechnicalInfos", b =>
                {
                    b.Property<int>("MatchTechnicalInfosID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("GameServerAddress")
                        .HasColumnType("TEXT");

                    b.Property<string>("GameServerPassword")
                        .HasColumnType("TEXT");

                    b.Property<int?>("GameServerPort")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("HoursBeforeRevealPasswords")
                        .HasColumnType("INTEGER");

                    b.Property<int>("MatchID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ModsCount")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ModsDefinition")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("ModsLastChange")
                        .HasColumnType("TEXT");

                    b.Property<string>("VoipServerAddress")
                        .HasColumnType("TEXT");

                    b.Property<string>("VoipServerPassword")
                        .HasColumnType("TEXT");

                    b.Property<int?>("VoipServerPort")
                        .HasColumnType("INTEGER");

                    b.Property<int>("VoipSystem")
                        .HasColumnType("INTEGER");

                    b.HasKey("MatchTechnicalInfosID");

                    b.HasIndex("MatchID")
                        .IsUnique();

                    b.ToTable("MatchTechnicalInfos");
                });

            modelBuilder.Entity("Arma3Event.Entities.MatchUser", b =>
                {
                    b.Property<int>("MatchUserID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("MatchID")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("MatchSideID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("UserID")
                        .HasColumnType("INTEGER");

                    b.HasKey("MatchUserID");

                    b.HasIndex("MatchID");

                    b.HasIndex("MatchSideID");

                    b.HasIndex("UserID");

                    b.ToTable("MatchUser");
                });

            modelBuilder.Entity("Arma3Event.Entities.Round", b =>
                {
                    b.Property<int>("RoundID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("MatchID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Number")
                        .HasColumnType("INTEGER");

                    b.HasKey("RoundID");

                    b.HasIndex("MatchID");

                    b.ToTable("Round");
                });

            modelBuilder.Entity("Arma3Event.Entities.RoundSide", b =>
                {
                    b.Property<int>("RoundSideID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("FactionID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("GameSide")
                        .HasColumnType("INTEGER");

                    b.Property<int>("MatchSideID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("RoundID")
                        .HasColumnType("INTEGER");

                    b.HasKey("RoundSideID");

                    b.HasIndex("FactionID");

                    b.HasIndex("MatchSideID");

                    b.HasIndex("RoundID");

                    b.ToTable("RoundSide");
                });

            modelBuilder.Entity("Arma3Event.Entities.RoundSlot", b =>
                {
                    b.Property<int>("RoundSlotID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsValidated")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Label")
                        .HasColumnType("TEXT");

                    b.Property<int?>("MatchUserID")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("Role")
                        .HasColumnType("INTEGER");

                    b.Property<int>("RoundSquadID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("SlotNumber")
                        .HasColumnType("INTEGER");

                    b.Property<long?>("Timestamp")
                        .HasColumnType("INTEGER");

                    b.HasKey("RoundSlotID");

                    b.HasIndex("MatchUserID");

                    b.HasIndex("RoundSquadID");

                    b.ToTable("RoundSlot");
                });

            modelBuilder.Entity("Arma3Event.Entities.RoundSquad", b =>
                {
                    b.Property<int>("RoundSquadID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("InviteOnly")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<bool>("RestrictTeamComposition")
                        .HasColumnType("INTEGER");

                    b.Property<int>("RoundSideID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("SlotsCount")
                        .HasColumnType("INTEGER");

                    b.Property<string>("UniqueDesignation")
                        .HasColumnType("TEXT");

                    b.HasKey("RoundSquadID");

                    b.HasIndex("RoundSideID");

                    b.ToTable("RoundSquad");
                });

            modelBuilder.Entity("Arma3Event.Entities.User", b =>
                {
                    b.Property<int>("UserID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT")
                        .HasMaxLength(100);

                    b.Property<int>("PrivacyOptions")
                        .HasColumnType("INTEGER");

                    b.Property<string>("SteamId")
                        .HasColumnType("TEXT")
                        .HasMaxLength(50);

                    b.Property<string>("SteamName")
                        .HasColumnType("TEXT")
                        .HasMaxLength(100);

                    b.HasKey("UserID");

                    b.ToTable("User");
                });

            modelBuilder.Entity("Arma3Event.Entities.MapMarker", b =>
                {
                    b.HasOne("Arma3Event.Entities.Match", "Match")
                        .WithMany()
                        .HasForeignKey("MatchID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Arma3Event.Entities.MatchUser", "MatchUser")
                        .WithMany()
                        .HasForeignKey("MatchUserID")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("Arma3Event.Entities.RoundSide", "RoundSide")
                        .WithMany()
                        .HasForeignKey("RoundSideID");

                    b.HasOne("Arma3Event.Entities.RoundSquad", "RoundSquad")
                        .WithMany()
                        .HasForeignKey("RoundSquadID")
                        .OnDelete(DeleteBehavior.SetNull);
                });

            modelBuilder.Entity("Arma3Event.Entities.Match", b =>
                {
                    b.HasOne("Arma3Event.Entities.GameMap", "GameMap")
                        .WithMany()
                        .HasForeignKey("GameMapID");
                });

            modelBuilder.Entity("Arma3Event.Entities.MatchSide", b =>
                {
                    b.HasOne("Arma3Event.Entities.Match", "Match")
                        .WithMany("Sides")
                        .HasForeignKey("MatchID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Arma3Event.Entities.MatchTechnicalInfos", b =>
                {
                    b.HasOne("Arma3Event.Entities.Match", "Match")
                        .WithOne("MatchTechnicalInfos")
                        .HasForeignKey("Arma3Event.Entities.MatchTechnicalInfos", "MatchID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Arma3Event.Entities.MatchUser", b =>
                {
                    b.HasOne("Arma3Event.Entities.Match", "Match")
                        .WithMany("Users")
                        .HasForeignKey("MatchID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Arma3Event.Entities.MatchSide", "Side")
                        .WithMany("Users")
                        .HasForeignKey("MatchSideID");

                    b.HasOne("Arma3Event.Entities.User", "User")
                        .WithMany("Matchs")
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Arma3Event.Entities.Round", b =>
                {
                    b.HasOne("Arma3Event.Entities.Match", "Match")
                        .WithMany("Rounds")
                        .HasForeignKey("MatchID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Arma3Event.Entities.RoundSide", b =>
                {
                    b.HasOne("Arma3Event.Entities.Faction", "Faction")
                        .WithMany()
                        .HasForeignKey("FactionID");

                    b.HasOne("Arma3Event.Entities.MatchSide", "MatchSide")
                        .WithMany("Rounds")
                        .HasForeignKey("MatchSideID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Arma3Event.Entities.Round", "Round")
                        .WithMany("Sides")
                        .HasForeignKey("RoundID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Arma3Event.Entities.RoundSlot", b =>
                {
                    b.HasOne("Arma3Event.Entities.MatchUser", "AssignedUser")
                        .WithMany("Slots")
                        .HasForeignKey("MatchUserID")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("Arma3Event.Entities.RoundSquad", "Squad")
                        .WithMany("Slots")
                        .HasForeignKey("RoundSquadID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Arma3Event.Entities.RoundSquad", b =>
                {
                    b.HasOne("Arma3Event.Entities.RoundSide", "Side")
                        .WithMany("Squads")
                        .HasForeignKey("RoundSideID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
