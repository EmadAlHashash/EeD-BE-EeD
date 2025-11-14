using System.ComponentModel.DataAnnotations;

namespace EeD_BE_EeD.Areas.Admin.ViewModels
{
    public class EditUserViewModel
    {
        [Required]
        public string Id { get; set; } = default!;

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = default!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;

        [Required]
        public string Role { get; set; } = "User"; // Admin/User/SuperAdmin

        [Required]
        public string Status { get; set; } = "Active"; // Active/Inactive/Banned

        public IFormFile? AvatarFile { get; set; }
    }
}
