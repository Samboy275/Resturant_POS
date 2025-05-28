using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace POS.Models
{
    public abstract class BaseEntity
    {
        [Key] // This marks it as the primary key for all inheriting entities
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; } // Set by SaveChanges()

        public DateTime ModifiedAt { get; set; } // New: Set by SaveChanges() for updates


        // Soft deletion flag
        public bool IsActive { get; set; } = true;

        // Optional: If you prefer a timestamp for soft delete instead of a boolean flag
        // public DateTime? DeletedAt { get; set; }
    }
}
