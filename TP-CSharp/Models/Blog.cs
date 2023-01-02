using System.ComponentModel.DataAnnotations;

namespace TP_CSharp.Models
{
    public class Blog
    {
        [Key]
        public int BlogId { get; set; }

        [Display(Name = "Titre")]
        [Required]
        [MaxLength(50, ErrorMessage = "Le titre doit contenir max 50 caractères")]
        public string? Title { get; set; }

        [Required]
        public string? Content { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Image")]
        [DataType(DataType.Upload)]
        public string? Image { get; set; }
        
        public List<Comment>? Comments { get; set; }

        public Guid? UserId { get; set; }

        public User? User { get; set; }
    }
}
