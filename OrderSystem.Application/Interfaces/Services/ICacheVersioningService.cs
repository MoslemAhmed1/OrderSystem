namespace OrderSystem.Application.Interfaces.Services
{
    public interface ICacheVersioningService
    {
        Task<int> GetVersionAsync(string versionKey);
        Task<int> UpdateVersionAsync(string versionKey);
    }
}
