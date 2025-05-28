// File: POS.Database/DbInitializer.cs (or wherever you put it)
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
// Assuming your DbContext is in POS.Data or POS.Database as well
using POS.Database; // Or using POS.Database; if that's where POSDbContext is

namespace POS.Database
{
    public static class DbInitializer
    {
        // This method now correctly expects DbContextOptions as an argument
        public static async Task InitializeAsync(DbContextOptions<POSDbContext> options)
        {
            // Use the provided options to create the context instance
            using var context = new POSDbContext(options);

            try
            {
                // This is generally what you want for a production app
                // It applies any pending migrations to bring the database schema up to date.
                await context.Database.MigrateAsync();
                Console.WriteLine("Database initialized and migrations applied successfully.");

                // Optional: Seed initial data if the database is empty
                await SeedDataAsync(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing database: {ex.Message}");
                // In a real app, you might want to log this error to a file or a dedicated logging service.
                // For a Windows Forms app, a MessageBox.Show() might also be appropriate to inform the user.
                // MessageBox.Show($"An error occurred while initializing the database: {ex.Message}",
                //                 "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw; // Re-throw the exception to prevent the application from starting if DB init fails critically.
            }
        }

        private static async Task SeedDataAsync(POSDbContext context)
        {
            // Example: Add initial categories if none exist
            if (!await context.Categories.AnyAsync())
            {
                // Make sure you have 'using POS.Models;' or fully qualify Category
                context.Categories.AddRange(
                    new POS.Models.Category { Name = "Drinks", Color = "#00BCD4" },
                    new POS.Models.Category
                    {
                        Name = "Food",
                        Color = "#FF9800" 
                    }
                );
                await context.SaveChangesAsync();
                Console.WriteLine("Initial categories seeded.");
            }
            // Add more seeding logic for other entities (e.g., a default admin user) if needed
        }
    }
}