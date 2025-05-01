using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CampRating.Models;

namespace CampRating.Data
{
    /// <summary>
    /// Контекст на базата данни за приложението
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Таблица за места за къмпингуване
        /// </summary>
        public DbSet<CampPlace> CampPlaces { get; set; }

        /// <summary>
        /// Таблица за ревюта
        /// </summary>
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Конфигурация на връзката между място за къмпингуване и потребител
            builder.Entity<CampPlace>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Предотвратява изтриване на потребител, ако има свързани места

            // Конфигурация на връзката между ревю и потребител
            builder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Предотвратява изтриване на потребител, ако има ревюта

            // Конфигурация на връзката между ревю и място за къмпингуване
            builder.Entity<Review>()
                .HasOne(r => r.CampPlace)
                .WithMany(c => c.Reviews)
                .HasForeignKey(r => r.CampPlaceId)
                .OnDelete(DeleteBehavior.Cascade); // Изтрива ревютата при изтриване на мястото
        }
    }
} 