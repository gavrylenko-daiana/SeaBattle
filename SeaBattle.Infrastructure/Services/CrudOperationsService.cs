using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using SeaBattle.Domain.Helpers;
using SeaBattle.Infrastructure.Exceptions;
using SeaBattle.Infrastructure.Interfaces;

namespace SeaBattle.Infrastructure.Services;

public class CrudOperationsService<TEntity> : ICrudOperationsService<TEntity> where TEntity : class
{
    private readonly IMappingService _mappingService;
    private readonly ISqlExecutorService _sqlExecutorService;
    private static readonly object _lock = new();
    private static readonly Dictionary<string, int> _values = new();

    public CrudOperationsService(IMappingService mappingService, ISqlExecutorService sqlExecutorService)
    {
        _mappingService = mappingService;
        _sqlExecutorService = sqlExecutorService;
    }
    
    public SqlCommand GetInsertCommand(TEntity entity)
    {
        if (entity is null)
        {
            throw new NullException();
        }
        
        var columns = _mappingService.GetColumnNames<TEntity>();
        var parameterNames = _mappingService.GetParameterNames<TEntity>();

        var commandText = $"INSERT INTO {_mappingService.GetEntityTableName(typeof(TEntity))} ({string.Join(", ", columns)}) VALUES ({string.Join(", ", parameterNames)})";

        var parameters = _mappingService.MapToParameters(entity);

        return CreateSqlCommand(commandText, parameters);
    }

    public SqlCommand GetUpdateCommand(TEntity entity)
    {
        if (entity is null)
        {
            throw new NullException();
        }
        
        var tableName = _mappingService.GetEntityTableName(typeof(TEntity));
        var columnParameterPairs = _mappingService.GetColumnParameterPairs<TEntity>();
        var setClause = GetSetClause(columnParameterPairs);
        var keyColumnName = _mappingService.GetKeyProperty<TEntity>();

        var commandText = $"UPDATE {tableName} SET {setClause} WHERE {keyColumnName} = @{keyColumnName}";

        var parameters = _mappingService.MapToParameters(entity);

        return CreateSqlCommand(commandText, parameters);
    }

    public List<SqlCommand> GetDeleteCommand(Type entityType, object entity, int id, bool isRootEntity = true)
    {
        var tableName = _mappingService.GetEntityTableName(entityType);
        var keyProperty = (ExecuteGenericMethodForMappingService("GetKeyProperty", entityType, new object[] { }) as string)!;
        var deleteCommands = new List<string>();

        if (!isRootEntity)
        {
            AddDeleteCommandForRootEntity(tableName, keyProperty, id, deleteCommands);
        }

        ProcessCollectionProperties(entityType, entity, deleteCommands);
        ProcessSingleEntityProperties(entityType, entity, deleteCommands);

        if (isRootEntity)
        {
            AddDeleteCommandForRootEntity(tableName, keyProperty, id, deleteCommands);
        }

        RemoveDuplicateCommands(deleteCommands);

        return CreateSqlCommands(deleteCommands, keyProperty, id);
    }

    public string GetByIdQuery(int id)
    {
        var columns = _mappingService.GetColumnNames<TEntity>();

        return $"SELECT {string.Join(", ", columns)} FROM {_mappingService.GetEntityTableName(typeof(TEntity))} WHERE {_mappingService.GetKeyProperty<TEntity>()} = {id}";
    }

    public string GetAllQuery(int pageNumber = 1, int pageSize = 20)
    {
        var query = String.Empty;
        int skip = (pageNumber - 1) * pageSize;
        var entityKey = _mappingService.GetKeyProperty<TEntity>();

        if (typeof(TEntity).IsAbstract)
        {
            var pageQuery = $" ORDER BY {entityKey} OFFSET {skip} ROWS FETCH NEXT {pageSize} ROWS ONLY";
            query = InnerJoinWithTypeQuery(typeof(TEntity)) + pageQuery;
        }
        else
        {
            var tableName = _mappingService.GetEntityTableName(typeof(TEntity));
            query = $"SELECT * FROM {tableName} ORDER BY {entityKey} OFFSET {skip} ROWS FETCH NEXT {pageSize} ROWS ONLY";
        }

        return query;
    }

