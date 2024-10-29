using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SeaBattle.Persistence;

public class AppContextFactory : IDesignTimeDbContextFactory<ApplicationContext>
{
    public ApplicationContext CreateDbContext(string[] args)
    {
        var optionsBuilder = GetDbContextOptionsBuilder();

        return new ApplicationContext(optionsBuilder.Options);
    }

    private DbContextOptionsBuilder<ApplicationContext> GetDbContextOptionsBuilder()
    {
        var builder = new ConfigurationBuilder();
        builder.SetBasePath(Directory.GetCurrentDirectory());
        builder.AddJsonFile("/Users/dayanagavrylenko/Downloads/daiana-gavrylenko-feature-unit-testing/SeaBattle.API/appsettings.Development.json");
        var config = builder.Build();
        var connectionString = config.GetConnectionString("DefaultConnection");

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return optionsBuilder;
    }
}