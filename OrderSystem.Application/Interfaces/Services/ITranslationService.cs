namespace OrderSystem.Application.Interfaces.Services
{
    public interface ITranslationService
    {
        string Translate(string key);
        string Translate(string key, params object[] args);
    }
}
