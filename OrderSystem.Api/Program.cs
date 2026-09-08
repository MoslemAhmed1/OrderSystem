using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OrderSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;

using OrderSystem.Common;
using OrderSystem.Middlewares;

using OrderSystem.Application.Interfaces.Services;
using OrderSystem.Application.Interfaces.Repositories;

using OrderSystem.Infrastructure.Context;
using OrderSystem.Infrastructure.Services;
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
            ValidateLifetime = true,
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
builder.Services.AddSingleton<ITokenHasher, Sha256TokenHasher>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// Localization
builder.Services.AddLocalization(options => { options.ResourcesPath = "Resources"; });
builder.Services.Configure<AppLocalizationOptions>(builder.Configuration.GetSection("Localization"));

var locOptions = builder.Configuration.GetSection("Localization").Get<AppLocalizationOptions>();
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(locOptions.DefaultCulture)
    .AddSupportedCultures(locOptions.SupportedCultures)
    .AddSupportedUICultures(locOptions.SupportedCultures);
builder.Services.AddScoped<ITranslationService, TranslationService>();

// Caching
builder.Services.Configure<CachingOptions>(builder.Configuration.GetSection("Caching"));
var cachingProvider = builder.Configuration.GetSection("Caching")["Provider"];
if (cachingProvider == "InMemory")
{
    builder.Services.AddMemoryCache();
    builder.Services.AddSingleton<ICacheService, InMemoryCacheService>();
}
else if (cachingProvider == "Redis")
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = builder.Configuration.GetConnectionString("Redis");
        options.InstanceName = "OrderSystem_";
    });
    builder.Services.AddSingleton<ICacheService, RedisCacheService>();
}
else
{
    builder.Services.AddSingleton<ICacheService, NullCacheService>();
}

builder.Services.AddSingleton<ICacheVersioningService, CacheVersioningService>();

var app = builder.Build();

// Automatic Migartions
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var dbContext = services.GetRequiredService<OrderContext>();
        if (dbContext.Database.HasPendingModelChanges())
        {
            logger.LogCritical("There are changes in Domain.Entities which haven't been added to a migration.");
            throw new InvalidOperationException("Add a migration before starting the application.");
        }

        logger.LogInformation("Applying database migrations...");
        await dbContext.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied.");
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "Database migration failed.");
        throw;
    }
}

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
- Learn & Implement CQRS (Command Query Responsibility Segregation) pattern
- Read about CORS
- Complete remaining TODOs in DotNetTest project
 
------------------------------------------------------------------------------------------ 
 
Finished Tasks 2:

Part 1 - Small Modifications:
- Move IDiscountPolicy to Application Layer, and DiscountPolicy to Infrastructure/Services/
- Apply cache versioning, and cache invalidation on update, delete, and create
- Learn & Apply automatic migrations (automatically apply migrations & update databse on application startup)
- Make sure of the optional attributes in RefreshToken entity

Part 2 - C# Topics:
- Learn Extension Methods
- Learn Records, Types, when to use each & Comparison between records & classes
- Learn Multi-threading, Sync vs Async

Part 3 - EF Core Topics:
- Learn Tracking vs AsNoTracking vs AsNoTrackingWithIdentityResolution
- Learn Entry State Tracking (Added, Modified, Deleted, Unchanged, Detached)
- Learn FindAsync vs FirstOrDefaultAsync
- Learn First vs FirstOrDefault
- Learn Single vs SingleOrDefault
- Learn Split Queries (AsSplitQuery) vs Single Query (AsSingleQuery)
- Learn TPT vs TPC vs TPH

Part 4 - Apply EF Concepts:
- Learn to explicitly specify entry as Added, Unchanged, Modified, Deleted, Detached (Creating User & Customer in RegisterAsync)
    - Can't explicitly put an id and change the state to "Added" unless Id is a Guid
    - Solved by linking the user object itself with the navigation property in Customer and RefreshToken
        1- EF Core's ChangeTracker tracks these links and builds a dependency graph, so it sees that the 
           user object is the parent and the user nav prop links in customer & refreshtoken are children and so it inserts the user first.
        2- When calling SaveChanges, EF Core starts processing the dependency graph from top down, so it inserts
           the user first, gets the generated Id.
        3- It then continues processing the dependency graph, so it replaces the userId in Customer & RefreshToken
           with the one which was obtained after inserting the user. This is called "Relationship Fixup".
        4- After the dependencies are handled, then customer & refresh token entities are inserted safely.
- Learn & Implement Transactions in Unit of Work (BeginTransaction, Commit, Rollback, ...)

Part 5 - JWT & Passwords:
- Learn JWT headers, payload, signature Sign(header + . + payload, secretKey)
- Learn JWT vs JWS vs JWE

Part 6 - Authentication & Authorization:
- AuthService: Apply Password Hashing Abstration & DI
- TokenService: Abstraction for SHA256 hashing, and use it for hashing refresh tokens
- Move IssueTokens into TokenService
- Add option to logout from all sessions, and logout from this session

Part 7 - Translations:
- Revise Translations(IStringLocalizer, IStringLocalizerFactory, and every related line in program.cs), Implement Translations for all messages, errors, view models(the views presented to the client), and DTOs(the data sent to the client)

------------------------------------------------------------------------------------------

Finished Tasks:
1-  Discount: move to appsettings.json, so any discount can be applied without changing the code [Configurations, DONE]
2-  Unit of Work: remove repositories, each service will have an instance of uow and the repositories it needs only [DONE]
3-  OrderService: fix N+1 query problem [DONE]
4-  Add StockQuantity to Product entity, and implement stock management in OrderService [DONE]
5-  Add Cancelled Status to Order entity, and implement order cancellation in OrderService [DONE]
6-  Apply: ViewModel <-> DTO <-> Entity mapping [DONE, learn extension methods]
7-  Controllers: use Generic Response structure which contains (StatusCode, Message, Data, Errors) [DONE]
8-  Middleware, Exception Handling, Logging [DONE]
9-  Clean Architecture [DONE, needs revision for concepts]
10- Authentication & Authorization: use JWT with Access & Refresh tokens, handle multiple sessions from Websites, Mobiles, etc..
11- Use Hashing, Salting for Passwords
12- Caching: In-Memory & Redis [DONE]
13- Language Translations (Localization)

----------------------------------------------------------------------

Questions:
- Should I split functionalities like (logout, logoutall), (2 overloaded revoke functions, revokeall&revokeby), etc..
- CachingOptions using which type of IOptions ?
- AppLocalizationOptions correct approach or just keep it as raw config ?
*/