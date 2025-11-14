using System.ComponentModel.DataAnnotations;

namespace EeD_BE_EeD.Models
{
    public class FAQ
    {
        [Key]

        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Question { get; set; }

        [Required]
        [StringLength(1000)]
        public string Answer { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
