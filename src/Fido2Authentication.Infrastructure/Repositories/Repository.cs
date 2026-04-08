using System.Linq.Expressions;
using Fido2Authentication.Business.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fido2Authentication.Infrastructure.Repositories;

public class Repository<TEntity>(DatabaseContext databaseContext) : IRepository<TEntity> where TEntity : class 
{
    protected readonly DatabaseContext _databaseContext = databaseContext;

    public async Task AddAsync(TEntity entity)
    {
        await _databaseContext.AddAsync(entity);
        await _databaseContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(TEntity entity)
    {
        _databaseContext.Attach(entity);
        await _databaseContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(TEntity entity)
    {
        _databaseContext.Remove(entity);
        await _databaseContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return await _databaseContext.Set<TEntity>()
            .ToListAsync();
    }

    public async Task<TEntity?> FindOneAsync(
        Expression<Func<TEntity, bool>> predicate)
    {
        var data = _databaseContext.Set<TEntity>()
            .Where(predicate);

        //data.Include();

        return await data.FirstOrDefaultAsync();
    }
}
