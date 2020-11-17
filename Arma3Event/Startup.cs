using System;
using System.IO;
using System.Linq;
using Arma3Event.Entities;
using Arma3Event.Hubs;
using Arma3Event.Services;
using Arma3ServerToolbox.ArmaPersist;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Arma3Event
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<PersistService>();

            services.AddHttpClient();
            services.AddControllersWithViews();
            services.AddSignalR();

            services.AddDbContext<Arma3EventContext>(options =>
                options.UseSqlite(Configuration.GetConnectionString("Arma3EventContext")));

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.LoginPath = "/Authentication/SignIn";
                options.LogoutPath = "/Authentication/SignOut";
                options.AccessDeniedPath = "/Authentication/Denied";
            })
            .AddSteam(s => s.ApplicationKey = Configuration.GetValue<string>("Steam:Key"));

            services.AddAuthorization(options =>
            {
                var admins = Configuration.GetSection("Admins").Get<string[]>();
                var videoSpecialists = Configuration.GetSection("VideoSpecialists").Get<string[]>() ?? new string[0];

                options.AddPolicy("Admin", policy => policy.RequireClaim(
                    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
                    admins.ToArray()
                    ));
                options.AddPolicy("VideoSpecialist", policy => policy.RequireClaim(
                    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
                    admins.Concat(videoSpecialists).ToArray()
                    ));
                options.AddPolicy("LoggedUser", policy => policy.RequireClaim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"));
            }); 
            
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                services.AddDataProtection()
                    .PersistKeysToFileSystem(new DirectoryInfo(Configuration.GetValue<string>("UnixKeysDirectory")))
                    .SetApplicationName("Arma3Event");
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedProto
                });
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseRequestLocalization("fr-FR");

            app.UseCookiePolicy(new CookiePolicyOptions()
            {
                HttpOnly = HttpOnlyPolicy.Always,
                Secure = CookieSecurePolicy.SameAsRequest,
                MinimumSameSitePolicy = SameSiteMode.Lax
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapHub<MapHub>("/MapHub");
            });
        }
    }
}
