using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arma3Event.Entities;

namespace Arma3Event.Models
{
    public class NewsDetailsViewModel
    {
        public News News { get; set; }
        public News Next { get; internal set; }
        public News Previous { get; internal set; }
    }
}
