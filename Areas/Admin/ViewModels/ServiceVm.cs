using EeD_BE_EeD.Models;

namespace EeD_BE_EeD.Areas.Admin.ViewModels
{
    public class ServiceVm
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? ExchangeSkill { get; set; }
        public DateTime CreatedAt { get; set; }
        public ServiceStatus Status { get; set; }
    }
}
