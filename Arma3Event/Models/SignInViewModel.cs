using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arma3Event.Entities;
using Microsoft.AspNetCore.Authentication;

namespace Arma3Event.Models
{
    public class SignInViewModel
    {
        public string ReturnUrl { get; set; }
        public AuthenticationScheme[] Providers { get; set; }
        public Match Event { get; internal set; }
    }
}
