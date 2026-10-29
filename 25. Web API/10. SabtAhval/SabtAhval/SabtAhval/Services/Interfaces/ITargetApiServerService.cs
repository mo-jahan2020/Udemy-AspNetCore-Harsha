using SabtAhval.Models.Entities;

namespace SabtAhval.Services.Interfaces
{
    public interface ITargetApiServerService
    {
        Task<TargetApiServer?> GetDefaultAsync();
    }
}
