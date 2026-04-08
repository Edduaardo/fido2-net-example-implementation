using System.Linq.Expressions;

namespace Fido2Authentication.Business.Interfaces.Repositories;

public interface IRepository<TEntity> where TEntity : class
{
    Task AddAsync(TEntity entity);
    Task UpdateAsync(TEntity entity);
    Task DeleteAsync(TEntity entity);
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<TEntity?> FindOneAsync(Expression<Func<TEntity, bool>> predicate);
}
