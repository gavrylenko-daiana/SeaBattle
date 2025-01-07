using SeaBattle.Persistence;
using Microsoft.EntityFrameworkCore;
using SeaBattle.API.Hubs.Interfaces;
using SeaBattle.API.Hubs.Services;
using SeaBattle.API.Middlewares;
using SeaBattle.Application.Interfaces;
using SeaBattle.Application.Services;
using SeaBattle.Domain.Interfaces;
using SeaBattle.Domain.Services;
using SeaBattle.Domain.Token;
using SeaBattle.Persistence.Repository;

namespace SeaBattle.API.DependencyRegistration;

public static class DependencyRegistration
{
    public static void RegisterServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        services.AddDbContext<ApplicationContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IPasswordHasherService, PasswordHasherService>();
        services.AddScoped<IAppUserService, AppUserService>();
        services.AddScoped<IShipService, ShipService>();
        services.AddScoped<IGameService, GameService>();
        services.AddScoped<IUserGameService, UserGameService>();
        services.AddScoped<IPointService, PointService>();
        services.AddScoped<IShipCoordinateService, ShipCoordinateService>();
        services.AddScoped<Application.Interfaces.IGameFieldService, Application.Services.GameFieldService>();
        services.AddScoped<Application.Interfaces.ICoordinateService, Application.Services.CoordinateService>();
        services.Configure<JwtTokenSettings>(configuration.GetSection("Jwt"));
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IGameInvitationService, GameInvitationService>();
        services.AddScoped<ICoordinateTypeService, CoordinateTypeService>();
        services.AddScoped<IShipTypeService, ShipTypeService>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        // services.AddScoped<IMappingService, MappingService>();
        // services.AddScoped<ISqlExecutorService, SqlExecutorService>(x => new SqlExecutorService(connectionString));
        // services.AddScoped(typeof(ICrudOperationsService<>), typeof(CrudOperationsService<>));
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
       
        services.AddScoped<Domain.Interfaces.IGameFieldService, Domain.Services.GameFieldService>();
        services.AddScoped<Domain.Interfaces.ICoordinateService, Domain.Services.CoordinateService>();
        services.AddScoped<IComputeCoordinateService, ComputeCoordinateService>();
        services.AddScoped<IValidationService, ValidationService>();
        
        services.AddScoped<IHubService, HubService>();
        services.AddTransient<GlobalErrorHandlingMiddleware>();
    }
}