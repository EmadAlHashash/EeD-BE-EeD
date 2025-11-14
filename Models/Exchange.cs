using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EeD_BE_EeD.Models
{
    public enum ExchangeStatus
    {
        Pending,
        Approved,
        Completed,
        Cancelled
    }
    public class Exchange
    {
        [Key]

        public int Id { get; set; }

        // ✅ المستخدم الذي طلب الخدمة
        [Required]
        public string RequesterId { get; set; }

        [ForeignKey(nameof(RequesterId))]
        public virtual ApplicationUser Requester { get; set; }

        // ✅ الخدمة المطلوبة
        [Required]
        public int RequestedServiceId { get; set; }

        [ForeignKey(nameof(RequestedServiceId))]
        public virtual Service RequestedService { get; set; }

        // ✅ المستخدم الذي سيقدم الخدمة
        [Required]
        public string ProviderId { get; set; }

        [ForeignKey(nameof(ProviderId))]
        public virtual ApplicationUser Provider { get; set; }

        // ✅ الخدمة المقابلة
        [Required]
        public int OfferedServiceId { get; set; }

        [ForeignKey(nameof(OfferedServiceId))]
        public virtual Service OfferedService { get; set; }

        public ExchangeStatus Status { get; set; } = ExchangeStatus.Pending;

        public DateTime RequestDate { get; set; } = DateTime.UtcNow;
        public DateTime? CompletionDate { get; set; }
    }
}
