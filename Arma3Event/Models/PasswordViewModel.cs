using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Arma3Event.Models
{
    public class PasswordViewModel
    {
        [Display(Name = "Identifiant")]
        [Required]
        [MaxLength(128)]
        public string Login { get; set; }

        [Display(Name = "Précédent mot de passe")]
        [MaxLength(128)]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [Display(Name = "Mot de passe")]
        [Required]
        [MaxLength(128)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Répéter le mot de passe")]
        [Required]
        [MaxLength(128)]
        [DataType(DataType.Password)]
        public string PasswordRepeat { get; set; }
        public bool NeedOldPassword { get; internal set; }
    }
}
