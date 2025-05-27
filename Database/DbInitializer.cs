using Microsoft.EntityFrameworkCore;
using POS.Database;
using System;
using System.Threading.Tasks;

namespace POS.Database
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync()
        {
            using var context = new PosDbContext();

            try
            {
                await context.Database.EnsureCreatedAsync();
                Console.WriteLine("Database initialized successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing database: {ex.Message}");
                throw;
            }
        }
    }
}