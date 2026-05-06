using AltWirePoint.DataAccess.Enums;
using AltWirePoint.DataAccess.Models;
using System.Linq.Expressions;

namespace AltWirePoint.DataAccess.Repository.Base;

public interface IEntityRepository<TKey, TEntity>
    where TEntity : class, IKeyedEntity<TKey>, new()
    where TKey : IEquatable<TKey>
{
    Task<TEntity> Create(TEntity entity);

    Task<IEnumerable<TEntity>> Create(IEnumerable<TEntity> entities);

    Task<TEntity> Update(TEntity entity);

    Task<TEntity> ReadAndUpdateWith<TDto>(TDto dto, Func<TDto, TEntity, TEntity> map)
        where TDto : IDto<TEntity, TKey>;

    Task Delete(TEntity entity);

    Task<T> RunInTransaction<T>(Func<Task<T>> operation);

    Task RunInTransaction(Func<Task> operation);

    Task<TEntity> GetById(TKey id);

    Task<TEntity> GetByIdWithDetails(
        TKey id,
        string includeProperties = "",
        Func<IQueryable<TEntity>, IQueryable<TEntity>> includeExpression = null);

    IQueryable<TEntity> GetByFilter(
        Expression<Func<TEntity, bool>> whereExpression,
        string includeProperties = "",
        Func<IQueryable<TEntity>, IQueryable<TEntity>> includeExpression = null);

    IQueryable<TEntity> Get(
        int skip = 0,
        int take = 0,
        string includeProperties = "",
        Expression<Func<TEntity, bool>> whereExpression = null,
        // Changed from IDictionary to IEnumerable of ValueTuples
        IEnumerable<(Expression<Func<TEntity, object>> Key, SortDirection Direction)> orderBy = null
    );

    Task<int> Count(Expression<Func<TEntity, bool>> whereExpression = null);

    Task<bool> Any(Expression<Func<TEntity, bool>> whereExpression = null);

    Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default);

    int SaveChanges(bool acceptAllChangesOnSuccess = true);
}
