using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Models
{
    public class MenuItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name {  get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        public int CategoryId { get; set; }
        public virtual Category Category { get; set; }


        [StringLength(100)]
        public string Color { get; set; } = "#2196F3";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
