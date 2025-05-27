using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Models
{
    public class DailySummary
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public int TotalOrders { get; set; }
        public int TakeAwayOrders { get; set; }
        public int DeliveryOrders { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal TotalSales { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal TakeAwaySales { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal DeliverySales { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal TotalTax { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}