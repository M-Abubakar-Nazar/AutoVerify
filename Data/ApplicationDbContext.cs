
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AutoVerifyMVC.Models;

namespace AutoVerifyMVC.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<CarListing> CarListings => Set<CarListing>();
        public DbSet<CarInquiry> CarInquiries => Set<CarInquiry>();

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<CarListing>(entity =>
            {
                entity.Property(car => car.Price).HasPrecision(18, 2);
                entity.HasIndex(car => car.SeedKey).IsUnique();
                entity.HasIndex(car => new { car.Status, car.ModelYear });
                entity.HasOne(car => car.Seller)
                    .WithMany()
                    .HasForeignKey(car => car.SellerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<CarInquiry>(entity =>
            {
                entity.HasIndex(inquiry => new { inquiry.BuyerId, inquiry.CreatedAtUtc });
                entity.HasIndex(inquiry => new { inquiry.CarListingId, inquiry.Status });
                entity.HasOne(inquiry => inquiry.CarListing)
                    .WithMany()
                    .HasForeignKey(inquiry => inquiry.CarListingId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(inquiry => inquiry.Buyer)
                    .WithMany()
                    .HasForeignKey(inquiry => inquiry.BuyerId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(inquiry => inquiry.Inspector)
                    .WithMany()
                    .HasForeignKey(inquiry => inquiry.InspectorId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(inquiry => new { inquiry.InspectorId, inquiry.Status });
            });
        }
    }
}
