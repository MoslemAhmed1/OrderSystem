using AutoMapper;
using OrderSystem.DTOs.Customers;
using OrderSystem.Models;
using OrderSystem.ViewModels;

namespace OrderSystem.Mappings
{
    public class CustomerMappingProfile : Profile
    {
        public CustomerMappingProfile()
        {
            // Entity -> Dto
            CreateMap<Customer, CustomerResponse>()
                .ForMember(dest => dest.CustomerType, opt => opt.MapFrom(src => src.CustomerType.ToString()));

            // Dto -> Entity
            CreateMap<CreateCustomerRequest, Customer>();
            CreateMap<UpdateCustomerRequest, Customer>();

            // Dto -> ViewModel
            CreateMap<CustomerResponse, CustomerViewModel>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(dest => dest.IsVIP, opt => opt.MapFrom(src => (src.CustomerType == "VIP")));
        }
    }
}
