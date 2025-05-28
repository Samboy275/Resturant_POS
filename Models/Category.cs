using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace POS.Models
{
    public class Category : BaseEntity // Inherit from BaseEntity
    {
        // Id, CreatedAt, IsActive are now inherited
        // [Key] // Removed, as it's in BaseEntity
        // public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(50)]
        public string Color { get; set; } = "#4CAF50"; // default is green

        // public bool IsActive { get; set; } = true; // Removed, as it's in BaseEntity

        public virtual ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
    }
}