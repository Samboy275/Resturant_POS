// File: Program.cs
using System;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection; // ADD THIS USING DIRECTIVE
using POS.Database; // Your DbInitializer (if still separate from Data)
using POS.Forms; // Your Forms
using POS.Controllers; // Your Controllers
using POS.Services; // Your Services
using POS.Models; // Your Models (e.g., User)

namespace POS.App
{
    static class Program
    {
        [STAThread]
        static async Task Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // --- 1. Build Service Provider (DI Container) ---
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // --- 2. Initialize and Migrate the Database ---
            // This is done once at application startup.
            using (var scope = serviceProvider.CreateScope()) // Use a scope for DB init
            {
                var services = scope.ServiceProvider;
                try
                {
                    var dbContextOptions = services.GetRequiredService<DbContextOptions<POSDbContext>>();
                    await DbInitializer.InitializeAsync(dbContextOptions);
                    Console.WriteLine("Database initialization and migration complete.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Application could not start due to a database error: {ex.Message}\n\nPlease check your database connection.",
                                    "Fatal Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Console.WriteLine($"Error during database initialization: {ex.Message}");
                    return; // Exit the application
                }
            }

            // --- 3. Start the Application Flow (Login then Main Form) ---
            try
            {
                User loggedInUser = null;

                // Resolve LoginForm using the serviceProvider
                // A new scope for the LoginForm is good practice
                using (var loginScope = serviceProvider.CreateScope())
                {
                    var loginServices = loginScope.ServiceProvider;
                    var loginForm = loginServices.GetRequiredService<LoginForm>(); // LoginForm now gets its own dependencies

                    var loginResult = loginForm.ShowDialog();

                    if (loginResult == DialogResult.OK && loginForm.LoggedInUser != null)
                    {
                        loggedInUser = loginForm.LoggedInUser;
                    }
                    else
                    {
                        Application.Exit(); // Login cancelled or failed
                        return;
                    }
                }

                // If login was successful, proceed to MainForm
                if (loggedInUser != null)
                {
                    // Create a new scope for the MainForm and its operations
                    // This ensures DbContext and other scoped services have proper lifetimes.
                    using (var mainFormScope = serviceProvider.CreateScope())
                    {
                        var mainFormServices = mainFormScope.ServiceProvider;
                        var mainForm = mainFormServices.GetRequiredService<MainForm>();
                        mainForm.SetCurrentUser(loggedInUser); // Pass the logged-in user
                        Application.Run(mainForm);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Application startup error: {ex.Message}", "Startup Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // Database Configuration
            string connectionString = "Server=(localdb)\\mssqllocaldb;Database=POSAppDb;Trusted_Connection=True;MultipleActiveResultSets=true";
            services.AddDbContext<POSDbContext>(options =>
                options.UseSqlServer(connectionString)
            );

            // Register your controllers and services as Scoped (per operation/scope)
            // A new DbContext will be created for each scope.
            services.AddScoped<MenuController>();
            services.AddScoped<OrderController>();
            services.AddScoped<PrintService>();
            services.AddScoped<AuthController>(); // Assuming you have a UserController for login/user ops

            // Register your Forms as Transient (a new instance each time requested)
            services.AddTransient<MainForm>();
            services.AddTransient<LoginForm>(); // Register LoginForm too

            // You might also need to register a "LoginService" if you extract login logic
            // services.AddScoped<ILoginService, LoginService>();
        }
    }
}