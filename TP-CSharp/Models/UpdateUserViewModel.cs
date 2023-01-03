using System.ComponentModel.DataAnnotations;

namespace TP_CSharp.Models
{
    public class UpdateUserViewModel
    {
        [StringLength(50)]
        public string? FullName { get; set; }

        [Required]
        [StringLength(35)]
        public string Email { get; set; }

        [StringLength(10)]
        [Required]
        public string Role { get; set; } = "user";

        [DataType(DataType.Upload)]
        public string? ProfileImage { get; set; }
    }
}
