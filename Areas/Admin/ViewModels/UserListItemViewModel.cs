namespace EeD_BE_EeD.Areas.Admin.ViewModels
{
    public class UserListItemViewModel
    {
        public string Id { get; set; } = default!;
        // المعرف الأساسي للمستخدم — نحتاجه لعمليات (Edit / Delete / Ban / Details)

        public string FullName { get; set; } = default!;
        // الاسم الكامل اللي راح ينعرض داخل الجدول

        public string Email { get; set; } = default!;
        // الإيميل — عرض رئيسي

        public string Role { get; set; } = default!;
        // الدور: Admin / SuperAdmin / User

        public string Status { get; set; } = default!;
        // Active / Inactive / Banned

        public string? AvatarUrl { get; set; }
        // صورة المستخدم — يمكن تكون null
    }
}
