using System;
using System.IO;
using System.Linq;
using ArmaEvent.Arma3GameInfos;
using Microsoft.EntityFrameworkCore;

namespace Arma3Event.Entities
{
    public class Arma3EventContext : DbContext
    {
        public Arma3EventContext(DbContextOptions<Arma3EventContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Faction> Factions { get; set; }
        public DbSet<MatchUser> MatchUsers { get; set; }
        public DbSet<MatchSide> MatchSides { get; set; }
        public DbSet<Match> Matchs { get; set; }
        public DbSet<GameMap> Maps { get; set; }
        public DbSet<Round> Rounds { get; set; }
        public DbSet<RoundSide> RoundSides { get; set; }
        public DbSet<RoundSquad> RoundSquads { get; set; }
        public DbSet<RoundSlot> RoundSlots { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<Role>().ToTable("Role");
            modelBuilder.Entity<Faction>().ToTable("Faction");

            modelBuilder.Entity<Match>().ToTable("Match");
            modelBuilder.Entity<MatchSide>().ToTable("MatchSide");
            modelBuilder.Entity<MatchUser>().ToTable("MatchUser");
            modelBuilder.Entity<Round>().ToTable("Round");
            modelBuilder.Entity<RoundSide>().ToTable("RoundSide");
            modelBuilder.Entity<RoundSquad>().ToTable("RoundSquad");
            modelBuilder.Entity<RoundSlot>().ToTable("RoundSlot").HasOne(t => t.Squad).WithMany(s => s.Slots).OnDelete(DeleteBehavior.Cascade); ;
        }

        internal void InitBaseData()
        {
            if (!Users.Any())
            {
                for (int i = 1; i <= 80; ++i)
                {
                    Users.Add(new User() { Name = $"User {i}", SteamId = "XXXXXXXXX" });
                }
                SaveChanges();

            }
            if (!Factions.Any())
            {
                Factions.Add(new Faction() { Name = "OTAN", UsualSide = GameSide.BLUFOR, Icon = "/img/flags/nato.png" });
                Factions.Add(new Faction() { Name = "CSAT", UsualSide = GameSide.OPFOR, Icon = "/img/flags/csat.png" });
                Factions.Add(new Faction() { Name = "AAF", UsualSide = GameSide.Independant, Icon = "/img/flags/aaf.png" });
                Factions.Add(new Faction() { Name = "USA", UsualSide = GameSide.Independant, Icon = "/img/flags/us.png" });
                Factions.Add(new Faction() { Name = "UK", UsualSide = GameSide.Independant, Icon = "/img/flags/uk.png" });
                Factions.Add(new Faction() { Name = "FIA", UsualSide = GameSide.Independant, Icon = "/img/flags/fia.png" });
                Factions.Add(new Faction() { Name = "France", UsualSide = GameSide.Independant, Icon = "/img/flags/fr.png" });
                SaveChanges();
            }
            if (!Maps.Any())
            {
                Maps.Add(new GameMap() { Name = "Altis", Image = "/img/maps/altis.jpg" });
                Maps.Add(new GameMap() { Name = "Stratis", Image = "/img/maps/stratis.jpg" });
                Maps.Add(new GameMap() { Name = "Tanoa", Image = "/img/maps/tanoa.jpg" });
                Maps.Add(new GameMap() { Name = "Linovia", Image = "/img/maps/linovia.jpg" });
                Maps.Add(new GameMap() { Name = "Taunus", Image = "/img/maps/x-cam-taunus.jpg", WebMap = "taunus", WorkshopLink = "https://steamcommunity.com/sharedfiles/filedetails/?id=836147398" });
                Maps.Add(new GameMap() { Name = "Lythium", Image = "/img/maps/lythium.jpg", WorkshopLink = "https://steamcommunity.com/sharedfiles/filedetails/?id=909547724" });
                SaveChanges();
            }
        }

    }
}
