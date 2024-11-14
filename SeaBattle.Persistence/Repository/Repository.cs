using System.Linq.Expressions;
using SeaBattle.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using SeaBattle.Application.Interfaces;
using SeaBattle.Domain.Models;

namespace SeaBattle.Persistence.Repository;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly ApplicationContext _context;
    private readonly DbSet<T> _dbSet;

    public Repository(ApplicationContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public async Task Insert(T item)
    {
        try
        {
            await _dbSet.AddAsync(item);
        }
        catch (Exception ex)
        {
            throw new Exception($"Exception message:{ex.Message}");
        }
    }

    public async Task Delete(T item)
    {
        try
        {
            if (_context.Entry(item).State == EntityState.Detached)
            {
                _dbSet.Attach(item);
            }

            _dbSet.Remove(item);
        }
        catch (Exception ex)
        {
            throw new Exception($"Exception message:{ex.Message}");
        }
    }

    public async Task Update(T item)
    {
        try
        {
            _dbSet.Attach(item);
            _context.Entry(item).State = EntityState.Modified;
        }
        catch (Exception ex)
        {
            throw new Exception($"Exception message: {ex.Message}");
        }
    }

    public async Task<T> GetById(int id, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null!)
    {
        try
        {
            if (include == null)
            {
                return await _dbSet.FindAsync(id);
            }
            
            var primaryKey = _context.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties[0].Name;

            IQueryable<T> query = include(_dbSet);

            query = query.Where(e => EF.Property<int>(e, primaryKey) == id);

            return await query.FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"Exception message:{ex.Message}");
        }
    }

    public virtual async Task<List<T>> GetAll(
        Expression<Func<T, bool>> filter = null!,
        Expression<Func<IQueryable<T>, IOrderedQueryable<T>>> orderBy = null!,
        Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null!)
    {
        try
        {
            IQueryable<T> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }
            
            if (include != null)
            {
                query = include(query);
            }

            if (orderBy != null)
            {
                return await orderBy.Compile()(query).ToListAsync();
            } 
            
            // ToDo: remove after tests
            var res = await query.ToListAsync();
            return res;
        }
        catch (Exception ex)
        {
            throw new Exception($"Exception message:{ex.Message}");
        }
    }
}