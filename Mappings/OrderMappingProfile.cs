using AutoMapper;
using OrderSystem.DTOs.Orders;
using OrderSystem.Models;
using OrderSystem.ViewModels;

namespace OrderSystem.Mappings
{
    public class OrderMappingProfile : Profile
    {
        public OrderMappingProfile()
        {
            // Entity → DTO
            CreateMap<Order, OrderResponse>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => $"{src.Customer.FirstName} {src.Customer.LastName}"))
                .ForMember(dest => dest.CustomerType, opt => opt.MapFrom(src => src.Customer.CustomerType.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

            CreateMap<OrderItem, OrderItemResponse>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name));

            // DTO → ViewModel
            CreateMap<OrderResponse, OrderViewModel>()
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => $"#{src.Id:D4}"))
                .ForMember(dest => dest.FormattedTotal, opt => opt.MapFrom(src => $"${src.Total:N2}"))
                .ForMember(dest => dest.OrderDate, opt => opt.MapFrom(src => src.CreatedAt.ToString("MMM dd, yyyy")))
                .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.Items.Count));
        }
    }
}
