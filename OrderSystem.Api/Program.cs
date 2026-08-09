using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using OrderSystem.Application.Auth;
using OrderSystem.Application.Caching;
using OrderSystem.Application.Interfaces.Repositories;
using OrderSystem.Infrastructure.Repositories;
using OrderSystem.Common;
using OrderSystem.Middlewares;
using OrderSystem.Infrastructure.Data;
using OrderSystem.Application.Discount;
using OrderSystem.Application.Interfaces.Services;
using OrderSystem.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

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

builder.Services.AddDbContext<OrderContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("OrderSystemDb"));
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Repositories
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

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

// Services
builder.Services.Configure<DiscountOptions>(builder.Configuration.GetSection("Discounts"));
builder.Services.AddSingleton<IDiscountPolicy, DiscountPolicy>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddHostedService<TokenCleanupService>();

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
1- Try moving Discount to appsettings.json, so any discount can be applied without changing the code [Done]
2- Unit of Work: remove repositories, each service will have an instance of uow and the repositories it needs only [Done]
3- OrderService: for loop in BuildItems() which gets products by id, add a function in OrderRepository which gets all products by a list of ids, and then use that function in OrderService [Done]
4- Apply: ViewModel <-> DTO <-> Entity mapping [Done]
5- Controllers: use Generic Response structure which contains (StatusCode, Message, Data, Errors) [Done]
6- Learn & Apply Middleware, Exception Handling, Logging [Done]
7- Learn & Apply Clean Architecture
8- Authentication/Authorization: use JWT with Access & Refresh tokens, handle multiple sessions from Websites, Mobiles, etc..
9- Use Hashing, Salting for Passwords, and use JWT for Authentication
10- Use In-Memory caching, Redis is a plus
11- Implement language translations
12- Add StockQuantity to Product entity, and implement stock management in OrderService [Done]
13- Add Cancelled Status to Order entity, and implement order cancellation in OrderService [Done]
14- Learn about EF Tracking
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
----------------------------------------------------------------------
Extra Changes:
1- VM<->DTO<->Entity mapping is applied in the following flow:
    - Controller (Presentation Layer) receives VM, converts VM to DTO and send to Service
    - Service (Application Layer) converts DTO to Entity to interact with DB
    - DB (Infrastructure Layer) returns Entity to Service
    - Service converts Entity to DTO and send to Controller
    - Controller converts DTO to VM and send to Client

2- Clean Architecture is applied in the following flow:
    - Presentation Layer (Controllers, Middlewares, etc...)
    - Application Layer (Business Logic, DTOs, Interfaces)
    - Domain Layer (Entities, Value Objects, Domain Services)
    - Infrastructure Layer (Interface Implementation, DbContext, External Services)

3- Split the mappings:
    - ViewModel mappers in Presentation Layer
    - DTO mappers in Application Layer
    - Entity mappers in Infrastructure Layer

4- Presentation Layer Abstraction: 
    - Instead of exposing the Infrastructure Layer to the Presentation Layer, we can 
    create a class library which becomes an abstraction between the Presentation layer and 
    the Application&Infrastructure layers
*/