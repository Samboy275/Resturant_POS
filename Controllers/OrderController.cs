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
        private readonly PosDbContext _context;

        public OrderController()
        {
            _context = new PosDbContext();
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            order.OrderNumber = GenerateOrderNumber();
            order.CalculateTotal();

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return order;
        }

        public async Task<Order> GetOrderAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<Customer> GetCustomerAsync(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return null;
            }

            return await _context.Customers.
                FirstOrDefaultAsync(c => c.Phone == phone);

        }

        public async Task<Customer> CreateCustomerAsync(Customer customer)
        {
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        public async Task<List<Order>> GetOrdersByDateAsync(DateTime date)
        {
            var startDate = date.Date;
            var endDate = startDate.AddDays(1);

            return await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.OrderDate >= startDate && o.OrderDate < endDate)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<List<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate.AddDays(1))
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        private string GenerateOrderNumber()
        {
            return $"ORD{DateTime.Now:yyyyMMdd}{DateTime.Now.Ticks.ToString().Substring(10)}";
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
