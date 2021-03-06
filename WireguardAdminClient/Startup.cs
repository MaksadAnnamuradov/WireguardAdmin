using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WireguardAdminClient.Models;
using WireguardAdminClient.Services;

namespace WireguardAdminClient
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
            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddScoped<IWireguardService, WireguardService>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<WireguardService>();
            services.AddControllers();

            services.AddAuthentication("cookieAuth")
               .AddCookie(("cookieAuth"), options =>
               {
                   options.Cookie.Name = "cookieAuth";
                   options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
                   options.SlidingExpiration = true;
                   options.LoginPath = "/Account/Login";
                   options.AccessDeniedPath = "/Account/AccessDenied";
               }
               );

            services.AddAuthorization(options =>
            {
                options.AddPolicy("admin", policy => policy.RequireClaim("admin"));
            });
            /*  .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
              {
                  options.ClientId = "576944989227-8l94os8k65sltc8fspim0pcaqlt4kcua.apps.googleusercontent.com";
                  options.ClientSecret = "8PRENu0lnO9Ck-7INd81bg3l";
                  options.SignInScheme = IdentityConstants.ExternalScheme;
                  options.ClaimActions.MapJsonKey("urn:google:picture", "picture", "url");
              });*/

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
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();
            app.UseAuthentication();

            var cookiePolicyOptions = new CookiePolicyOptions
            {
                MinimumSameSitePolicy = SameSiteMode.Strict,
            };
            app.UseCookiePolicy(cookiePolicyOptions);


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Account}/{action=Index}/{id?}"
                    );
            });
        }
    }
}
