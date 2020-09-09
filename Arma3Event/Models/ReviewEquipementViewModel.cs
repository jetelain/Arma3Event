using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arma3Event.Entities;
using Arma3Event.Services.ArmaPersist;

namespace Arma3Event.Models
{
    public class ReviewEquipementViewModel
    {
        public Match Match { get; internal set; }
        public RoundSquad Squad { get; internal set; }
        public PersistBackup Backup { get; internal set; }
    }
}
