using System.ComponentModel.DataAnnotations;

namespace EeD_BE_EeD.Models
{
    public class ContactMessage
    {
        [Key]

        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; }

        [Required]
        [StringLength(1000)]
        public string Message { get; set; }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public bool IsReplied { get; set; } = false;
    }
}
