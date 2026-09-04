namespace OrderSystem.Application.Interfaces.Services
{
    public interface ITokenHasher
    {
        string Hash(string token);
    }
}
