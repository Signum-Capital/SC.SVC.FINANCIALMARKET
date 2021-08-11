using KissLog;
using KissLog.AspNetCore;
using KissLog.CloudListeners.Auth;
using KissLog.CloudListeners.RequestLogsListener;

using Infra.Dependencies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SC.FINANCIALMARKET.DOMAIN.Configuration;
using SC.FINANCIALMARKET.DOMAIN.Hubs;
using SC.INFRA.INFRAESTRUCTURE.Contexts;
using SC.PKG.SERVICES.Filters;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.Text;
using System;
using KissLog.Web;
using System.Linq;

namespace SC.FINANCIALMARKET.API
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
            services.AddControllers();
            services.ResolveDependenciesRepository();
            services.AddDbContext<FinancialMarketDataContext>();

            services.AddControllersWithViews(options => options.Filters.Add<PlataformAuthorizationFilter>());

            services.SwaggerConfigure();
            services.AddSignalR(o =>
            {
                o.EnableDetailedErrors = true;
                o.MaximumReceiveMessageSize = 256;
            });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<ILogger>((context) =>
            {
                return Logger.Factory.Get();
            });

            services.AddLogging(logging =>
            {
                logging.AddKissLog();
            });

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder
                            .AllowCredentials()
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .SetIsOriginAllowedToAllowWildcardSubdomains()
                            .WithOrigins("https://*.signumcapital.net", "https://localhost:44301");
                    });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "SC.FINANCIALMARKET.API v1");
                c.RoutePrefix = "docs";
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();
            app.UseAuthentication();

            app.UseCors();

            app.UseKissLogMiddleware(options => {
                ConfigureKissLog(options);
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<CatalogadorHub>("/ws/cataloger");
            });
        }

        private void ConfigureKissLog(IOptionsBuilder options)
        {
            // optional KissLog configuration
            options.Options
                .AppendExceptionDetails((Exception ex) =>
                {
                    StringBuilder sb = new StringBuilder();

                    if (ex is System.NullReferenceException nullRefException)
                    {
                        sb.AppendLine("Important: check for null references");
                    }

                    return sb.ToString();
                });

            options.Options.GetUser((RequestProperties request) =>
            {
                // user name can be retrieved from the Request Claims
                // string nameClaim = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";
                // string name = request.Claims.FirstOrDefault(p => p.Key == nameClaim).Value;

                string name = request.Claims.FirstOrDefault(e => e.Key == "Username").Value;

                return new UserDetails
                {
                    EmailAddress = name
                };
            });

            // KissLog internal logs
            options.InternalLog = (message) =>
            {
                Debug.WriteLine(message);
            };

            // register logs output
            RegisterKissLogListeners(options);
        }

        private void RegisterKissLogListeners(IOptionsBuilder options)
        {
            // multiple listeners can be registered using options.Listeners.Add() method

            // register KissLog.net cloud listener
            options.Listeners.Add(new RequestLogsApiListener(new Application(
                Configuration["KissLog.OrganizationId"],    //  "200c5a1b-5efc-48a8-8a20-5223e22a4437"
                Configuration["KissLog.ApplicationId"])     //  "b40ad633-c7bb-4bcb-b5dc-aef9b02384c4"
            )
            {
                ApiUrl = Configuration["KissLog.ApiUrl"]    //  "https://kisslogback.signumcapital.net"
            });
        }
    }
}
