using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace POS.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(50)]
        public string Color { get; set; } = "#4CAF50"; // defualt is green

        public bool IsActive { get; set; } = true;


        public virtual ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
    }
}
