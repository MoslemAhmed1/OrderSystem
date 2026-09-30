using FluentValidation;
using MediatR;
using OrderSystem.Application.DTOs.Products;
using OrderSystem.Application.Interfaces.Repositories;
using OrderSystem.Application.Interfaces.Services;
using OrderSystem.Application.Mappings;
using System.Globalization;

namespace OrderSystem.Application.Features.Products.Queries
{
    public record ProductQueryParameters
    {
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 20;
        public string? Search { get; init; }
        public decimal? MinPrice { get; init; }
        public decimal? MaxPrice { get; init; }
        public string? SortBy { get; init; }
        public bool SortDescending { get; init; } = false;
    }

    public record GetAllProductsQuery(ProductQueryParameters Params) : IRequest<PagedResponse<ProductResponse>>;

    public class GetAllProductsQueryValidator : AbstractValidator<GetAllProductsQuery>
    {
        private static readonly string[] AllowedSortFields = ["name", "price", "stockquantity"];

        public GetAllProductsQueryValidator()
        {
            RuleFor(x => x.Params.Page)
                .GreaterThan(0).WithMessage("Page number must be greater than zero.");

            RuleFor(x => x.Params.PageSize)
                .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");

            RuleFor(x => x.Params.SortBy)
                .Must(s => s is null || AllowedSortFields.Contains(s.ToLowerInvariant()))
                .WithMessage($"SortBy must be one of: {string.Join(", ", AllowedSortFields)}.");

            RuleFor(x => x.Params.MinPrice)
                .GreaterThanOrEqualTo(0).When(x => x.Params.MinPrice.HasValue)
                .WithMessage("MinPrice cannot be negative.");

            RuleFor(x => x.Params.MaxPrice)
                .GreaterThanOrEqualTo(0).When(x => x.Params.MaxPrice.HasValue)
                .WithMessage("MaxPrice cannot be negative.");

            RuleFor(x => x)
                .Must(x => x.Params.MinPrice is null || x.Params.MaxPrice is null || x.Params.MinPrice <= x.Params.MaxPrice)
                .WithMessage("MinPrice must be less than or equal to MaxPrice.");
        }
    }

    public class GetAllProductsHandler : IRequestHandler<GetAllProductsQuery, PagedResponse<ProductResponse>>
    {
        private readonly IProductRepository _productRepository;
        private readonly ICacheService _cache;
        private readonly ICacheVersioningService _cacheVersioning;

        public GetAllProductsHandler(
            IProductRepository productRepository,
            ICacheService cache,
            ICacheVersioningService cacheVersioning)
        {
            _productRepository = productRepository;
            _cache = cache;
            _cacheVersioning = cacheVersioning;
        }

        public async Task<PagedResponse<ProductResponse>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
        {
            var queryParams = request.Params;
            var version = await _cacheVersioning.GetVersionAsync(CacheKeys.ProductsVersion);
            var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            var cacheKey = BuildCacheKey(version, culture, queryParams);

            var cached = await _cache.GetAsync<PagedResponse<ProductResponse>>(cacheKey);
            if (cached is not null)
                return cached;

            var (products, totalCount) = await _productRepository.GetPagedAsync(culture, queryParams);

            var response = new PagedResponse<ProductResponse>
            {
                Items      = products.Select(pr => pr.ToDto()).ToList(),
                TotalCount = totalCount,
                Page       = queryParams.Page,
                PageSize   = queryParams.PageSize
            };

            await _cache.SetAsync(cacheKey, response);
            return response;
        }

        private static string BuildCacheKey(int version, string culture, ProductQueryParameters queryParams)
        {
            var search = queryParams.Search?.ToLowerInvariant() ?? "all";
            var minPrice = queryParams.MinPrice?.ToString("F2") ?? "any";
            var maxPrice = queryParams.MaxPrice?.ToString("F2") ?? "any";
            var sort = $"{queryParams.SortBy?.ToLowerInvariant() ?? "default"}:{(queryParams.SortDescending ? "desc" : "asc")}";

            return $"products:v{version}:list:{culture}:s={search}:price={minPrice}-{maxPrice}:sort={sort}:pg={queryParams.Page}x{queryParams.PageSize}";
        }
    }
}
