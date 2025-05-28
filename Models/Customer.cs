using System.ComponentModel.DataAnnotations;

namespace POS.Models
{
    public class Customer : BaseEntity // Inherit from BaseEntity
    {
        // Id, CreatedAt, IsActive are now inherited
        // [Key] // Removed
        // public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [Required]
        [StringLength(500)]
        public string Address { get; set; }

        [Required]
        [StringLength(15)]
        public string Phone { get; set; }

        // public DateTime CreatedAt { get; set; } = DateTime.Now; // Removed
    }
}