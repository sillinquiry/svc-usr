using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using svc_usr.Data;
using svc_usr.Models;
using svc_usr.Models.Identity;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.IdentityModel.Tokens;
using svc_usr.Settings;

namespace svc_usr
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
            services.AddMvc();

            services.Configure<Services>(Configuration.GetSection("Services"));

            services.AddDbContext<UsrDbContext>(options => {
                options.UseInMemoryDatabase(nameof(UsrDbContext));
                options.UseOpenIddict();
            });

            services.AddIdentity<Usr, Role>()
                .AddEntityFrameworkStores<UsrDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options => {
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false;
                options.ClaimsIdentity.UserNameClaimType = OpenIdConnectConstants.Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = OpenIdConnectConstants.Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = OpenIdConnectConstants.Claims.Role;
            });
            
            services.AddOpenIddict(options => {
                options.AddEntityFrameworkCoreStores<UsrDbContext>();
                options.AddMvcBinders();
                options.EnableTokenEndpoint("/api/auth/token");
                options.EnableUserinfoEndpoint("/api/usrinfo");
                options.AllowPasswordFlow();
                options.AllowRefreshTokenFlow();
                options.SetAccessTokenLifetime(TimeSpan.FromSeconds(3600));
                options.SetRefreshTokenLifetime(TimeSpan.FromDays(14));
                options.DisableHttpsRequirement();
                options.UseJsonWebTokens();
                options.AddEphemeralSigningKey();
            });

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

            services.AddAuthentication(options => {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options => {
                options.Authority = Configuration.GetSection("Services").GetValue("Identity", String.Empty);
                options.Audience = "resource-server";
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters {
                    NameClaimType = OpenIdConnectConstants.Claims.Subject,
                    RoleClaimType = OpenIdConnectConstants.Claims.Role
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();

            app.UseMvc();
        }
    }
}
