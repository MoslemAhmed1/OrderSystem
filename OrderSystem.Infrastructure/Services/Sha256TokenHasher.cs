using System.Text;
using System.Security.Cryptography;
using OrderSystem.Application.Interfaces.Services;

namespace OrderSystem.Infrastructure.Services
{
    public class Sha256TokenHasher : ITokenHasher
    {
        public string Hash(string token)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(bytes);
        }
    }
}
