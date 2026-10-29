namespace SabtAhval.Services.Interfaces
{
    public interface IAuditService
    {
         Task LogSuccessAsync(string reason);
         Task LogRejectedAsync(string reason);
    }
}
