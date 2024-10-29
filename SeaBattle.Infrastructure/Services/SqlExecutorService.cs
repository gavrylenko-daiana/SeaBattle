using System.Data;
using System.Data.SqlClient;
using SeaBattle.Infrastructure.Exceptions;
using SeaBattle.Infrastructure.Interfaces;

namespace SeaBattle.Infrastructure.Services;

public class SqlExecutorService : ISqlExecutorService
{
    private static List<SqlCommand> _commands = new();
    private readonly string _connectionString;

    public SqlExecutorService(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    public bool ExecuteCommands()
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        var transaction = connection.BeginTransaction();

        try
        {
            foreach (var command in _commands)
            {
                command.Connection = connection;
                command.Transaction = transaction;
                command.ExecuteNonQuery();
            }

            transaction.Commit();

            return true;
        }
        catch
        {
            transaction.Rollback();
            
            return false;
        }
        finally
        {
            transaction.Dispose();
            ClearCommands();
        }
    }

    public void AddCommand(SqlCommand command)
    {
        _commands.Add(command);
    }

    public DataTable ExecuteQuery(string query)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var command = new SqlCommand(query, connection);
            using var reader = command.ExecuteReader();
            var dataTable = new DataTable();
            
            dataTable.Load(reader);

            return dataTable;
        }
        catch (Exception)
        {
            throw new NonExistentIdException();
        }
    }

    public int GetNewId<TEntity>(string query) where TEntity : class
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        
        var command = new SqlCommand(query, connection);
        var result = command.ExecuteScalar();

        var newId = result != DBNull.Value && result != null ? Convert.ToInt32(result) + 1 : 1;

        return newId;
    }

    public bool IsExistInDataBase(string query, object keyValue)
    {
        using var connection = new SqlConnection(_connectionString);
        var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@keyValue", keyValue);

        connection.Open();
        var count = (int)command.ExecuteScalar();

        return count > 0;
    }
    
    private void ClearCommands()
    {
        _commands.Clear();
    }
}