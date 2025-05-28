using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using POS.Enums;

namespace POS.Models
{

    public class Order : BaseEntity
    {
        [Required]
        [StringLength(20)]
        public string OrderNumber {  get; set; }

        [Required]
        public OrderType OrderType { get; set; } // Delivery, Takeaway

        public OrderStatus OrderStatus { get; set; } // Completed, Pending, Cancelled

        [Column(TypeName = "Decimal(10,2)")]
        public decimal Total { get; set; }

        [Column(TypeName = "Decimal(10,2)")]
        public decimal AmountPaid { get; set; }

        [Column(TypeName = "Decimal(10,2)")]
        public decimal Change {  get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        // Delivry Specific Fields
        public int? CustomerId { get; set; }
        public virtual Customer? Customer { get; set; }


        public void CalculateTotal()
        {
            Total = OrderItems.Sum(oi => oi.Quantity * oi.Price);
        }
    }

}
