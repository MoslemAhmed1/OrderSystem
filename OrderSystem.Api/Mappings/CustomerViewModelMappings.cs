using OrderSystem.Application.DTOs.Customers;
using OrderSystem.Application.Interfaces.Services;
using OrderSystem.ViewModels.Customers;

namespace OrderSystem.Mappings
{
    public static class CustomerViewModelMappings
    {
        public static UpdateCustomerRequest ToDto(this UpdateCustomerViewModel vm)
        {
            return new UpdateCustomerRequest
            {
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                CustomerType = vm.CustomerType
            };
        }

        public static UpdateCustomerProfileRequest ToDto(this UpdateCustomerProfileViewModel vm)
        {
            return new UpdateCustomerProfileRequest
            {
                FirstName = vm.FirstName,
                LastName = vm.LastName
            };
        }

        public static CustomerViewModel ToViewModel(this CustomerResponse dto, ITranslationService translationService)
        {
            return new CustomerViewModel
            {
                Id = dto.Id,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                CustomerType = translationService.Translate($"CustomerType_{dto.CustomerType}")
            };
        }
    }
}