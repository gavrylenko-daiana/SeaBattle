using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Reflection;
using SeaBattle.Domain.Helpers;
using SeaBattle.Infrastructure.Interfaces;

namespace SeaBattle.Infrastructure.Services;

public class MappingService : IMappingService
{
    public IEnumerable<string> GetColumnNames<TEntity>() where TEntity : class
    {
        return typeof(TEntity).GetProperties()
            .Select(p => p.GetCustomAttribute<ColumnAttribute>())
            .Where(a => a != null)
            .Select(a => a.Name);
    }

    public IEnumerable<string> GetParameterNames<TEntity>() where TEntity : class
    {
        return typeof(TEntity).GetProperties()
            .Select(p => p.GetCustomAttribute<ColumnAttribute>())
            .Where(a => a != null)
            .Select(a => "@" + a.Name);
    }

    public string GetEntityTableName(Type entityType)
    {
        var tableAttribute = entityType.GetCustomAttribute<TableAttribute>();

        return tableAttribute is not null ? tableAttribute.Name : entityType.Name;
    }

    public string GetKeyProperty<TEntity>() where TEntity : class
    {
        var keyProperty = typeof(TEntity).GetProperties()
            .FirstOrDefault(prop => Attribute.IsDefined(prop, typeof(KeyAttribute))) ?? typeof(TEntity)
            .GetProperties().FirstOrDefault();

        return GetKeyColumnName(keyProperty);
    }

    public Dictionary<string, object> MapToParameters<TEntity>(TEntity entity) where TEntity : class
    {
        var parameters = new Dictionary<string, object>();
        var properties = typeof(TEntity).GetProperties();

        foreach (var property in properties)
        {
            if (property.GetCustomAttribute<ColumnAttribute>() is not null)
            {
                parameters.Add(property.Name, property.GetValue(entity));
            }
        }

        return parameters;
    }

    public TEntity MapFromDataRow<TEntity>(DataRow row) where TEntity : class
    {
        TEntity entity = null;
        
        if (typeof(TEntity).IsAbstract)
        {
            var typeEntityProperty = typeof(TEntity).GetProperties().FirstOrDefault(prop => Attribute.IsDefined(prop, typeof(TypeEntityAttribute)));

            if (typeEntityProperty is not null)
            {
                var relatedEntityType = typeEntityProperty.PropertyType;
                var typeProperty = relatedEntityType.GetProperties().FirstOrDefault(prop => Attribute.IsDefined(prop, typeof(TypeEntityAttribute)));

                if (typeProperty is not null && row.Table.Columns.Contains(typeProperty.Name))
                {
                    var typeName = row[typeProperty.Name] as string;

                    if (!string.IsNullOrEmpty(typeName))
                    {
                        entity = DerivedInstanceCreator.CreateInstance<TEntity>(typeName);
                    }
                }
            }
        }
        else
        {
            entity = Activator.CreateInstance<TEntity>();
        }
        
        var properties = typeof(TEntity).GetProperties();
    
        foreach (var property in properties)
        {
            if (row.Table.Columns.Contains(property.Name) && row[property.Name] != DBNull.Value)
            {
                property.SetValue(entity, row[property.Name]);
            }
        }
    
        return entity;
    }

    public IEnumerable<KeyValuePair<string, string>> GetColumnParameterPairs<TEntity>() where TEntity : class
    {
        return typeof(TEntity).GetProperties()
            .Where(p => p.GetCustomAttribute<ColumnAttribute>() is not null && p.GetCustomAttribute<KeyAttribute>() is null)
            .Select(p => new KeyValuePair<string, string>(p.GetCustomAttribute<ColumnAttribute>()?.Name ?? p.Name, p.Name));
    }

    private string GetKeyColumnName(PropertyInfo? keyProperty)
    {
        return keyProperty is not null
            ? (keyProperty.GetCustomAttribute<ColumnAttribute>()?.Name ?? keyProperty.Name)
            : "Id";
    }
}