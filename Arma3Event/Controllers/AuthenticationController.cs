using Arma3Event.Entities;
using Arma3Event.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Arma3Event.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly Arma3EventContext _context;

        public AuthenticationController(Arma3EventContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> SignIn(string ReturnUrl)
        {
            if (ReturnUrl != null && !ReturnUrl.StartsWith("/"))
            {
                return BadRequest();
            }
            var vm = new SignInViewModel();
            vm.ReturnUrl = ReturnUrl ?? "/";
            vm.Providers = await GetExternalProvidersAsync(HttpContext);

            if (ReturnUrl != null && ReturnUrl.StartsWith("/Events/Subscription/"))
            {
                var eventIdText = ReturnUrl.Substring(21).Split('?')[0];
                int eventId;
                if (int.TryParse(eventIdText, out eventId))
                {
                    vm.Event = await _context.Matchs.FindAsync(eventId);
                }
            }
            return View("SignIn", vm);
        }

        [HttpPost]
        public async Task<IActionResult> SignIn([FromForm] string provider, [FromForm] bool isPersistent, [FromForm] string ReturnUrl)
        {
            // Note: the "provider" parameter corresponds to the external
            // authentication provider choosen by the user agent.
            if (string.IsNullOrWhiteSpace(provider))
            {
                return BadRequest();
            }

            if (!await IsProviderSupportedAsync(HttpContext, provider))
            {
                return BadRequest();
            }

            if (!ReturnUrl.StartsWith("/"))
            {
                return BadRequest();
            }

            // Instruct the middleware corresponding to the requested external identity
            // provider to redirect the user agent to its own authorization endpoint.
            // Note: the authenticationScheme parameter must match the value configured in Startup.cs
            return Challenge(new AuthenticationProperties { RedirectUri = ReturnUrl, IsPersistent = isPersistent }, provider);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignInPassword([FromForm] string login, [FromForm] string password, [FromForm] bool isPersistent, [FromForm] string ReturnUrl)
        {
            if (!ReturnUrl.StartsWith("/"))
            {
                return BadRequest();
            }

            var user = await _context.UserLogins
                .Include(u => u.User)
                .FirstOrDefaultAsync(u => u.Login.ToLower() == login.ToLower());

            if (user != null && user.IsValidPassword(password))
            {
                var claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.Name, user.User.Name));
                claims.Add(new Claim(UserHelper.UserIdClaim, user.UserID.ToString()));
                claims.Add(new Claim(ClaimTypes.NameIdentifier, "https://github.com/jetelain/Arma3Event/id/" + user.UserID));

                var claimIdenties = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimPrincipal = new ClaimsPrincipal(claimIdenties);

                await Request.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimPrincipal, new AuthenticationProperties() { IsPersistent = isPersistent });

                return Redirect(ReturnUrl);
            }

            ModelState.AddModelError(string.Empty, "Nom d'utilisateur ou mot de passe incorrect.");

            return await SignIn(ReturnUrl);
        }

        /*private async Task SignInUser(string username, bool isPersistent)
        {
            // Initialization.  

            try
            {
                // Setting  
                claims.Add(new Claim(ClaimTypes.Name, username));
                var claimIdenties = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimPrincipal = new ClaimsPrincipal(claimIdenties);
                var authenticationManager = Request.HttpContext;

                // Sign In.  
                await authenticationManager.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimPrincipal, new AuthenticationProperties() { IsPersistent = isPersistent });
            }
            catch (Exception ex)
            {
                // Info  
                throw ex;
            }
        }*/





        [HttpGet, HttpPost]
        public IActionResult SignOut()
        {
            // Instruct the cookies middleware to delete the local cookie created
            // when the user agent is redirected from the external identity provider
            // after a successful authentication flow (e.g Google or Facebook).
            return SignOut(new AuthenticationProperties { RedirectUri = "/" },
                CookieAuthenticationDefaults.AuthenticationScheme);
        }
        public static async Task<AuthenticationScheme[]> GetExternalProvidersAsync(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var schemes = context.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();

            return (from scheme in await schemes.GetAllSchemesAsync()
                    where !string.IsNullOrEmpty(scheme.DisplayName)
                    select scheme).ToArray();
        }

        public static async Task<bool> IsProviderSupportedAsync(HttpContext context, string provider)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return (from scheme in await GetExternalProvidersAsync(context)
                    where string.Equals(scheme.Name, provider, StringComparison.OrdinalIgnoreCase)
                    select scheme).Any();
        }

        public IActionResult Denied() => View("Denied");

    }
}
