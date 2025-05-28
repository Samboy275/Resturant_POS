using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic; // Added for List<Order>
using POS.Database;
using POS.Models;
using POS.Enums;

namespace POS.Controllers
{
    public class ReportController
    {
        private readonly POSDbContext _context; // Renamed to POSDbContext for consistency

        // CRITICAL: Change this constructor to accept POSDbContext via DI
        public ReportController(POSDbContext context) // This is the corrected constructor
        {
            _context = context;
        }

        // Removed the problematic public ReportController() { _context = new PosDbContext(); }

        public async Task<DailySummary> GenerateDailySummaryAsync(DateTime date)
        {
            var startDate = date.Date;
            var endDate = startDate.AddDays(1); // Correctly gets orders for the entire day

            var orders = await _context.Orders
                // Include OrderItems if you need to calculate sales per item, but not needed for total sales
                // .Include(o => o.OrderItems)
                // Ensure OrderStatus is compared with the enum value, not string
                .Where(o => o.OrderDate >= startDate && o.OrderDate < endDate && o.OrderStatus == OrderStatus.Completed)
                .ToListAsync();

            // DailySummary model needs to be defined
            var summary = new DailySummary
            {
                Date = date.Date,
                TotalOrders = orders.Count,
                // Ensure OrderType is compared with enum values
                TakeAwayOrders = orders.Count(o => o.OrderType == OrderType.Takeaway),
                DeliveryOrders = orders.Count(o => o.OrderType == OrderType.Delivery),
                TotalSales = orders.Sum(o => o.Total),
                TakeAwaySales = orders.Where(o => o.OrderType == OrderType.Takeaway).Sum(o => o.Total),
                DeliverySales = orders.Where(o => o.OrderType == OrderType.Delivery).Sum(o => o.Total)
            };

            return summary;
        }

        public async Task<object> GetShiftSummaryAsync(int userId, DateTime shiftStart)
        {
            var shiftEnd = DateTime.Now;

            var orders = await _context.Orders
                // .Include(o => o.OrderItems) // Only if needed for item-level reporting
                .Where(o => o.UserId == userId &&
                            o.OrderDate >= shiftStart &&
                            o.OrderDate <= shiftEnd &&
                            o.OrderStatus == OrderStatus.Completed) // Compare with enum value
                .ToListAsync();

            return new // Anonymous object is fine for simple reports, but a dedicated model is better
            {
                ShiftStart = shiftStart,
                ShiftEnd = shiftEnd,
                TotalOrders = orders.Count,
                TotalSales = orders.Sum(o => o.Total),
                TakeAwayOrders = orders.Count(o => o.OrderType == OrderType.Takeaway),
                DeliveryOrders = orders.Count(o => o.OrderType == OrderType.Delivery),
                TakeAwaySales = orders.Where(o => o.OrderType == OrderType.Takeaway).Sum(o => o.Total),
                DeliverySales = orders.Where(o => o.OrderType == OrderType.Delivery).Sum(o => o.Total)
            };
        }

        // REMOVE THIS Dispose() method. Let DI handle DbContext lifecycle.
        public void Dispose()
        {
            _context?.Dispose();
        }
    }

    // You need to define this DailySummary model in your POS.Models namespace
    public class DailySummary
    {
        public DateTime Date { get; set; }
        public int TotalOrders { get; set; }
        public int TakeAwayOrders { get; set; }
        public int DeliveryOrders { get; set; }
        public decimal TotalSales { get; set; }
        public decimal TakeAwaySales { get; set; }
        public decimal DeliverySales { get; set; }
        // Add more fields as needed (e.g., total items sold, average order value)
    }
}