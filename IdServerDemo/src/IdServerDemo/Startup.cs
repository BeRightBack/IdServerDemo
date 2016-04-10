using System;
using AspNet.Security.OpenIdConnect.Extensions;
using Microsoft.AspNet.Authentication.JwtBearer;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using cloudscribe.Web.Pagination;
using IdServerDemo.Extensions;
using IdServerDemo.Models;
using IdServerDemo.Providers;
using IdServerDemo.Services;



namespace IdServerDemo
{
    public class Startup
    {
        public Startup()
        {

            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json");

            Configuration = builder.Build();
        }

        public static IConfigurationRoot Configuration { get; set; }


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    builder =>
                    {
                        builder.AllowAnyOrigin();
                    });
            });//Wide open for test purposes but sould be more restrictive


            services.AddEntityFramework()
                .AddSqlServer()
                .AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(Configuration["Data:DefaultConnection:ConnectionString"]));

            services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationDbContext, int>()
                .AddUserStore<UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, int>>()
                .AddRoleStore<RoleStore<ApplicationRole, ApplicationDbContext, int>>()
                .AddDefaultTokenProviders();


            services.AddAuthorization(options =>
            {
                // Add a new policy requiring a "scope" claim
                // containing the "api-resource-controller" value.
                options.AddPolicy("API", policy =>
                {
                    policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireClaim(OpenIdConnectConstants.Claims.Scope, "api-resource-controller");
                });
                options.AddPolicy("RequireAdministratorRole", policy => policy.RequireRole("Administrators"));
            });

            services.AddCaching();

            services.AddMvc();

            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(new RequireHttpsAttribute());
            });

            // Add application services.
            services.TryAddTransient<IBuildPaginationLinks, PaginationLinkBuilder>();
            services.AddTransient<SeedData>();
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async void Configure(IApplicationBuilder app, SeedData seeder)
        {
            app.UseExceptionHandler("/Home/Error");
            try
            {
                using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
                    .CreateScope())
                {
                    serviceScope.ServiceProvider.GetService<ApplicationDbContext>()
                         .Database.Migrate();
                }
            }
            catch { }

            app.UseCors("AllowAllOrigins");

            app.UseIISPlatformHandler(options => options.AuthenticationDescriptions.Clear());

            app.UseStaticFiles();

            app.UseIdentity();

            // Create a new branch where the registered middleware will be executed only for API calls.
            app.UseWhen(context => context.Request.Path.StartsWithSegments(new PathString("/api")), branch =>
            {
                branch.UseJwtBearerAuthentication(options =>
                {
                    options.AutomaticAuthenticate = true;
                    options.AutomaticChallenge = true;
                    options.RequireHttpsMetadata = true;

                    options.Audience = Configuration["Audience"];
                    options.Authority = Configuration["Authority"];
                });
            });

            // Create a new branch where the registered middleware will be executed only for non API calls.
            app.UseWhen(context => !context.Request.Path.StartsWithSegments(new PathString("/api")), branch =>
            {

                branch.UseCookieAuthentication(options =>
                {
                    options.AutomaticAuthenticate = true;
                    options.AutomaticChallenge = true;
                    options.ExpireTimeSpan = TimeSpan.FromHours(12);
                    options.LoginPath = new PathString("/Authentication/Login");
                });

            });


            app.UseOpenIdConnectServer(options =>
            {

                options.Provider = new AuthorizationProvider();
                options.ApplicationCanDisplayErrors = true;
                options.AllowInsecureHttp = false;
                options.AccessTokenLifetime = TimeSpan.FromHours(12);

                // Note: by default, tokens are signed using dynamically-generated
                // RSA keys but you can also use your own certificate:
                // options.SigningCredentials.AddCertificate(certificate);
            });

            app.UseFacebookAuthentication(options =>
            {
                options.AppId = "1285424324804833";
                options.AppSecret = "0e06a3d356893febc1da43f1640cc0c1";
            });
            app.UseGoogleAuthentication(options =>
            {
                options.ClientId = "79357097135-03qn70cpsf94ci5phsshanho04pov108.apps.googleusercontent.com";
                options.ClientSecret = "x7Xhzs3urTnDKlgcoV8dJ6TD";
            });

            app.UseTwitterAuthentication(options =>
            {
                options.ConsumerKey = "YourConsumerKeyHere";
                options.ConsumerSecret = "YourConsumerSecretHere";
            });

            app.UseMicrosoftAccountAuthentication(options =>
            {
                options.ClientId = "YourClientIdHere";
                options.ClientSecret = "YourClientSecretHere";
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            await seeder.EnsureSeedDataAsync();
        }

        // Entry point for the application.
        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
    
}
