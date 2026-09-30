using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OrderSystem.Application.Behaviors;
using OrderSystem.Application.Features.Products.Commands;
using OrderSystem.Application.Features.Products.Commands.SetProductTranslation;
using OrderSystem.Application.Features.Products.Commands.UpdateProduct;
using OrderSystem.Application.Features.Products.Queries;

namespace OrderSystem.Application.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // MediatR
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblyContaining<GetProductByIdHandler>();

                cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));

                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            });

            services.AddValidatorsFromAssemblyContaining<CreateProductCommandValidator>();

            return services;
        }
    }
}
