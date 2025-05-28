using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Models
{
    public class OrderItem : BaseEntity // Inherit from BaseEntity
    {
        // Id, CreatedAt, IsActive are now inherited
        // public int Id { get; set; } // Removed

        public int OrderId { get; set; }
        public virtual Order Order { get; set; }

        public int MenuItemId { get; set; }
        public virtual MenuItem MenuItem { get; set; }

        [Required]
        [StringLength(50)]
        public string ItemName { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; } // Store price at time of order

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal LineTotal => Price * Quantity;
    }
}