using EeD_BE_EeD.Models;
using System.Collections.Generic;

namespace EeD_BE_EeD.Areas.Admin.ViewModels
{
    public class ServiceManagementViewModel
    {
        // Services and Categories view models
        public List<ServiceVm> Services { get; set; } = new();
        public List<CategoryVm> Categories { get; set; } = new();

        // Filters
        public int? CategoryId { get; set; }
        public ServiceStatus? Status { get; set; }
        public string? City { get; set; }

        // Available lists for filters
        public List<string> AvailableCities { get; set; } = new();

        // Pagination for services
        public int ServicesPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int ServicesTotalCount { get; set; }

        // Pagination for categories
        public int CategoriesPage { get; set; } = 1;
        public int CategoriesPageSize { get; set; } = 10;
        public int CategoriesTotalCount { get; set; }
    }
}
