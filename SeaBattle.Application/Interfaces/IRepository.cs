using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace SeaBattle.Application.Interfaces;

public interface IRepository<TEntity> where TEntity : class
{
    Task<TEntity> GetById(int id, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null!);
    Task<List<TEntity>> GetAll(Expression<Func<TEntity, bool>> filter = null!,
        Expression<Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>> orderBy = null!,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null!);
    Task Insert(TEntity entity);
    Task Delete(TEntity entity);
    Task Update(TEntity entity);
}