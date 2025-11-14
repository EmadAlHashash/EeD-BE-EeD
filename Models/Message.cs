using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EeD_BE_EeD.Models
{
    public class Message
    {
        [Key]

        public int Id { get; set; }

        [Required]
        [StringLength(2000)]
        public string Content { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;

        // ✅ المرسل
        [Required]
        public string SenderId { get; set; }

        [ForeignKey(nameof(SenderId))]
        public virtual ApplicationUser Sender { get; set; }

        // ✅ المستقبل
        [Required]
        public string ReceiverId { get; set; }

        [ForeignKey(nameof(ReceiverId))]
        public virtual ApplicationUser Receiver { get; set; }
    }
}
