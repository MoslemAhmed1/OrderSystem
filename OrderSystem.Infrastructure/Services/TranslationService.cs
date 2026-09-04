using Microsoft.Extensions.Localization;
using OrderSystem.Application.Interfaces.Services;
using OrderSystem.Infrastructure.Resources;

namespace OrderSystem.Infrastructure.Services
{
    public class TranslationService : ITranslationService
    {
        private readonly IStringLocalizer<Messages> _localizer;

        public TranslationService(IStringLocalizer<Messages> localizer)
        {
            _localizer = localizer;
        }

        public string Translate(string key)
        {
            var value = _localizer[key];
            return value.ResourceNotFound ? key : value;
        }

        public string Translate(string key, params object[] args)
        {
            var value = _localizer[key, args];
            return value.ResourceNotFound ? key : value;
        }
    }
}
