using System.ComponentModel.DataAnnotations;

namespace TP_CSharp.Models
{
    public class LoginViewModel
    {
        [Display(Name = "Adresse email")]
        [Required(ErrorMessage = "L'adresse email est obligatoire")]
        [EmailAddress(ErrorMessage = "Adresse email invalide")]

        public string Email { get; set; }

        [Required(ErrorMessage = "Le mot de passe est obligatoire")]
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe")]
        [MinLength(6, ErrorMessage = "Le mot de passe doit faire au moins 6 caractères")]
        public string Password { get; set; }
    }
}
