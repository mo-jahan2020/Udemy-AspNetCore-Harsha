using SabtAhval.Models.Entities;

namespace SabtAhval.Services.Interfaces
{
    public interface IApiKeyService
    {
       public  Task<ApiClient?> GetByApiKeyAsync(string key);
    }
}
