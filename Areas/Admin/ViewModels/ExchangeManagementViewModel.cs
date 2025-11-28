namespace EeD_BE_EeD.Areas.Admin.ViewModels
{
    public class ExchangeManagementViewModel
    {
        public List<ExchangeVm> Exchanges { get; set; } = new();

        // Filters
        public string? Status { get; set; }
        public string? UserName { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        // For dropdowns
        public List<string> AvailableStatuses { get; set; } = new();

        // Stats
        public int TotalExchanges { get; set; }
        public int PendingCount { get; set; }
        public int CompletedCount { get; set; }
        public int CancelledCount { get; set; }
    }

    public class ExchangeVm
    {
        public int Id { get; set; }
        public string RequesterName { get; set; }
        public string ProviderName { get; set; }
        public string RequestedServiceTitle { get; set; }
        public string OfferedServiceTitle { get; set; }
        public string Status { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? CompletionDate { get; set; }

        public string RequestDateFormatted => RequestDate.ToString("yyyy-MM-dd");
        public string CompletionDateFormatted => CompletionDate?.ToString("yyyy-MM-dd") ?? "-";
    }
}
