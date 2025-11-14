using System.ComponentModel.DataAnnotations;

namespace EeD_BE_EeD.Models
{
    public class HeroSection
    {
        [Key]

        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; }

        [Required]
        [StringLength(500)]
        public string Subtitle { get; set; }

        [Url]
        [StringLength(500)]
        public string? BackgroundImageUrl { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
