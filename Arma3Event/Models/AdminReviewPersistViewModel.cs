using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arma3Event.Entities;
using Arma3Event.Services.ArmaPersist;

namespace Arma3Event.Models
{
    public class AdminReviewPersistViewModel
    {
        public List<PersistBackup> Backups { get; internal set; }
        public List<Match> Matchs { get; internal set; }
    }
}
