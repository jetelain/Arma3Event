﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Arma3Event.Arma3GameInfos
{
    public enum GameMarkerColor
    {
        [Display(Name = "Noir")]
        ColorBlack,
        [Display(Name = "Gris")]
        ColorGrey,
        [Display(Name = "Rouge")]
        ColorRed,
        [Display(Name = "Marron")]
        ColorBrown,
        [Display(Name = "Orange")]
        ColorOrange,
        [Display(Name = "Jaune")]
        ColorYellow,
        [Display(Name = "Khaki")]
        ColorKhaki,
        [Display(Name = "Vert")]
        ColorGreen,
        [Display(Name = "Bleu")]
        ColorBlue,
        [Display(Name = "Rose")]
        ColorPink,
        [Display(Name = "Blanc")]
        ColorWhite,
        [Display(Name = "Inconnu")]
        ColorUNKNOWN,
        [Display(Name = "BLUFOR")]
        colorBLUFOR,
        [Display(Name = "OPFOR")]
        colorOPFOR,
        [Display(Name = "Indépendent")]
        colorIndependent,
        [Display(Name = "Civil")]
        colorCivilian
    }
}
