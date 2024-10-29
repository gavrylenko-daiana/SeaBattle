using SeaBattle.Application.Interfaces;
using SeaBattle.Infrastructure.Interfaces;

namespace SeaBattle.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    private readonly ISqlExecutorService _executorService;

    public UnitOfWork(ISqlExecutorService executorService)
    {
        _executorService = executorService;
    }
    
    public bool SaveChanges()
    {
        var result = _executorService.ExecuteCommands();

        return result;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
