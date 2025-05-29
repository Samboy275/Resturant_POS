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
        static void Main()
        {
            string connectionString = "";
            var services = new ServiceCollection();

            services.AddDbContext<POSDbContext>(options =>
             options.UseSqlServer(connectionString));

            // Register Contollers and services
            services.AddTransient<AuthController>();
            services.AddTransient<OrderController>();
            services.AddTransient<MenuController>();
            services.AddTransient<PrintService>();
            services.AddTransient<ReportController>();
            services.AddTransient<LoginForm>();
            services.AddTransient<MainForm>();

            var serviceProvider = services.BuildServiceProvider();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (var loginFrom = serviceProvider.GetRequiredService<LoginForm>())
            {
                if (loginFrom.ShowDialog() == DialogResult.OK)
                {
                    var currentUser = loginFrom.AuthenticatedUser;

                    var mainForm = serviceProvider.GetRequiredService<MainForm>();
                    mainForm.SetCurrentUser(currentUser);
                    Application.Run(mainForm);
                }
                else
                {
                    Application.Exit();
                }
            }

        }
    }
}