using System.ComponentModel.DataAnnotations;

namespace TP_CSharp.Models
{
    public class UpdateBlogViewModel
    {
        [Required]
        [MaxLength(50, ErrorMessage = "Le titre doit faire moins de 50 caractères")]
        public string? Title { get; set; }

        [Required]
        public string? Content { get; set; }

        [Display(Name = "Image")]
        [DataType(DataType.Upload)]
        public string? Image { get; set; }
    }
}
