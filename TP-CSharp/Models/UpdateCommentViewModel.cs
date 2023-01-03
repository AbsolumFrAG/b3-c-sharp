using System.ComponentModel.DataAnnotations;

namespace TP_CSharp.Models
{
    public class UpdateCommentViewModel
    {
        [Required(ErrorMessage = "Le contenu est requis")]
        public string Content { get; set; }
    }
}
