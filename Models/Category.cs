using System.ComponentModel.DataAnnotations;

namespace EeD_BE_EeD.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(300)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        // ✅ علاقة مع الخدمات
        public virtual ICollection<Service>? Services { get; set; } = new List<Service>();
    }
}
