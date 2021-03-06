﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Arma3TacMapLibrary.Arma3;
using Arma3TacMapLibrary.TacMaps;

namespace Arma3Event.Entities
{
    public class Match
    {
        public int MatchID { get; set; }

        [Display(Name = "Titre")]
        public string Name { get; set; }

        [Display(Name = "Image")]
        public string Image { get; set; }

        [DataType(DataType.Date)]
        [Required]
        [Display(Name = "Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        [NotMapped]
        public DateTime Date { get; set; }

        [DataType(DataType.Time)]
        [Required]
        [Display(Name = "Heure de début")]
        [NotMapped]
        public DateTime StartTime { get; set; }

        [Display(Name = "Description")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Display(Name = "Description résumée")]
        public string ShortDescription { get; set; }

        [Display(Name = "Date et heure de début")]
        public DateTime StartDate
        {
            get { return new DateTime(Date.Year, Date.Month, Date.Day, StartTime.Hour, StartTime.Minute, StartTime.Second); }
            set
            {
                Date = value.Date;
                StartTime = value;
            }
        }

        [Display(Name = "Accès sur invitation uniquement")]
        public bool InviteOnly { get; set; }

        [Display(Name = "Type d'opération")]
        public MatchTemplate Template { get; set; }

        public List<Round> Rounds { get; set; }
        public List<MatchSide> Sides { get; set; }
        public List<MatchUser> Users { get; set; }
        public List<News> News { get; set; }
        public List<Video> Videos { get; set; }

        [Display(Name = "Carte")]
        [Obsolete]
        public int? GameMapID { get; set; }

        [Display(Name = "Carte")]
        [Obsolete]
        public GameMap GameMap { get; set; }

        [Display(Name = "Carte")]
        public string WorldName { get; set; }


        [Display(Name = "Lien Discord")]
        public string DiscordLink { get; set; }

        [Display(Name = "Lien vers le réglement")]
        public string RulesLink { get; set; }

        public MatchTechnicalInfos MatchTechnicalInfos { get; set; }

        [Display(Name = "Etat")]
        public MatchState State { get; set; }

        [Display(Name = "Lien vers le Mission Brief")]
        public string MissionBriefLink { get; set; }

        public List<Document> Documents { get; set; }

        public int? TacMapId { get; set; }

        [NotMapped]
        public ApiTacMap TacMap { get; set; }

        [NotMapped]
        public MapInfos MapInfos { get; set; }
    }
}
