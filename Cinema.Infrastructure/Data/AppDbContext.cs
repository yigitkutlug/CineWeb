using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Cinema.Domain.Entities;

namespace Cinema.Infrastructure.Data;

// EF Core context: Identity tablolarini ve uygulama tablolarini birlikte yonetir.
public class AppDbContext : IdentityDbContext
{
    // DI ile gelen DbContext ayarlari base sinifa aktarilir.
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Film tablosu.
    public DbSet<Movie> Movies => Set<Movie>();
    // Film <-> Oyuncu join tablosu (many-to-many)
    public DbSet<MovieActor> MovieActors => Set<MovieActor>();
    public DbSet<Hall> Halls => Set<Hall>();
    public DbSet<Seat> Seats => Set<Seat>();
    public DbSet<Showtime> Showtimes => Set<Showtime>();
    // Oyuncu tablosu.
    public DbSet<Actor> Actors => Set<Actor>();
    // Kullanici profil tablosu.
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    // Bilet gecmisi tablosu.
    public DbSet<Ticket> Tickets => Set<Ticket>();
    // Film yorumlari tablosu.
    public DbSet<MovieReview> MovieReviews => Set<MovieReview>();
    // Film yorum yanitlari ve begeniler.
    public DbSet<MovieReviewReply> MovieReviewReplies => Set<MovieReviewReply>();
    public DbSet<MovieReviewLike> MovieReviewLikes => Set<MovieReviewLike>();
    // Rozetler ve kullanici rozetleri.
    public DbSet<Badge> Badges => Set<Badge>();
    public DbSet<UserBadge> UserBadges => Set<UserBadge>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Once Identity tablolarinin konfigurasyonu calissin.
        base.OnModelCreating(builder);


        // Many-to-many (MovieActor) icin composite primary key:
        // Ayni actor ayni filme ikinci kez baglanamasin.
        builder.Entity<MovieActor>()
            .HasKey(x => new { x.MovieId, x.ActorId });

        // MovieActor -> Movie iliskisi (coktan-bire)
        builder.Entity<MovieActor>()
            .HasOne(x => x.Movie)
            .WithMany(x => x.MovieActors)
            .HasForeignKey(x => x.MovieId);

        // MovieActor -> Actor iliskisi (coktan-bire)
        builder.Entity<MovieActor>()
            .HasOne(x => x.Actor)
            .WithMany(x => x.MovieActors)
            .HasForeignKey(x => x.ActorId);

        // Koltuklar salon bazinda tekil olmali (A-10 iki kere olusmasin).
        // Bir salonda ayni satir+numara koltuk tekrar olusmasin.
        builder.Entity<Seat>()
            .HasIndex(x => new { x.HallId, x.RowLabel, x.SeatNumber })
            .IsUnique();

        // Admin seans girerken ayni film/salon/saat duplicate acamasin.
        // Ayni film icin ayni salonda ayni saatte ikinci seans olusmasin.
        builder.Entity<Showtime>()
            .HasIndex(x => new { x.MovieId, x.HallId, x.StartsAt })
            .IsUnique();

        // Satin alma guvenligi: ayni seansta ayni koltuk tekrar satilamaz.
        // Ayni seansta ayni koltuk ikinci kez satilamasin (seat lock).
        builder.Entity<Ticket>()
            .HasIndex(x => new { x.ShowtimeId, x.SeatId })
            .IsUnique();

        // Bir kullanici ayni filme tek yorum yazabilir.
        builder.Entity<MovieReview>()
            .HasIndex(x => new { x.MovieId, x.UserId })
            .IsUnique();

        // MovieReview -> IdentityUser iliskisi (FK: UserId)
        builder.Entity<MovieReview>()
            .HasOne<IdentityUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Onayli yorumlari hizli sorgulamak icin.
        builder.Entity<MovieReview>()
            .HasIndex(x => new { x.MovieId, x.IsApproved });

        // Reply -> Review iliskisi
        builder.Entity<MovieReviewReply>()
            .HasOne(x => x.MovieReview)
            .WithMany()
            .HasForeignKey(x => x.MovieReviewId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<MovieReviewReply>()
            .HasIndex(x => x.MovieReviewId);

        // Like -> Review iliskisi, kullanici bir kez begenir.
        builder.Entity<MovieReviewLike>()
            .HasOne(x => x.MovieReview)
            .WithMany()
            .HasForeignKey(x => x.MovieReviewId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<MovieReviewLike>()
            .HasIndex(x => new { x.MovieReviewId, x.UserId })
            .IsUnique();

        // Kullanici adi tekil olmali (nullable oldugu icin birden fazla NULL kabul edilir).
        builder.Entity<UserProfile>()
            .HasIndex(x => x.UserName)
            .IsUnique();

        // Badge code tekil olmali.
        builder.Entity<Badge>()
            .HasIndex(x => x.Code)
            .IsUnique();

        // Kullanici ayni rozeti bir kere alir.
        builder.Entity<UserBadge>()
            .HasIndex(x => new { x.UserId, x.BadgeId })
            .IsUnique();

        builder.Entity<UserBadge>()
            .HasOne(x => x.Badge)
            .WithMany(x => x.UserBadges)
            .HasForeignKey(x => x.BadgeId);

    }
}
