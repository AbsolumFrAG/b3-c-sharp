using System.ComponentModel.DataAnnotations;

namespace TP_CSharp.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        [StringLength(50)]
        public string? FullName { get; set; }

        [Required]
        [StringLength(35)]
        public string Email { get; set; }

        [Required]
        [StringLength(100)]
        public string Password { get; set; }

        public bool Locked { get; set; } = false;

        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(10)]
        [Required]
        public string Role { get; set; } = "user";

        [DataType(DataType.Upload)]
        public string? ProfileImage { get; set; }
    }
}
