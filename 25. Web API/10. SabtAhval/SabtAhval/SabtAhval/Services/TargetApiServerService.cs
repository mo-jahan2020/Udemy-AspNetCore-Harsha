using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SabtAhval.Data;
using SabtAhval.Models.Entities;
using SabtAhval.Services.Interfaces;

namespace SabtAhval.Services
{
    public class TargetApiServerService :ITargetApiServerService
    {
        private readonly AppDbContext _context;
        public TargetApiServerService(AppDbContext context)
        {
            _context = context;
        }
        public async  Task<TargetApiServer?> GetDefaultAsync()
        {
// 1. اولین سرور فعال را برگردان (مرتب‌شده بر اساس Id)
            //    اگر هیچ سرور فعالی وجود نداشت، null برگردان
            return await _context.targetApiServers
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Id)
                .FirstOrDefaultAsync();
        }
    }
}
