using System.Text;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;

using OrderSystem.Common;
using OrderSystem.Middlewares;

using OrderSystem.Application.Interfaces.Services;
using OrderSystem.Application.Interfaces.Repositories;

using OrderSystem.Infrastructure.Data;
using OrderSystem.Infrastructure.Services;
using OrderSystem.Infrastructure.Discount;
using OrderSystem.Infrastructure.Repositories;
using OrderSystem.Infrastructure.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(entry => entry.Value?.Errors.Count > 0)
                .SelectMany(entry => entry.Value!.Errors.Select(e => $"{entry.Key}: {e.ErrorMessage}"))
                .ToList();

            var response = ApiResponse.Fail("Validation failed.", StatusCodes.Status400BadRequest, errors);
            return new BadRequestObjectResult(response);
        };
    });
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddOpenApi();

// JWT Authentication
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
var jwtOptions = builder.Configuration.GetSection("Jwt");
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtOptions["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions["Secret"]!)),
            ClockSkew = TimeSpan.Zero,
        };
    });
builder.Services.AddAuthorization();

// SQL Server DbContext
builder.Services.AddDbContext<OrderContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("OrderSystemDb"));
});

// Repositories
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Services
builder.Services.Configure<DiscountOptions>(builder.Configuration.GetSection("Discounts"));
builder.Services.AddSingleton<IDiscountPolicy, DiscountPolicy>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddHostedService<TokenCleanupService>();

// Localization
builder.Services.AddLocalization();
var supportedCultures = new[] { "en", "ar" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("en")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);
builder.Services.AddSingleton<ITranslationService, TranslationService>();

// Caching
builder.Services.AddMemoryCache();
builder.Services.Configure<CachingOptions>(builder.Configuration.GetSection("Caching"));
var cachingProvider = builder.Configuration.GetSection("Caching")["Provider"];
if (cachingProvider == "InMemory")
{
    builder.Services.AddScoped<ICacheService, InMemoryCacheService>();
}
else if (cachingProvider == "Redis")
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = builder.Configuration.GetConnectionString("Redis");
        options.InstanceName = "OrderSystem_";
    });
    builder.Services.AddScoped<ICacheService, RedisCacheService>();
}
else
{
    builder.Services.AddScoped<ICacheService, NullCacheService>();
}

var app = builder.Build();

app.UseExceptionHandler(opt => { });

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "OrderSystem API v1");
    });
}

app.UseRequestTiming();
app.UseRateLimiting();

app.UseRequestLocalization(localizationOptions);

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

/*
Upcoming Tasks:
1-  Discount: move to appsettings.json, so any discount can be applied without changing the code
2-  Unit of Work: remove repositories, each service will have an instance of uow and the repositories it needs only
3-  OrderService: fix N+1 query problem
4-  Add StockQuantity to Product entity, and implement stock management in OrderService
5-  Add Cancelled Status to Order entity, and implement order cancellation in OrderService
6-  Apply: ViewModel <-> DTO <-> Entity mapping
7-  Controllers: use Generic Response structure which contains (StatusCode, Message, Data, Errors)
8-  Middleware, Exception Handling, Logging
9-  Clean Architecture
10- Authentication & Authorization: use JWT with Access & Refresh tokens, handle multiple sessions from Websites, Mobiles, etc..
11- Use Hashing, Salting for Passwords
12- Caching: In-Memory & Redis
13- Language Translations (Localization)
----------------------------------------------------------------------
Questions:
1- User <-> Customer Design: should we have a separate entity for User and Customer, or should we merge them into one entity?
----------------------------------------------------------------------
Meeting Flow:
- Models:
    - Data Annotations vs Fluent API
    - Migrations Up & Down purpose
- Repositories & Unit of Work:
    - Purpose of Repositories
    - Immediate vs Deferred Execution in LINQ (IQueryable vs IEnumerable)
    - Eager vs Lazy vs Explicit Loading in EF Core
    - Purpose of Unit of Work 
- Services:
    - AutoMapper
    - Why Services should not return Entities directly to Controllers
- Controllers:
    - Why Controllers should not return Entities directly to Clients
    - Generic Response
- Program.cs:
    - Purpose of JsonSerializerOptions
    - Scoped vs Singleton vs Transient
----------------------------------------------------------------------
Extra Notes:
- Purpose of DTOs
- Why there's no UpdateAsync for dbContext
- Learn about EF Tracking
----------------------------------------------------------------------
Extra Changes:
- Presentation Layer Abstraction: 
    - Instead of exposing the Infrastructure Layer to the Presentation Layer, we can 
    create a class library which becomes an abstraction between the Presentation layer and 
    the Application&Infrastructure layers
*/