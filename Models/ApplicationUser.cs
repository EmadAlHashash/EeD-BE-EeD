using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace EeD_BE_EeD.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [Url]
        public string? ProfilePictureUrl { get; set; }

        public string? Bio { get; set; }

        public bool IsBanned { get; set; } = false;
        public DateTime? LastLoginAt { get; set; }

        public string? Country { get; set; }
        public string Status { get; set; } = "Active";

        //✅ العلاقات

        //خدمات المستخدم
        public virtual ICollection<Service>? OfferedServices { get; set; } = new List<Service>();

        // تقييمات كتبها المستخدم
        [InverseProperty("Reviewer")]
        public virtual ICollection<Review>? ReviewsWritten { get; set; } = new List<Review>();

        // تقييمات تلقاها المستخدم
        [InverseProperty("ReviewedUser")]
        public virtual ICollection<Review>? ReviewsReceived { get; set; } = new List<Review>();

        // رسائل أرسلها
        [InverseProperty("Sender")]
        public virtual ICollection<Message>? MessagesSent { get; set; } = new List<Message>();

        // رسائل استلمها
        [InverseProperty("Receiver")]
        public virtual ICollection<Message>? MessagesReceived { get; set; } = new List<Message>();

        // شهادات كتبها
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public virtual ICollection<Testimonial> Testimonials { get; set; } = new List<Testimonial>();

        public ICollection<ActivityLog>? ActivityLog { get; set; }


    }
}
