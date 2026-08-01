using AutoMapper;
using OrderSystem.DTOs.Products;
using OrderSystem.Models;
using OrderSystem.ViewModels;

namespace OrderSystem.Mappings
{
    public class ProductMappingProfile : Profile
    {
        public ProductMappingProfile()
        {
            // Entity -> Dto
            CreateMap<Product, ProductResponse>();

            // Dto -> Entity
            CreateMap<CreateProductRequest, Product>();
            CreateMap<UpdateProductRequest, Product>();

            // Dto -> ViewModel
            CreateMap<ProductResponse, ProductViewModel>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.FormattedPrice, opt => opt.MapFrom(src => $"${src.Price:F2}"))
                .ForMember(dest => dest.IsLowStock, opt => opt.MapFrom(src => src.StockQuantity < 5))
                .ForMember(dest => dest.StockStatus, opt => opt.MapFrom(src =>
                    src.StockQuantity == 0 ? "Out of Stock" :
                    src.StockQuantity < 5 ? "Low Stock" : "In Stock"));
        }
    }
}
