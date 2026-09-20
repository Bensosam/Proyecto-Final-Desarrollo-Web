using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Clinica.API.Data
{
    public class ClinicaDbContextFactory : IDesignTimeDbContextFactory<ClinicaDbContext>
    {
        public ClinicaDbContext CreateDbContext(string[] args)
        {
            var basePath = Directory.GetCurrentDirectory();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false)
                .AddUserSecrets<ClinicaDbContextFactory>()
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<ClinicaDbContext>();

            optionsBuilder.UseSqlServer(connectionString);

            return new ClinicaDbContext(optionsBuilder.Options);
        }
    }
}