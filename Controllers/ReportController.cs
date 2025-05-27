using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using System.Linq;
using POS.Database;
using POS.Models;

namespace POS.Controllers
{
    public class ReportController
    {
        private readonly PosDbContext _context;

        public ReportController()
        {
            _context = new PosDbContext();
        }

        public async Task<DailySummary> GenerateDailySummaryAsync(DateTime date)
        {
            var startDate = date.Date;
            var endDate = startDate.AddDays(1);

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.OrderDate >= startDate && o.OrderDate < endDate && o.OrderStatus == "Completed")
                .ToListAsync();

            var summary = new DailySummary
            {
                Date = date.Date,
                TotalOrders = orders.Count,
                TakeAwayOrders = orders.Count(o => o.OrderType == "TakeAway"),
                DeliveryOrders = orders.Count(o => o.OrderType == "Delivery"),
                TotalSales = orders.Sum(o => o.Total),
                TakeAwaySales = orders.Where(o => o.OrderType == "TakeAway").Sum(o => o.Total),
                DeliverySales = orders.Where(o => o.OrderType == "Delivery").Sum(o => o.Total)
            };

            return summary;
        }

        public async Task<object> GetShiftSummaryAsync(int userId, DateTime shiftStart)
        {
            var shiftEnd = DateTime.Now;

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == userId &&
                           o.OrderDate >= shiftStart &&
                           o.OrderDate <= shiftEnd &&
                           o.OrderStatus == "Completed")
                .ToListAsync();

            return new
            {
                ShiftStart = shiftStart,
                ShiftEnd = shiftEnd,
                TotalOrders = orders.Count,
                TotalSales = orders.Sum(o => o.Total),
                TakeAwayOrders = orders.Count(o => o.OrderType == "TakeAway"),
                DeliveryOrders = orders.Count(o => o.OrderType == "Delivery"),
                TakeAwaySales = orders.Where(o => o.OrderType == "TakeAway").Sum(o => o.Total),
                DeliverySales = orders.Where(o => o.OrderType == "Delivery").Sum(o => o.Total)
            };
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
