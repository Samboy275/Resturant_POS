using System.ComponentModel.DataAnnotations;
using POS.Enums;
namespace POS.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public Roles Role {  get; set; } // Admin, Cashir


        [StringLength(100)]
        public string FullName { get; set; }


        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;

    }
}
