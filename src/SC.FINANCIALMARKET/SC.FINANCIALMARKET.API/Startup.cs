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

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<CatalogadorHub>("/ws/cataloger");
            });
        }
    }
}
