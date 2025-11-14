using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EeD_BE_EeD.Models
{
    public class ActivityLog
    {
        [Key]                          // PK
        public int Id { get; set; }

        [MaxLength(450)]
        public string? UserId { get; set; }  // nullable: ???? ???? ?????? ???? ??????

        [Required]
        [MaxLength(100)]
        public string Action { get; set; } = default!;  // ?????: "Register","Login","CreateService","Review","Exchange"

        [MaxLength(2000)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }
    }
}
