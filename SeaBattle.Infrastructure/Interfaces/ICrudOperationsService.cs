using System.Data.SqlClient;

namespace SeaBattle.Infrastructure.Interfaces;

public interface ICrudOperationsService<TEntity> where TEntity : class
{
    SqlCommand GetInsertCommand(TEntity entity);
    SqlCommand GetUpdateCommand(TEntity entity);
    List<SqlCommand> GetDeleteCommand(Type entityType, object entity, int id, bool isRootEntity = true);
    string GetByIdQuery(int id);
    string GetAllQuery(int pageNumber = 1, int pageSize = 10);
    string InnerJoinWithTypeQuery(Type entityType, int? id = null);
    object ExecuteGenericMethodForMappingService(string methodName, Type type, object[] parameters);
    void LoadListProperties(Type entityType, object entity, int id, object parentEntity = null!);
    int GenerateId<TEntity>() where TEntity : class;
    bool ExistsInDatabase<TEntity>(TEntity entity) where TEntity : class;
}