using System.Linq.Expressions;

namespace SeaBattle.Application.Interfaces;

public interface IRepository<TEntity> where TEntity : class
{
    Task<TEntity> GetById(int id);
    Task<List<TEntity>> GetAll(Expression<Func<TEntity, bool>> filter = null,
        Expression<Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>> orderBy = null);
    Task Insert(TEntity entity);
    Task Delete(TEntity entity);
    Task Update(TEntity entity);
}