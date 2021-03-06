using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Text;
using System.Threading.Tasks;
using WireguardAdmin.Models;
using WireguardAdmin.Services;
using WireguardAdmin.Infrastructure;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

var jwtSettings = new JwtSetting();
builder.Configuration.Bind("JwtSettings", jwtSettings);

builder.Services.AddDbContext<AdminDBContext>(options => options.UseNpgsql(builder.Configuration["DATABASE_URL"]));
builder.Services.AddTransient<IAdminRepository, AdminRepository>();
builder.Services.AddTransient<IWireguardService, WireguardService>();
builder.Services.AddTransient<JwtTokenCreator>();
builder.Services.AddSingleton(jwtSettings);
builder.Services.AddControllers();

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

//For Identity
builder.Services.AddIdentity<WireguardUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = false;
})
    .AddEntityFrameworkStores<AdminDBContext>()
    .AddDefaultTokenProviders();



builder.Services.AddAuthentication(options =>
{
    /*options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;*/
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
})
 .AddJwtBearer(options =>
 {
     options.TokenValidationParameters = new TokenValidationParameters
     {
         ValidateIssuer = true,
         ValidateAudience = true,
         ValidateLifetime = true,
         ValidateIssuerSigningKey = true,
         ValidIssuer = jwtSettings.Issuer,
         ValidAudience = jwtSettings.Audience,
         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
         ClockSkew = jwtSettings.Expire
     };
     options.SaveToken = true;
     options.Events = new JwtBearerEvents();
     options.Events.OnMessageReceived = context => {

         if (context.Request.Cookies.ContainsKey("X-Access-Token"))
         {
             context.Token = context.Request.Cookies["X-Access-Token"];
         }

         return Task.CompletedTask;
     };
 })
.AddCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.IsEssential = true;
})

.AddGoogle(g =>
{
    g.ClientId = "576944989227-8l94os8k65sltc8fspim0pcaqlt4kcua.apps.googleusercontent.com";
    g.ClientSecret = "8PRENu0lnO9Ck-7INd81bg3l";
    g.SaveTokens = true;
})
 .AddOpenIdConnect("Auth0", options =>
 {
     options.Authority = $"https://{builder.Configuration["Auth0:Domain"]}";
     options.ClientId = builder.Configuration["Auth0:ClientId"];
     options.ClientSecret = builder.Configuration["Auth0:ClientSecret"];
     options.SaveTokens = true;
     options.ResponseType = OpenIdConnectResponseType.Code;
     options.CallbackPath = new PathString("/callback");
     options.TokenValidationParameters = new TokenValidationParameters
     {
         NameClaimType = "name"
     };
     options.Events = new OpenIdConnectEvents
     {
         OnRedirectToIdentityProviderForSignOut = (context) =>
         {
             var logoutUri = $"https://{builder.Configuration["Auth0:Domain"]}/v2/logout?client_id={builder.Configuration["Auth0:ClientId"]}";

             var postLogoutUri = context.Properties.RedirectUri;
             if (!string.IsNullOrEmpty(postLogoutUri))
             {
                 if (postLogoutUri.StartsWith("/"))
                 {
                     // transform to absolute
                     var request = context.Request;
                     postLogoutUri = request.Scheme + "://" + request.Host + request.PathBase + postLogoutUri;
                 }
                 logoutUri += $"&returnTo={ Uri.EscapeDataString(postLogoutUri)}";
             }

             context.Response.Redirect(logoutUri);
             context.HandleResponse();

             return Task.CompletedTask;
         },
        OnRedirectToIdentityProvider = context =>
        {
            context.ProtocolMessage.SetParameter("audience", builder.Configuration["Auth0:ApiAudience"]);
            return Task.FromResult(0);
        }

     };
 });

builder.Services.AddEndpointsApiExplorer();

// Enable Swagger   
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "JCMFitnessPostgresAPI", Version = "v1" });


    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "JWT Authentication",
        Description = "Enter JWT Bearer token **_only_**",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer", // must be lower case
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };
    c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {securityScheme, new string[] { }}
                });

    // add Basic Authentication
    var basicSecurityScheme = new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "basic",
        Reference = new OpenApiReference { Id = "BasicAuth", Type = ReferenceType.SecurityScheme }
    };

    c.AddSecurityDefinition(basicSecurityScheme.Reference.Id, basicSecurityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {basicSecurityScheme, new string[] { }}
                });
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AdminDBContext>();
    context.Database.Migrate();
    // context.Database.EnsureCreated();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
