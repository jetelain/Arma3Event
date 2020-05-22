using System.ComponentModel.DataAnnotations;

namespace Arma3Event.Entities
{
    public enum VoipSystem
    {
        [Display(Name = "ACRE2 (TeamSpeak)")]
        ACRE2,

        [Display(Name = "TFAR (TeamSpeak)")]
        TFAR,

        [Display(Name = "TFAR Beta (TeamSpeak)")]
        TFARBeta,

        [Display(Name = "Intégré au jeu (VON)")]
        GameNative
    }
}