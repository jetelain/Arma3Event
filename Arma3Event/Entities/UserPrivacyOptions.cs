using System.ComponentModel.DataAnnotations;

namespace Arma3Event.Entities
{
    public enum UserPrivacyOptions
    {
        [Display(Name = "Masquer le lien vers le profil Steam aux autres utilisateurs ")]
        Private,

        [Display(Name = "Afficher le lien vers le profil Steam aux autres utilisateurs")]
        SteamProfilPublic
    }
}