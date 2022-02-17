using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WireguardAdmin.Models;

namespace WireguardAdmin
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
            services.AddDbContext<AdminDBContext>(options => options.UseNpgsql(Configuration["DATABASE_URL"]));
            services.AddScoped<IAdminRepository, AdminRepository>();
            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddSession();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
               .AddCookie(x => x.LoginPath = "/Account/Login");

            services.Configure<WireguardAdminOptions>(Configuration.GetSection(WireguardAdminOptions.WireguardAdmin));
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


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();

                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllerRoute(
                        name: "default",
                        pattern: "{controller=Account}/{action=Index}/{id?}"
                        );
                });


                /*endpoints.MapControllerRoute(
                    name: "Default",
                    pattern: "Login",
                    defaults: new { controller = "Account", action = "Login" });

                endpoints.MapControllerRoute(
                 name: "Success",
                 pattern: "Success",
                 defaults: new { controller = "Account", action = "Success" });

                endpoints.MapControllerRoute(
                 name: "AddNewClient",
                 pattern: "AddNewClient",
                 defaults: new { controller = "Account", action = "AddNewClient" });
                endpoints.MapControllerRoute(
                 name: "AddNewUser",
                 pattern: "AddNewUser",
                 defaults: new { controller = "Account", action = "AddNewUser" });*/

                /*  endpoints.MapControllerRoute(
                   name: "Login",
                   pattern: "Login",
                   defaults: new { controller = "Account", action = "Login" });*/
            });

        }
    }
}
