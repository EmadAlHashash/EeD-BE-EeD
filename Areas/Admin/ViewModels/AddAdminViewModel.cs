using System.ComponentModel.DataAnnotations;

namespace EeD_BE_EeD.Areas.Admin.ViewModels
{
    public class AddAdminViewModel
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = default!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;

        // كلمة السر للأدمن الجديد
        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = default!;

        // تأكيد كلمة السر
        [Required]
        [Compare(nameof(Password))]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = default!;

        // الصلاحيات: أدمن / سوبر أدمن
        [Required]
        public string Role { get; set; } = "Admin"; // Admin or SuperAdmin

        // الحالة: Active / Inactive / Banned
        [Required]
        public string Status { get; set; } = "Active"; // Active/Inactive/Banned

        public IFormFile? AvatarFile { get; set; }

    }
}
