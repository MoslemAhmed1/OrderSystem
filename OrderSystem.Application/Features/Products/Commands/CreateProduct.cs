using FluentValidation;
using MediatR;
using OrderSystem.Application.DTOs.Products;
using OrderSystem.Application.Features.Products.Events;
using OrderSystem.Application.Interfaces.Repositories;
using OrderSystem.Application.Interfaces.Services;
using OrderSystem.Application.Mappings;
using OrderSystem.Domain.Entities;

namespace OrderSystem.Application.Features.Products.Commands
{
    public record CreateProductCommand(
        string Name,
        decimal Price,
        int StockQuantity,
        List<ProductTranslationRequest> Translations) : IRequest<ProductResponse>;

    public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        private static readonly List<string> AllowedCultures = new List<string>() { "en", "ar", "de" };
        public CreateProductCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required.")
                .MaximumLength(100).WithMessage("Product name must not exceed 100 characters.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than zero.");

            RuleFor(x => x.StockQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative.");

            RuleForEach(x => x.Translations).ChildRules(t =>
            {
                t.RuleFor(tr => tr.Culture)
                    .Must(ct => AllowedCultures.Contains(ct.ToLowerInvariant())).WithMessage("This culture isn't supported")
                    .NotEmpty().WithMessage("Translation culture is required.")
                    .MaximumLength(10).WithMessage("Culture code must not exceed 10 characters.");

                t.RuleFor(tr => tr.Name)
                    .NotEmpty().WithMessage("Translated name is required.")
                    .MaximumLength(100).WithMessage("Translated name must not exceed 100 characters.");
            });

            RuleFor(x => x.Translations)
                .Must(ts => ts.Select(t => t.Culture.ToLowerInvariant()).Distinct().Count() == ts.Count)
                .When(x => x.Translations.Count > 0)
                .WithMessage("Duplicate cultures are not allowed in translations.");
        }
    }

    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductResponse>
    {
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _uow;
        private readonly ICacheVersioningService _cacheVersioning;
        private readonly IMediator _mediator;

        public CreateProductCommandHandler(
            IProductRepository productRepository,
            IUnitOfWork uow,
            ICacheVersioningService cacheVersioning,
            IMediator mediator)
        {
            _productRepository = productRepository;
            _uow = uow;
            _cacheVersioning = cacheVersioning;
            _mediator = mediator;
        }

        public async Task<ProductResponse> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var product = request.ToEntity();

            await _productRepository.AddAsync(product);

            foreach (var t in request.Translations)
            {
                var translation = new ProductTranslation
                {
                    Product = product,
                    Culture = t.Culture.ToLowerInvariant(),
                    Name = t.Name
                };
                product.Translations.Add(translation);
                await _productRepository.AddTranslationAsync(translation);
            }

            await _uow.SaveChangesAsync();

            var response = product.ToDto();

            await _cacheVersioning.UpdateVersionAsync(CacheKeys.ProductsVersion);

            await _mediator.Publish(
                new ProductCreatedEvent(product.Id, product.Name, product.Price, product.StockQuantity),
                cancellationToken);

            return response;
        }
    }
}
