using Microsoft.EntityFrameworkCore;
using SabtAhval.Common;
using SabtAhval.Models.Entities;
using System.Buffers.Text;
using System.Diagnostics.Metrics;
using System.Security.Cryptography;
using System.Text;


namespace SabtAhval.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<ApiRequestLog> apiRequestLogs { get; set; }
        public DbSet<TargetApiServer> targetApiServers { get; set; }
        public DbSet<ApiClient> apiClients { get; set; }
        public DbSet<AllowedIp> allowedIps { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApiRequestLog>().ToTable("apiRequestLogs");
            modelBuilder.Entity<TargetApiServer>().ToTable("targetApiServers");
            modelBuilder.Entity<ApiClient>().ToTable("apiClients");
            modelBuilder.Entity<AllowedIp>().ToTable("allowedIps");

            //Seed to Countries
            modelBuilder.Entity<TargetApiServer>().HasData(
                new TargetApiServer()
                {
                    Id = 1,
                    Name = "typicode",
                    BaseUrl = "https://jsonplaceholder.typicode.com",
                    CreatedAt = new DateTime(2026, 1, 1),
                    IsActive = true,
                    TimeoutSeconds = 30,
                    Description = "jsonplaceholder typicode"

                });

            modelBuilder.Entity<ApiClient>().HasData(
                new ApiClient()
                {
                    Id = 1,
                    ApiKeyHash = Constants.ComputeSha256Hash("98AHGG^%^aassjd12"),
                    IsActive = true,
                    TargetApiServerId = 1,
                    Username = "240",
                    CreatedAt = new DateTime(2026, 1, 1),

                });
            modelBuilder.Entity<AllowedIp>().HasData(
                new AllowedIp() { Id = 1, ApiClientId = 1, IpAddress = "127.0.0.1", IsActive = true },
                new AllowedIp() { Id = 2, ApiClientId = 1, IpAddress = "::1", IsActive = true });
        }


    }

}
