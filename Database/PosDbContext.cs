using Microsoft.EntityFrameworkCore;
using POS.Models; // Assuming your models are in this namespace
using System;
using System.Linq; // Needed for .Sum() and potentially other LINQ methods
using System.Threading;
using System.Threading.Tasks;

namespace POS.Database
{
    public class POSDbContext : DbContext
    {
        // --- DbSets for your Models ---
        public DbSet<Category> Categories { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<DailySummary> DailySummaries { get; set; } // DailySummary does NOT inherit BaseEntity
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<User> Users { get; set; }

        // --- Constructor for Dependency Injection ---
        public POSDbContext(DbContextOptions<POSDbContext> options) : base(options) { }

        // --- Model Configuration (Fluent API & Global Query Filters) ---
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Always call the base method first to ensure default EF Core conventions are applied
            base.OnModelCreating(modelBuilder);

            // Apply Global Query Filter for Soft Deletion (IsActive)
            // This ensures that all queries for entities inheriting BaseEntity will automatically
            // filter out records where IsActive is false.
            // This is inside the foreach loop within OnModelCreating
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    // Corrected line: Use a helper method or a more explicit cast if needed.
                    // A common and robust way is to use a non-generic HasQueryFilter override
                    // that takes an Expression<Func<TEntity, bool>>
                    var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                    var property = System.Linq.Expressions.Expression.Property(parameter, nameof(BaseEntity.IsActive));
                    var filter = System.Linq.Expressions.Expression.Lambda(property, parameter);

                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
                }
            }

            // --- Optional: Configure Enum-to-String Conversion (if you prefer string storage) ---
            // By default, EF Core stores enums as integers. If you want them stored as strings (e.g., "Delivery", "Admin"),
            // you need to specify a value converter. If you don't add these, enums will be stored as integers.
            // Remove the [StringLength] attribute from the model properties if using these converters,
            // as EF Core will determine the max length based on the enum values.

            modelBuilder.Entity<Order>()
                .Property(o => o.OrderType)
                .HasConversion<string>(); // Converts OrderType enum to its string name in DB

            modelBuilder.Entity<Order>()
                .Property(o => o.OrderStatus)
                .HasConversion<string>(); // Converts OrderStatus enum to its string name in DB

            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>(); // Converts UserRole enum to its string name in DB
        }

        // --- Override SaveChanges for Automatic Auditing (CreatedAt, ModifiedAt) ---
        public override int SaveChanges()
        {
            // Use UtcNow for consistency across time zones and servers
            var now = DateTime.UtcNow;

            // Iterate through all tracked entity entries that have changed
            foreach (var entry in ChangeTracker.Entries())
            {
                // Check if the current entity inherits from our BaseEntity
                if (entry.Entity is BaseEntity baseEntity)
                {
                    // If the entity is being added (new record)
                    if (entry.State == EntityState.Added)
                    {
                        baseEntity.CreatedAt = now;
                        baseEntity.ModifiedAt = now; // Also set ModifiedAt on creation
                        // baseEntity.IsActive is already true by default in BaseEntity's constructor
                    }
                    // If the entity is being modified (existing record updated)
                    else if (entry.State == EntityState.Modified)
                    {
                        baseEntity.ModifiedAt = now; // Update ModifiedAt timestamp

                        // Prevent the CreatedAt field from being updated after initial creation.
                        // This ensures it truly represents the record's creation time.
                        entry.Property(nameof(BaseEntity.CreatedAt)).IsModified = false;
                    }
                }
            }
            // Call the base SaveChanges method to persist changes to the database
            return base.SaveChanges();
        }

        // --- Override SaveChangesAsync for Asynchronous Auditing ---
        // It's important to override both synchronous and asynchronous versions for consistency
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is BaseEntity baseEntity)
                {
                    if (entry.State == EntityState.Added)
                    {
                        baseEntity.CreatedAt = now;
                        baseEntity.ModifiedAt = now;
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        baseEntity.ModifiedAt = now;
                        entry.Property(nameof(BaseEntity.CreatedAt)).IsModified = false;
                    }
                }
            }
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}