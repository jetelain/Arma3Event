﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Arma3Event.Entities
{
    public class User
    {
        public int UserID { get; set; }

        [StringLength(100)]
        [Display(Name = "Nom dans Arma 3")]
        public string Name { get; set; }

        [StringLength(50)]
        [Display(Name = "Identifiant Steam")]
        public string SteamId { get; set; }

        [StringLength(100)]
        [Display(Name = "Nom dans steam")]
        public string SteamName { get; set; }

        [Display(Name = "Paramètres de confidentialité")]
        public UserPrivacyOptions PrivacyOptions { get; set; }

        public UserLogin ManualLogin { get; set; }

        public List<MatchUser> Matchs { get; set; }
    }
}