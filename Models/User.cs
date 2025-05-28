using System.ComponentModel.DataAnnotations;
using POS.Enums;
namespace POS.Models
{
    public class User : BaseEntity // Inherit from BaseEntity
    {
        // Id, CreatedAt, IsActive are now inherited
        // [Key] // Removed
        // public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public Role Role { get; set; } // Admin, Cashir

        [StringLength(100)]
        public string FullName { get; set; }

        // public DateTime CreatedAt { get; set; } = DateTime.Now; // Removed
        // public bool IsActive { get; set; } = true; // Removed
    }
}