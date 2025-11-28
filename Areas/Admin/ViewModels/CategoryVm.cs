namespace EeD_BE_EeD.Areas.Admin.ViewModels
{
    public class CategoryVm
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
}
