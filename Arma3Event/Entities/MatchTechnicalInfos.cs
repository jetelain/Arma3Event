using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Arma3Event.Entities
{
    public class MatchTechnicalInfos
    {
        public int MatchTechnicalInfosID { get; set; }

        public int MatchID { get; set; }

        public Match Match { get; set; }

        [Display(Name = "Adresse du serveur Arma 3")]
        public string GameServerAddress { get; set; }

        [Display(Name = "Port du serveur Arma 3")]
        public int? GameServerPort { get; set; }

        [Display(Name = "Mot de passe de serveur Arma 3")]
        public string GameServerPassword { get; set; }

        [Display(Name = "Système de communication vocale en jeu")]
        public VoipSystem VoipSystem { get; set; }

        [Display(Name = "Adresse du serveur TeamSpeak")]
        public string VoipServerAddress { get; set; }

        [Display(Name = "Port du serveur TeamSpeak")]
        public int? VoipServerPort { get; set; }

        [Display(Name = "Mot de passe de serveur TeamSpeak")]
        public string VoipServerPassword { get; set; }

        [Display(Name = "Nombre de mods")]
        public int ModsCount { get; set; }

        [Display(Name = "Définition du ModPack")]
        [DataType(DataType.MultilineText)]
        public string ModsDefinition { get; set; }

        [Display(Name = "Dernière mise à jour du ModPack")]
        public DateTime? ModsLastChange { get; set; }

        [Display(Name = "Masque les mots de passe jusqu'au nombre d'heure précisé avant le début")]
        public int? HoursBeforeRevealPasswords { get; set; }
    }

}
