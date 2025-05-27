using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using POS.Models;
using Microsoft.EntityFrameworkCore;
namespace POS.Database
{
    public class PosDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<DailySummary> DailySummaries { get; set; }
        public DbSet<Shift> Shifts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=RestaurantPOS;Trusted_Connection=true;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<MenuItem>()
                .HasOne(m => m.Category)
                .WithMany(c => c.MenuItems)
                .HasForeignKey(m => m.CategoryId);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.MenuItem)
                .WithMany()
                .HasForeignKey(oi => oi.MenuItemId);

            modelBuilder.Entity<Shift>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId);

            // Seed initial data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed admin user (password: "admin123")
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    Role = "Admin",
                    FullName = "System Administrator"
                }
            );

            // Seed categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Appetizers", Color = "#FF5722" },
                new Category { Id = 2, Name = "Main Dishes", Color = "#4CAF50" },
                new Category { Id = 3, Name = "Desserts", Color = "#E91E63" },
                new Category { Id = 4, Name = "Beverages", Color = "#2196F3" },
                new Category { Id = 5, Name = "Salads", Color = "#8BC34A" }
            );

            // Seed menu items
            modelBuilder.Entity<MenuItem>().HasData(
                // Appetizers
                new MenuItem { Id = 1, Name = "Buffalo Wings", Price = 12.99m, CategoryId = 1, Color = "#FF6B35" },
                new MenuItem { Id = 2, Name = "Mozzarella Sticks", Price = 9.99m, CategoryId = 1, Color = "#FFB830" },
                new MenuItem { Id = 3, Name = "Onion Rings", Price = 7.99m, CategoryId = 1, Color = "#FF8C42" },

                // Main Dishes
                new MenuItem { Id = 4, Name = "Grilled Chicken", Price = 18.99m, CategoryId = 2, Color = "#6BCF7F" },
                new MenuItem { Id = 5, Name = "Beef Burger", Price = 15.99m, CategoryId = 2, Color = "#4D7C0F" },
                new MenuItem { Id = 6, Name = "Fish & Chips", Price = 16.99m, CategoryId = 2, Color = "#059669" },

                // Desserts
                new MenuItem { Id = 7, Name = "Chocolate Cake", Price = 8.99m, CategoryId = 3, Color = "#EC4899" },
                new MenuItem { Id = 8, Name = "Ice Cream", Price = 5.99m, CategoryId = 3, Color = "#F472B6" },

                // Beverages
                new MenuItem { Id = 9, Name = "Coca Cola", Price = 2.99m, CategoryId = 4, Color = "#3B82F6" },
                new MenuItem { Id = 10, Name = "Orange Juice", Price = 3.99m, CategoryId = 4, Color = "#60A5FA" },
                new MenuItem { Id = 11, Name = "Coffee", Price = 2.49m, CategoryId = 4, Color = "#1E40AF" },

                // Salads
                new MenuItem { Id = 12, Name = "Caesar Salad", Price = 11.99m, CategoryId = 5, Color = "#9CA3AF" },
                new MenuItem { Id = 13, Name = "Greek Salad", Price = 12.99m, CategoryId = 5, Color = "#6B7280" }
            );
        }
    }
}