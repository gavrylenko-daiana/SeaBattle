using System.Linq.Expressions;
using SeaBattle.Persistence;
using Microsoft.EntityFrameworkCore;
using SeaBattle.Application.Interfaces;

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

    public async Task<T> GetById(int id)
    {
        try
        {
            return await _dbSet.FindAsync(id);
        }
        catch (Exception ex)
        {
            throw new Exception($"Exception message:{ex.Message}");
        }
    }

    public virtual async Task<List<T>> GetAll(
        Expression<Func<T, bool>> filter = null,
        Expression<Func<IQueryable<T>, IOrderedQueryable<T>>> orderBy = null)
    {
        try
        {
            IQueryable<T> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (orderBy != null)
            {
                return await orderBy.Compile()(query).ToListAsync();
            } 
            
            return await query.ToListAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"Exception message:{ex.Message}");
        }
    }
}