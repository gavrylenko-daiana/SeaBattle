using System.Data;
using System.Data.SqlClient;

namespace SeaBattle.Infrastructure.Interfaces;

public interface ISqlExecutorService
{
    bool ExecuteCommands();
    void AddCommand(SqlCommand command);
    DataTable ExecuteQuery(string query);
    int GetNewId<TEntity>(string query) where TEntity : class;
    bool IsExistInDataBase(string query, object keyValue);
}