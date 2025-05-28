using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using POS.Database;
using POS.Models;

namespace POS.Controllers
{
    public class OrderController
    {
        private readonly POSDbContext _context; // Renamed to POSDbContext for consistency

        // CRITICAL: Change this constructor to accept POSDbContext via DI
        public OrderController(POSDbContext context) // This is the corrected constructor
        {
            _context = context;
        }

        // Removed the problematic public OrderController() { _context = new PosDbContext(); }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            order.OrderNumber = GenerateOrderNumber();
            order.CalculateTotal();

            // Ensure related entities are tracked if they're new (e.g., Customer)
            if (order.Customer != null && order.Customer.Id == 0) // If a new customer is linked
            {
                _context.Customers.Add(order.Customer);
            }
            // EF Core will handle OrderItems when Order is added if they are new
            // (OrderItems are typically part of the order graph you're adding)

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<Order> GetOrderAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem) // Make sure MenuItem is correctly included if needed for display
                .Include(o => o.User)
                // Include Customer if you need its details (e.g., for reports or order viewing)
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<Customer> GetCustomerAsync(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return null;
            }
            // Use AsNoTracking() for read-only lookups for performance
            // Also, consider adding IsActive check for customers, similar to users
            return await _context.Customers
                .AsNoTracking() // Add AsNoTracking()
                .FirstOrDefaultAsync(c => c.Phone == phone && c.IsActive); // Assumed Customer.Phonetic property, add IsActive
        }

        public async Task<Customer> CreateCustomerAsync(Customer customer)
        {
            // Add validation here if needed
            if (customer == null) throw new ArgumentNullException(nameof(customer));
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        // Add an UpdateCustomerAsync if you intend to update customer details
        public async Task<Customer> UpdateCustomerAsync(Customer customer)
        {
            if (customer == null) throw new ArgumentNullException(nameof(customer));
            _context.Customers.Update(customer); // Marks the entity as modified
            await _context.SaveChangesAsync();
            return customer;
        }


        public async Task<List<Order>> GetOrdersByDateAsync(DateTime date)
        {
            var startDate = date.Date;
            var endDate = startDate.AddDays(1); // Correctly gets orders for the entire day

            return await _context.Orders
                .Include(o => o.OrderItems) // Include items for display
                .Where(o => o.OrderDate >= startDate && o.OrderDate < endDate) // Use < endDate for entire day
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<List<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            // Note: endDate.AddDays(1) will include orders up to the start of the *next* day after endDate.
            // If you want to include orders up to 23:59:59 of endDate, this is correct.
            // If you want only full days *between* startDate and endDate (exclusive of endDate's last moment), use <= endDate.Date.
            return await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.OrderDate >= startDate && o.OrderDate < endDate.AddDays(1)) // Use < for end date
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        private string GenerateOrderNumber()
        {
            // Good simple unique number generation. For high-volume systems, consider database sequences.
            return $"ORD{DateTime.Now:yyyyMMdd}{DateTime.Now.Ticks.ToString().Substring(10)}";
        }
    }
}