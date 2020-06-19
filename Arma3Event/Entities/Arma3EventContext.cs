using System;
using System.IO;
using System.Linq;
using Arma3Event.Arma3GameInfos;
using ArmaEvent.Arma3GameInfos;
using Microsoft.EntityFrameworkCore;
using Arma3Event.Entities;

namespace Arma3Event.Entities
{
    public class Arma3EventContext : DbContext
    {
        public Arma3EventContext(DbContextOptions<Arma3EventContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Faction> Factions { get; set; }
        public DbSet<MatchUser> MatchUsers { get; set; }
        public DbSet<MatchSide> MatchSides { get; set; }
        public DbSet<Match> Matchs { get; set; }
        public DbSet<GameMap> Maps { get; set; }
        public DbSet<Round> Rounds { get; set; }
        public DbSet<RoundSide> RoundSides { get; set; }
        public DbSet<RoundSquad> RoundSquads { get; set; }
        public DbSet<RoundSlot> RoundSlots { get; set; }
        public DbSet<MapMarker> MapMarkers { get; set; }
        public DbSet<MatchTechnicalInfos> MatchTechnicalInfos { get; set; }
        public DbSet<UserLogin> UserLogins { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<Faction>().ToTable("Faction");

            var mapMarkers = modelBuilder.Entity<MapMarker>().ToTable("MapMarkers");
            mapMarkers.HasOne(t => t.MatchUser).WithMany().OnDelete(DeleteBehavior.SetNull);
            mapMarkers.HasOne(t => t.RoundSquad).WithMany().OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<MatchTechnicalInfos>().ToTable("MatchTechnicalInfos");
            modelBuilder.Entity<Match>().ToTable("Match");
            modelBuilder.Entity<MatchSide>().ToTable("MatchSide");
            modelBuilder.Entity<MatchUser>().ToTable("MatchUser");
            modelBuilder.Entity<Round>().ToTable("Round");
            modelBuilder.Entity<RoundSide>().ToTable("RoundSide");
            modelBuilder.Entity<RoundSquad>().ToTable("RoundSquad");

            var roundSlot = modelBuilder.Entity<RoundSlot>().ToTable("RoundSlot");
            roundSlot.HasOne(t => t.Squad).WithMany(s => s.Slots).OnDelete(DeleteBehavior.Cascade);
            roundSlot.HasOne(t => t.AssignedUser).WithMany(s => s.Slots).OnDelete(DeleteBehavior.SetNull);

            var userLogin = modelBuilder.Entity<UserLogin>().ToTable("UserLogin");
            userLogin.HasIndex(p => p.Login).IsUnique();
            userLogin.HasIndex(p => p.UserID).IsUnique();

            modelBuilder.Entity<News>().ToTable("News");
        }

        internal void InitBaseData()
        {
            if (!Factions.Any())
            {
                Factions.Add(new Faction() { Name = "OTAN", UsualSide = GameSide.BLUFOR, Flag = "/img/flags/nato.png", GameMarker = GameMarkerType.flag_nato });
                Factions.Add(new Faction() { Name = "CSAT", UsualSide = GameSide.OPFOR, Flag = "/img/flags/csat.png", GameMarker = GameMarkerType.flag_csat });
                Factions.Add(new Faction() { Name = "AAF", UsualSide = GameSide.Independant, Flag = "/img/flags/aaf.png", GameMarker = GameMarkerType.flag_aaf });
                Factions.Add(new Faction() { Name = "USA", UsualSide = GameSide.BLUFOR, Flag = "/img/flags/us.png", GameMarker = GameMarkerType.flag_usa });
                Factions.Add(new Faction() { Name = "UK", UsualSide = GameSide.BLUFOR, Flag = "/img/flags/uk.png", GameMarker = GameMarkerType.flag_uk });
                Factions.Add(new Faction() { Name = "FIA", UsualSide = GameSide.Independant, Flag = "/img/flags/fia.png", GameMarker = GameMarkerType.flag_fia });
                Factions.Add(new Faction() { Name = "France", UsualSide = GameSide.BLUFOR, Flag = "/img/flags/fr.png", GameMarker = GameMarkerType.flag_france });
                SaveChanges();
            }
            if (!Maps.Any())
            {
                Maps.Add(new GameMap() { Name = "Altis", Image = "/img/maps/altis.jpg" });
                Maps.Add(new GameMap() { Name = "Stratis", Image = "/img/maps/stratis.jpg" });
                Maps.Add(new GameMap() { Name = "Tanoa", Image = "/img/maps/tanoa.jpg", WebMap = "tanoa" });
                Maps.Add(new GameMap() { Name = "Linovia", Image = "/img/maps/linovia.jpg" });
                Maps.Add(new GameMap() { Name = "Taunus", Image = "/img/maps/x-cam-taunus.jpg", WebMap = "taunus", WorkshopLink = "https://steamcommunity.com/sharedfiles/filedetails/?id=836147398" });
                Maps.Add(new GameMap() { Name = "Lythium", Image = "/img/maps/lythium.jpg", WorkshopLink = "https://steamcommunity.com/sharedfiles/filedetails/?id=909547724" });
                SaveChanges();
            }
        }

        public DbSet<Arma3Event.Entities.News> News { get; set; }

    }
}
