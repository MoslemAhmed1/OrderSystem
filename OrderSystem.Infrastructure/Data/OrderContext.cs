using Microsoft.EntityFrameworkCore;
using OrderSystem.Domain.Entities;

namespace OrderSystem.Infrastructure.Data
{
    public class OrderContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Product> Products { get; set; }

        public OrderContext(DbContextOptions<OrderContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Customer
            modelBuilder.Entity<Customer>()
                .Property(c => c.FirstName)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<Customer>()
                .Property(c => c.LastName)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<Customer>()
                .Property(c => c.CustomerType)
                .HasConversion<string>()
                .HasMaxLength(20);

            // User -> Customer: One-to-One
            modelBuilder.Entity<Customer>()
                .HasOne(c => c.User)
                .WithOne()
                .HasForeignKey<Customer>(c => c.UserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            // Product
            modelBuilder.Entity<Product>()
                .Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Product>().Property(p => p.Price).HasPrecision(18, 2);

            // OrderItem
            modelBuilder.Entity<OrderItem>().Property(i => i.UnitPrice).HasPrecision(18, 2);

            // Order
            modelBuilder.Entity<Order>()
                .Property(o => o.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            modelBuilder.Entity<Order>().Property(o => o.Total).HasPrecision(18, 2);
            modelBuilder.Entity<Order>().HasQueryFilter(o => !o.IsDeleted);

            // Order -> Customer: Many-One, Required
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany()
                .HasForeignKey(o => o.CustomerId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Order -> OrderItems: One-Many, Cascade delete
            modelBuilder.Entity<Order>()
                .HasMany(o => o.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // OrderItem -> Product: Many-One, Required
            modelBuilder.Entity<OrderItem>()
                .HasOne(i => i.Product)
                .WithMany()
                .HasForeignKey(i => i.ProductId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // User
            modelBuilder.Entity<User>()
                .Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(256);

            modelBuilder.Entity<User>()
                .Property(u => u.PasswordHash)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>()
                .HasMaxLength(20);

            modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

            // RefreshToken
            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
