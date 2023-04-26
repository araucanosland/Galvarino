using Galvarino.Web.Data;
using Galvarino.Web.Data.Repository;
using Galvarino.Web.Models.Security;
using Galvarino.Web.Services;
using Galvarino.Web.Services.Notification;
using Galvarino.Web.Services.Workflow;
using Galvarino.Web.Workers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Rotativa.AspNetCore;
using System;

namespace Galvarino.Web
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

            services.Configure<IISOptions>(iis =>
            {
                iis.AuthenticationDisplayName = "Windows";
                iis.AutomaticAuthentication = true;
            });

            services.AddAuthentication(IISDefaults.AuthenticationScheme);

            services.AddDbContext<ApplicationDbContext>(options =>            
            {
                options.UseSqlServer(Configuration.GetConnectionString("DocumentManagementConnection"));
                options.EnableSensitiveDataLogging();
            });

            services.AddIdentity<Usuario, Rol>()
                 .AddEntityFrameworkStores<ApplicationDbContext>()
                 .AddDefaultTokenProviders();

            services.Configure<IISOptions>(options =>
            {
                options.ForwardClientCertificate = true;
                options.AutomaticAuthentication = true;
            });

 
            services.Configure<IdentityOptions>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;

                // User settings.
                options.User.RequireUniqueEmail = true;
            });


            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(20);

                options.LoginPath = "/";
                options.AccessDeniedPath = "/Home/SinPermiso";
                options.SlidingExpiration = true;
            });



            services.AddHttpContextAccessor();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();




            services.AddScoped<IUserClaimsPrincipalFactory<Usuario>, GalvarinoClaimsPrincipalFactory>();
            services.AddTransient<IWorkflowKernel, DefaultWorkflowKernel>();
            services.AddTransient<IWorkflowService, WorkflowService>();
            services.AddTransient<INotificationKernel, MailSender>();
            services.AddTransient<ISolicitudRepository, SolicitudesRepository>();
            services.AddScoped<IClaimsTransformation, CustomClaimsTransformer>();



            /*Workers & background tasks*/


            services.AddHostedService<CargaInicialWorker>();
            //services.AddHostedService<GeneraArchivoIronMountain>();
            //services.AddHostedService<GenerarReporteGestion>();


            // services.AddHostedService<CargaDatosCreditoService>();
            //services.AddHostedService<CierrePagaresDeIronMountainWorker>();
            // services.AddHostedService<CargaGalvarinoHistorico>();
            // services.AddHostedService<CargaGalvarinoVenta>();


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1).AddJsonOptions(ConfigureJson);
        }

        private void ConfigureJson(MvcJsonOptions obj)
        {
            obj.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAuthentication();

            //app.Run(async (context) =>
            //{
            //    await context.Response.WriteAsync(JsonConvert.SerializeObject(new
            //    {
            //        UserName = context?.User?.Identity?.Name
            //    }));
            //});

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
            RotativaConfiguration.Setup(env);
        }
    }
}