    public string InnerJoinWithTypeQuery(Type entityType, int? id = null)
    {
        var typeEntityProperty = entityType.GetProperties().FirstOrDefault(prop => Attribute.IsDefined(prop, typeof(TypeEntityAttribute)));
        var entityTableName = typeEntityProperty.GetCustomAttribute<TypeEntityAttribute>();
        var entityForeignKeyAttribute = typeEntityProperty.GetCustomAttribute<ForeignKeyAttribute>();

        var columns = (ExecuteGenericMethodForMappingService("GetColumnNames", entityType, new object[] { }) as IEnumerable<string>)!.ToList();
        var columnType = (ExecuteGenericMethodForMappingService("GetColumnNames", typeEntityProperty!.PropertyType, new object[] { }) as IEnumerable<string>)!.ToList();
        
        columns = columns.Union(columnType).ToList();
        var relationIdIndex = columns.FindIndex(c => c.Equals(entityForeignKeyAttribute.Name));
        columns[relationIdIndex] = $"{entityTableName.Name}.{entityForeignKeyAttribute.Name}";
        
        var selectQuery = $"SELECT {string.Join(", ", columns)} FROM {_mappingService.GetEntityTableName(entityType)}";
        var innerQuery = $" INNER JOIN {entityTableName.Name} ON {_mappingService.GetEntityTableName(entityType)}.{entityForeignKeyAttribute.Name} = {entityTableName.Name}.{entityForeignKeyAttribute.Name}";

        if (id is not null)
        {
            var entityKey = ExecuteGenericMethodForMappingService("GetKeyProperty", entityType, new object[] { });

            var whereQuery = $" WHERE {entityKey as string} = {id}";

            return selectQuery + innerQuery + whereQuery;
        }

        return selectQuery + innerQuery;
    }

    public void LoadListProperties(Type entityType, object entity, int id, object parentEntity = null!)
    {
        var properties = entityType.GetProperties();

        foreach (var property in properties)
        {
            var isEnumerableType = typeof(IEnumerable).IsAssignableFrom(property.PropertyType);
            var isCorrectAttribute = property.GetIndexParameters().Length == 0;
            var isNotStringType = property.PropertyType != typeof(string);
            var isNotLoadAttribute = property?.GetCustomAttribute<NotLoadAttribute>();

            if (parentEntity is not null)
            {
                var isEqualNames = parentEntity.GetType().Name.Contains(property.Name);
                var isEqualTableNames = _mappingService.GetEntityTableName(parentEntity.GetType()).Contains(property.Name);

                if (isEqualNames || isEqualTableNames)
                {
                    continue;
                }
            }

            if (isEnumerableType && isNotStringType && isNotLoadAttribute is null)
            {
                LoadCollectionProperty(entityType, entity, property, id);
            }
            else if (property.PropertyType.IsClass && !isEnumerableType && isNotStringType && isCorrectAttribute && isNotLoadAttribute is null)
            {
                LoadSingleEntityProperty(entityType, entity, property, id);
            }
        }
    }

    public int GenerateId<TEntity>() where TEntity : class
    {
        lock (_lock)
        {
            var entityType = typeof(TEntity);
            var entityName = _mappingService.GetEntityTableName(entityType);

            if (_values.TryGetValue(entityName, out int currentId))
            {
                _values[entityName] = currentId + 1;

                return currentId + 1;
            }

            var keyColumnName = _mappingService.GetKeyProperty<TEntity>();
            var query = $"SELECT MAX({keyColumnName}) FROM {entityName}";
            var newId = _sqlExecutorService.GetNewId<TEntity>(query);

            _values.Add(entityName, newId);

            return newId;
        }
    }

    public bool ExistsInDatabase<TEntity>(TEntity entity) where TEntity : class
    {
        var keyProperty = typeof(TEntity).GetProperties()
            .FirstOrDefault(p => Attribute.IsDefined(p, typeof(KeyAttribute)));

        if (keyProperty is null)
        {
            throw new KeyPropertyException();
        }
        
        var keyValue = keyProperty.GetValue(entity);
        var keyColumnName = keyProperty.GetCustomAttribute<ColumnAttribute>()?.Name ?? keyProperty.Name;

        var query = $"SELECT COUNT(1) FROM {_mappingService.GetEntityTableName(typeof(TEntity))} WHERE {keyColumnName} = @keyValue";
        
        return _sqlExecutorService.IsExistInDataBase(query, keyValue!);
    }
    
    public object ExecuteGenericMethodForMappingService(string methodName, Type type, object[] parameters)
    {
        var method = _mappingService.GetType().GetMethod(methodName)?.MakeGenericMethod(type);
        var item = method.Invoke(_mappingService, parameters);

        return item;
    }
    
    private SqlCommand CreateSqlCommand(string commandText, IDictionary<string, object> parameters)
    {
        var command = new SqlCommand(commandText);

        foreach (var param in parameters)
        {
            command.Parameters.AddWithValue($"@{param.Key}", param.Value ?? DBNull.Value);
        }

        return command;
    }
    
    private string GetSetClause(IEnumerable<KeyValuePair<string, string>> columnParameterPairs)
    {
        return string.Join(", ", columnParameterPairs.Select(kv => $"{kv.Key} = @{kv.Key}"));
    }

