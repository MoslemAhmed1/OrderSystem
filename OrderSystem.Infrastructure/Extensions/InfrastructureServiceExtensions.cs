using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderSystem.Application.Interfaces.Repositories;
using OrderSystem.Application.Interfaces.Services;
using OrderSystem.Domain.Entities;
using OrderSystem.Infrastructure.Context;
using OrderSystem.Infrastructure.Options;
using OrderSystem.Infrastructure.Repositories;
using OrderSystem.Infrastructure.Services;

namespace OrderSystem.Infrastructure.Extensions
{
    public static class InfrastructureServiceExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Database
            services.AddDbContext<OrderContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("OrderSystemDb")));

            // Repositories
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Domain services
            services.Configure<DiscountOptions>(configuration.GetSection("Discounts"));
            services.AddSingleton<IDiscountPolicy, DiscountPolicy>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<ICustomerService, CustomerService>();

            // Auth & token services
            services.AddScoped<ITokenService, TokenService>();
            services.AddSingleton<ITokenHasher, Sha256TokenHasher>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

            // Localization, Translation Service
            services.Configure<AppLocalizationOptions>(configuration.GetSection("Localization"));
            services.AddScoped<ITranslationService, TranslationService>();

            // Caching
            services.Configure<CachingOptions>(configuration.GetSection("Caching"));

            var cachingProvider = configuration.GetSection("Caching")["Provider"];
            if (cachingProvider == "InMemory")
            {
                services.AddMemoryCache();
                services.AddSingleton<ICacheService, InMemoryCacheService>();
            }
            else if (cachingProvider == "Redis")
            {
                services.AddStackExchangeRedisCache(opt =>
                {
                    opt.Configuration = configuration.GetConnectionString("Redis");
                    opt.InstanceName = "OrderSystem_";
                });
                services.AddSingleton<ICacheService, RedisCacheService>();
            }
            else
            {
                services.AddSingleton<ICacheService, NullCacheService>();
            }

            services.AddSingleton<ICacheVersioningService, CacheVersioningService>();

            return services;
        }
    }
}
