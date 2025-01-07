using System.Data;

namespace SeaBattle.Infrastructure.Interfaces;

public interface IMappingService
{
    IEnumerable<string> GetColumnNames<TEntity>() where TEntity : class;
    IEnumerable<string> GetParameterNames<TEntity>() where TEntity : class;
    Dictionary<string, object> MapToParameters<TEntity>(TEntity entity) where TEntity : class;
    TEntity MapFromDataRow<TEntity>(DataRow row) where TEntity : class;
    IEnumerable<KeyValuePair<string, string>> GetColumnParameterPairs<TEntity>() where TEntity : class;
    string GetEntityTableName(Type entityType);
    string GetKeyProperty<TEntity>() where TEntity : class;
}