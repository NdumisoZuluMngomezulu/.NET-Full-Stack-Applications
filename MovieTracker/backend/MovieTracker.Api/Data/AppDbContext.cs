using Microsoft.EntityFrameworkCore;
using MovieTracker.Api.Models;

namespace MovieTracker.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Viewing> Viewings => Set<Viewing>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Movie>(e =>
        {
            e.Property(m => m.Title).HasMaxLength(200).IsRequired();
            e.Property(m => m.Country).HasMaxLength(100);
            e.Property(m => m.Director).HasMaxLength(200);
            e.Property(m => m.PosterUrl).HasMaxLength(2000);
            e.Property(m => m.Synopsis).HasMaxLength(2000);
            e.HasIndex(m => m.Title);
            e.HasIndex(m => m.Country);

            e.HasMany(m => m.Viewings)
                .WithOne(v => v.Movie)
                .HasForeignKey(v => v.MovieId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Viewing>(e =>
        {
            e.Property(v => v.Notes).HasMaxLength(1000);
            e.HasIndex(v => v.WatchedOn);
        });
    }
}
