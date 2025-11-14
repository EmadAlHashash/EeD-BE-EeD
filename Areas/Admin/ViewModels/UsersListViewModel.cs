namespace EeD_BE_EeD.Areas.Admin.ViewModels
{
    public class UsersListViewModel
    {
        public List<UserListItemViewModel> Users { get; set; } = new();
        // قائمة المستخدمين اللي راح تنعرض في الجدول

        public string? SearchQuery { get; set; }
        // لو بدك تضيف بحث باسم المستخدم أو الإيميل

        public string? RoleFilter { get; set; }
        // فلتر حسب الدور

        public string? StatusFilter { get; set; }
        // فلتر حسب Active / Inactive / Banned

        public int CurrentPage { get; set; }
        // الصفحة الحالية في Pagination

        public int TotalPages { get; set; }
        // عدد الصفحات الكلي

        public int PageSize { get; set; } = 10;
        // عدد العناصر بكل صفحة
    }
}
