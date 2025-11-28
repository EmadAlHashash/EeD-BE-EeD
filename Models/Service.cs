using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EeD_BE_EeD.Models
{
    public enum ServiceStatus
    {
        Pending = 0,
        Active = 1,
        Removed = 2
    }
    public class Service
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; }

        public ServiceStatus Status { get; set; } = ServiceStatus.Pending;

        // ✅ الخدمة المطلوبة مقابل هذه الخدمة
        [StringLength(200)]
        public string? DesiredSkillOrService { get; set; }

        // Indicates if the service is active (soft toggle separate from Status)
        public bool IsActive { get; set; } = true;

        // Created timestamp
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ✅ علاقة المالك
        [Required]
        public string OwnerId { get; set; }

        [ForeignKey(nameof(OwnerId))]
        public virtual ApplicationUser Owner { get; set; }

        // ✅ فئة الخدمة
        public int CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public virtual Category Category { get; set; }
    }
}
