using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EeD_BE_EeD.Models
{
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

        public bool IsActive { get; set; } = true;

        // ✅ الخدمة المطلوبة مقابل هذه الخدمة
        [StringLength(200)]
        public string? DesiredSkillOrService { get; set; }

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
