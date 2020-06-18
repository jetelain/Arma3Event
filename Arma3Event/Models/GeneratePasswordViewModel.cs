using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Arma3Event.Entities;

namespace Arma3Event.Models
{
    public class GeneratePasswordViewModel
    {
        [Display(Name = "Identifiant")]
        [Required]
        [MaxLength(128)]
        public string Login { get; set; }

        [Display(Name = "Mot de passe")]
        [Required]
        [MaxLength(128)]
        public string GeneratedPassword { get; set; }

        public User User { get; set; }
    }
}
