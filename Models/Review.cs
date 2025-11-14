using Microsoft.CodeAnalysis.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EeD_BE_EeD.Models
{
    public class Review
    {
        [Key]

        public int Id { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; } // من 1 إلى 5 نجوم

        [StringLength(1000)]
        public string? Comment { get; set; }

        public DateTime DatePosted { get; set; } = DateTime.UtcNow;

        // ✅ المراجع (الذي كتب التقييم)
        [Required]
        public string ReviewerId { get; set; }

        [ForeignKey(nameof(ReviewerId))]
        public virtual ApplicationUser Reviewer { get; set; }

        // ✅ الشخص الذي يتم تقييمه
        [Required]
        public string ReviewedUserId { get; set; }

        [ForeignKey(nameof(ReviewedUserId))]
        public virtual ApplicationUser ReviewedUser { get; set; }

        // ✅ ربط التقييم مع عملية تبادل (اختياري)
        public int? ExchangeId { get; set; }

        [ForeignKey(nameof(ExchangeId))]
        public virtual Exchange? Exchange { get; set; }
        public DateTime CreatedAt { get; internal set; }
    }
}
