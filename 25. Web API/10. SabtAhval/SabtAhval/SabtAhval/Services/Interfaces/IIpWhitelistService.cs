namespace SabtAhval.Services.Interfaces
{
    public interface IIpWhitelistService
    {
        Task<bool> IsIpAllowedAsync(string ip, int? clientId);
    }
}
