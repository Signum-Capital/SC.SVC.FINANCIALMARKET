using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace SC.FINANCIALMARKET.DOMAIN.Configuration
{
    public static class SwaggerConfiguration
    {
        public static IServiceCollection SwaggerConfigure(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "SC.FINANCIALMARKET Serviços do Mercado Financeiro" });

                options.AddSecurityDefinition("PlataformKey", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "",
                    Name = "PlataformKey",
                    Type = SecuritySchemeType.ApiKey
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "PlataformKey"
                            }
                        },
                        new string[]{ }
                    }
                });
            });

            return services;
        }
    }
}