    private void LoadCollectionProperty(Type entityType, object entity, PropertyInfo property, int id)
    {
        var listType = property.PropertyType.GetGenericArguments()[0];
        var listName = _mappingService.GetEntityTableName(listType);

        if (listType is not null)
        {
            var entityTypeKey = (ExecuteGenericMethodForMappingService("GetKeyProperty", entityType, new object[] { }) as string)!;
            var columns = ExecuteGenericMethodForMappingService("GetColumnNames", listType, new object[] { }) as IEnumerable<string>;
            var query = $"SELECT {string.Join(", ", columns)} FROM {listName} WHERE {entityTypeKey} = {id}";
            var dataTable = _sqlExecutorService.ExecuteQuery(query);
            var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(listType))!;

            foreach (DataRow row in dataTable.Rows)
            {
                var item = ExecuteGenericMethodForMappingService("MapFromDataRow", listType, new object[] { row });
                string listTypeKey = (ExecuteGenericMethodForMappingService("GetKeyProperty", listType, new object[] { }) as string)!;

                LoadListProperties(listType, item, Convert.ToInt32(row[$"{listTypeKey}"]), entity);

                list.Add(item);
            }

            property.SetValue(entity, list);
        }
    }

    private void LoadSingleEntityProperty(Type entityType, object entity, PropertyInfo property, int id)
    {
        var propertyType = property.PropertyType;

        if (propertyType is not null)
        {
            var query = InnerJoinQuery(entityType, property, id);
            var dataTable = _sqlExecutorService.ExecuteQuery(query);
            var item = ExecuteGenericMethodForMappingService("MapFromDataRow", propertyType, new object[] { dataTable.Rows[0] });

            property.SetValue(entity, item);

            var listTypeKey = (ExecuteGenericMethodForMappingService("GetKeyProperty", propertyType, new object[] { }) as string)!;

            LoadListProperties(propertyType, item, Convert.ToInt32(dataTable.Rows[0][$"{listTypeKey}"]), entity);
        }
    }

    private void HandleCollectionDeletion(Type listType, PropertyInfo property, object entity,
        List<string> deleteCommands)
    {
        var listKeyProperty = (ExecuteGenericMethodForMappingService("GetKeyProperty", listType, new object[] { }) as string)!;

        if (property.GetValue(entity) is IEnumerable list)
        {
            var tempDeleteCommands = new List<string>();

            foreach (var item in list)
            {
                var itemId = listType.GetProperty(listKeyProperty).GetValue(item);
                var itemDeleteCommand = GetDeleteCommand(item.GetType(), item, Convert.ToInt32(itemId), false);
                tempDeleteCommands.AddRange(itemDeleteCommand.Select(i => i.CommandText));
            }

            deleteCommands.InsertRange(0, tempDeleteCommands);
        }
    }

    private void HandleSingleEntityDeletion(PropertyInfo property, object entity, List<string> deleteCommands)
    {
        var relatedEntity = property.GetValue(entity);

        if (relatedEntity is not null)
        {
            var relatedEntityType = property.PropertyType;
            var relatedEntityKey = (ExecuteGenericMethodForMappingService("GetKeyProperty", relatedEntityType, new object[] { }) as string)!;
            var relatedEntityId = relatedEntityType.GetProperty(relatedEntityKey).GetValue(relatedEntity);
            var relatedEntityDeleteCommand = GetDeleteCommand(relatedEntityType, relatedEntity, Convert.ToInt32(relatedEntityId), false);

            deleteCommands.AddRange(relatedEntityDeleteCommand.Select(i => i.CommandText));
        }
    }

    private string InnerJoinQuery(Type entityType, PropertyInfo property = null, int? id = null)
    {
        var columns = ExecuteGenericMethodForMappingService("GetColumnNames", entityType, new object[] { }) as IEnumerable<string>;
        
        var selectQuery = $"SELECT {string.Join(", ", columns)} FROM {_mappingService.GetEntityTableName(entityType)}";
        var innerJoinQuery = GenerateInnerJoinQuery(entityType, selectQuery, property);

        var whereQuery = string.Empty;

        if (id.HasValue)
        {
            var entityKeyProperty = (ExecuteGenericMethodForMappingService("GetKeyProperty", entityType, new object[] { }) as string)!;
            whereQuery = $" WHERE {_mappingService.GetEntityTableName(entityType)}.{entityKeyProperty} = {id.Value}";
        }

        return innerJoinQuery + whereQuery;
    }

    private string GenerateInnerJoinQuery(Type entityType, string selectQuery, PropertyInfo property = null!)
    {
        var generalColumn = string.Empty;
        var tableName = _mappingService.GetEntityTableName(entityType);
        var foreignKeyAttribute = property?.GetCustomAttribute<ForeignKeyAttribute>();
        var relatedTableName = property is not null ? _mappingService.GetEntityTableName(property.PropertyType) : null;
        var foreignKeyPropertyName = foreignKeyAttribute?.Name;

        if (property is not null && relatedTableName is not null && foreignKeyPropertyName is not null)
        {
            var innerQuery = string.Empty;
            var typeEntityProperty = property.PropertyType.GetProperties().FirstOrDefault(prop => Attribute.IsDefined(prop, typeof(TypeEntityAttribute)));
            var columns = (ExecuteGenericMethodForMappingService("GetColumnNames", entityType, new object[] { }) as IEnumerable<string>)!.ToList();
            
            if (typeEntityProperty is not null && property.PropertyType.IsAbstract)
            {
                var columnType = (ExecuteGenericMethodForMappingService("GetColumnNames", typeEntityProperty!.PropertyType, new object[] { }) as IEnumerable<string>)!.ToList();
                var entityTableName = typeEntityProperty.GetCustomAttribute<TypeEntityAttribute>();
                var entityForeignKeyAttribute = typeEntityProperty.GetCustomAttribute<ForeignKeyAttribute>();
                
                if (entityTableName is not null && entityForeignKeyAttribute is not null)
                {
                    columns = columns.Union(columnType).ToList();
                    var relationIdIndex = columns.FindIndex(c => c.Equals(entityForeignKeyAttribute.Name));
                    columns[relationIdIndex] = $"{entityTableName.Name}.{entityForeignKeyAttribute.Name}";
                    generalColumn = entityForeignKeyAttribute.Name;
                    
                    innerQuery = $" INNER JOIN {entityTableName.Name} ON {_mappingService.GetEntityTableName(property.PropertyType)}.{entityForeignKeyAttribute.Name} = {entityTableName.Name}.{entityForeignKeyAttribute.Name}";
                }
            }
            
            var relatedEntityKey = (ExecuteGenericMethodForMappingService("GetKeyProperty", property.PropertyType, new object[] { }) as string)!;
            var columnRelationEntity = (ExecuteGenericMethodForMappingService("GetColumnNames", property.PropertyType, new object[] { }) as IEnumerable<string>)!.ToList();

            columns = columns.Union(columnRelationEntity).ToList();
            var relationForeignIdIndex = columns.FindIndex(c => c.Equals(foreignKeyPropertyName));
            columns[relationForeignIdIndex] = $"{relatedTableName}.{foreignKeyPropertyName}";

            if (!string.IsNullOrEmpty(generalColumn))
            {
                var relationGeneralColumn = columns.FindIndex(c => c.Equals(generalColumn));
                columns.RemoveAt(relationGeneralColumn);
            }
            
            var select = $"SELECT {string.Join(", ", columns)} FROM {_mappingService.GetEntityTableName(entityType)}";

            return select + $" INNER JOIN {relatedTableName} ON {tableName}.{foreignKeyPropertyName} = {relatedTableName}.{relatedEntityKey}" + innerQuery;
        }
       
        return selectQuery;
    }

    private void AddDeleteCommandForRootEntity(string tableName, string keyProperty, int id,
        List<string> deleteCommands)
    {
        deleteCommands.Add($"DELETE FROM {tableName} WHERE {keyProperty} = {id};");
    }

    private void ProcessCollectionProperties(Type entityType, object entity, List<string> deleteCommands)
    {
        foreach (var property in entityType.GetProperties())
        {
            if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType) && property.PropertyType != typeof(string))
            {
                var listType = property.PropertyType.GetGenericArguments()[0];
                HandleCollectionDeletion(listType, property, entity, deleteCommands);
            }
        }
    }

    private void ProcessSingleEntityProperties(Type entityType, object entity, List<string> deleteCommands)
    {
        foreach (var property in entityType.GetProperties())
        {
            var isCorrectAttribute = property.CustomAttributes.All(ca => ca.AttributeType != typeof(TypeEntityAttribute)) && property.GetIndexParameters().Length == 0;

            if (property.PropertyType.IsClass && !typeof(IEnumerable).IsAssignableFrom(property.PropertyType) && isCorrectAttribute)
            {
                HandleSingleEntityDeletion(property, entity, deleteCommands);
            }
        }
    }

    private void RemoveDuplicateCommands(List<string> deleteCommands)
    {
        for (int i = 0; i < deleteCommands.Count; i++)
        {
            var lastIndex = deleteCommands.LastIndexOf(deleteCommands[i]);

            if (i != lastIndex)
            {
                deleteCommands.RemoveAt(i);
                i--;
            }
        }
    }

    private List<SqlCommand> CreateSqlCommands(List<string> deleteCommands, string keyProperty, int id)
    {
        var combinedCommands = new List<SqlCommand>();

        foreach (var command in deleteCommands)
        {
            var sqlCommand = new SqlCommand(command);
            sqlCommand.Parameters.AddWithValue(keyProperty, id);
            combinedCommands.Add(sqlCommand);
        }

        return combinedCommands;
    }
}