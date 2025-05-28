using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Models
{
    public class Shift : BaseEntity // Inherit from BaseEntity
    {
        // Id, CreatedAt, IsActive are now inherited
        // public int Id { get; set; } // Removed

        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal StartingCash { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal EndingCash { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal TotalSales { get; set; }

        public int OrderCount { get; set; }

        // public bool IsActive { get; set; } = true; // Removed
    }
}