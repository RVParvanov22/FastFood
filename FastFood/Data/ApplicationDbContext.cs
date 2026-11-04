using Microsoft.EntityFrameworkCore;
using FastFoodMVC.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace FastFood.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<FoodItem> FoodItems { get; set; }
        public DbSet<FoodCategory> FoodCategories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<FoodItem>().HasOne<FoodCategory>().WithMany(c => c.FoodItems)
                .HasForeignKey(f => f.FoodTypeId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<OrderItem>().HasOne(i => i.Order).WithMany(o => o.Items).HasForeignKey(i => i.OrderId);
            builder.Entity<OrderItem>().HasOne(i => i.FoodItem).WithMany(f => f.OrderItems).HasForeignKey(i => i.FoodItemId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Order>().HasOne(o => o.User).WithMany().HasForeignKey(o => o.UserId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
