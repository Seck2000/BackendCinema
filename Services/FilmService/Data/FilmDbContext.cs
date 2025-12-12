using Microsoft.EntityFrameworkCore;
using FilmService.Models;

namespace FilmService.Data
{
    public class FilmDbContext : DbContext
    {
        public FilmDbContext(DbContextOptions<FilmDbContext> options)
            : base(options)
        {
        }

        public DbSet<Film> Films { get; set; }
        public DbSet<Salle> Salles { get; set; }
        public DbSet<Seance> Seances { get; set; }
        public DbSet<Siege> Sieges { get; set; }
        public DbSet<CategorieAge> CategoriesAge { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuration des relations
            modelBuilder.Entity<Seance>()
                .HasOne(s => s.Film)
                .WithMany(f => f.Seances)
                .HasForeignKey(s => s.FilmId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Seance>()
                .HasOne(s => s.Salle)
                .WithMany(s => s.Seances)
                .HasForeignKey(s => s.SalleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Siege>()
                .HasOne(s => s.Salle)
                .WithMany(s => s.Sieges)
                .HasForeignKey(s => s.SalleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed data pour les catégories d'âge
            modelBuilder.Entity<CategorieAge>().HasData(
                new CategorieAge { Id = 1, Nom = "Adulte", TrancheAge = "18+", Prix = 12.00m, EstActive = true, Description = "Tarif adulte standard" },
                new CategorieAge { Id = 2, Nom = "Étudiant", TrancheAge = "16-25", Prix = 9.00m, EstActive = true, Description = "Tarif étudiant avec carte" },
                new CategorieAge { Id = 3, Nom = "Enfant", TrancheAge = "0-15", Prix = 7.00m, EstActive = true, Description = "Tarif enfant" },
                new CategorieAge { Id = 4, Nom = "Senior", TrancheAge = "65+", Prix = 8.00m, EstActive = true, Description = "Tarif senior" }
            );
        }
    }
}

