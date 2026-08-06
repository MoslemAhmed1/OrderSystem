using Microsoft.Extensions.Localization;
using OrderSystem.Application.Interfaces.Services;

namespace OrderSystem.Infrastructure.Services
{
    public class TranslationService : ITranslationService
    {
        private readonly IStringLocalizer _localizer;

        public TranslationService(IStringLocalizerFactory factory)
        {
            var assemblyName = typeof(TranslationService).Assembly.FullName ?? string.Empty;
            _localizer = factory.Create("Resources.Messages", assemblyName);
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
