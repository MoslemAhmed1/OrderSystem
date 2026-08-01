using Microsoft.EntityFrameworkCore;
using OrderSystem.Data;
using OrderSystem.Mappings;
using OrderSystem.Repositories;
using OrderSystem.Services;
using OrderSystem.Services.Discount;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddOpenApi();

builder.Services.AddDbContext<OrderContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("OrderSystemDb"));
});

builder.Services.AddAutoMapper(cfg => { },
    typeof(ProductMappingProfile),
    typeof(CustomerMappingProfile),
    typeof(OrderMappingProfile)
    );

// Repositories
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Services
builder.Services.Configure<DiscountOptions>(builder.Configuration.GetSection("Discounts"));
builder.Services.AddSingleton<IDiscountPolicy, DiscountPolicy>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IProductService, ProductService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "OrderSystem API v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

/*
Upcoming Tasks:
1- Try moving Discount to appsettings.json, so any discount can be applied without changing the code [Done]
2- Unit of Work: remove repositories, each service will have an instance of uow and the repositories it needs only
3- OrderService: for loop in BuildItems() which gets products by id, add a function in OrderRepository which gets all products by a list of ids, and then use that function in OrderService
4- Apply: ViewModel <-> DTO <-> Entity mapping
5- Controllers: use Generic Response structure which contains (StatusCode, Message, Data, Errors)
6- Learn & Apply Clean Architecture
7- Learn & Apply Middleware, Exception Handling, Logging
8- Authentication/Authorization: use JWT with Access & Refresh tokens, handle multiple sessions from Websites, Mobiles, etc..
9- Use Hashing, Salting for Passwords, and use JWT for Authentication
10- Use In-Memory caching, Redis is a plus
11- Implement language translations
12- Add StockQuantity to Product entity, and implement stock management in OrderService
13- Add Cancelled Status to Order entity, and implement order cancellation in OrderService
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
*/