// using System.Data;
// using SeaBattle.Application.Interfaces;
// using SeaBattle.Infrastructure.Interfaces;
//
// namespace SeaBattle.Infrastructure;
//
// public class Repository<TEntity>
// {
//     private readonly IMappingService _mappingService;
//     private readonly ISqlExecutorService _sqlExecutorService;
//     private readonly ICrudOperationsService<TEntity> _crudOperationsService;
//
//     public Repository(ISqlExecutorService sqlExecutorService, ICrudOperationsService<TEntity> crudOperationsService,
//         IMappingService mappingService)
//     {
//         _sqlExecutorService = sqlExecutorService;
//         _crudOperationsService = crudOperationsService;
//         _mappingService = mappingService;
//     }
//
//     public IEnumerable<TEntity> GetAll(int pageNumber = 1, int pageSize = 1000)
//     {
//         var query = _crudOperationsService.GetAllQuery(pageNumber, pageSize);
//
//         var dataTable = _sqlExecutorService.ExecuteQuery(query);
//
//         var entities = new List<TEntity>();
//
//         foreach (DataRow row in dataTable.Rows)
//         {
//             var entity = _mappingService.MapFromDataRow<TEntity>(row);
//             var entityTypeKey =
//                 (_crudOperationsService.ExecuteGenericMethodForMappingService("GetKeyProperty", entity.GetType(),
//                     new object[] { }) as string)!;
//
//             _crudOperationsService.LoadListProperties(typeof(TEntity), entity, Convert.ToInt32(row[entityTypeKey]));
//
//             entities.Add(entity);
//         }
//
//         return entities;
//     }
//
//     public TEntity GetById(int id)
//     {
//         var query = String.Empty;
//
//         if (typeof(TEntity).IsAbstract)
//         {
//             query = _crudOperationsService.InnerJoinWithTypeQuery(typeof(TEntity), id);
//         }
//         else
//         {
//             query = _crudOperationsService.GetByIdQuery(id);
//         }
//
//         var dataTable = _sqlExecutorService.ExecuteQuery(query);
//
//         if (dataTable.Rows.Count == 0)
//         {
//             return null;
//         }
//
//         var entity = _mappingService.MapFromDataRow<TEntity>(dataTable.Rows[0]);
//
//         _crudOperationsService.LoadListProperties(typeof(TEntity), entity, id);
//
//         return entity;
//     }
//
//     public TEntity Insert(TEntity entity)
//     {
//         var newId = _crudOperationsService.GenerateId<TEntity>();
//
//         var keyProperty = _mappingService.GetKeyProperty<TEntity>();
//         var propertyInfo = typeof(TEntity).GetProperty(keyProperty);
//
//         if (propertyInfo is not null && propertyInfo.CanWrite)
//         {
//             propertyInfo.SetValue(entity, newId);
//         }
//
//         var command = _crudOperationsService.GetInsertCommand(entity);
//         _sqlExecutorService.AddCommand(command);
//
//         return entity;
//     }
//
//     public void Update(TEntity entity)
//     {
//         var command = _crudOperationsService.GetUpdateCommand(entity);
//
//         _sqlExecutorService.AddCommand(command);
//     }
//
//     public void Delete(int id, TEntity? entity = null)
//     {
//         entity ??= GetById(id);
//         var commands = _crudOperationsService.GetDeleteCommand(entity.GetType(), entity, id);
//
//         foreach (var command in commands)
//         {
//             _sqlExecutorService.AddCommand(command);
//         }
//     }
// }