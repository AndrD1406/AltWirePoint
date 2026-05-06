using AltWirePoint.DataAccess.Enums;
using AltWirePoint.DataAccess.Extensions;
using AltWirePoint.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AltWirePoint.DataAccess.Repository.Base;

public class EntityRepository<TKey, TEntity> : IEntityRepository<TKey, TEntity>
    where TEntity : class, IKeyedEntity<TKey>, new()
    where TKey : IEquatable<TKey>
{
    protected readonly AltWirePointDbContext dbContext;
    protected readonly DbSet<TEntity> dbSet;

    public EntityRepository(AltWirePointDbContext dbContext)
    {
        this.dbContext = dbContext;
        dbSet = this.dbContext.Set<TEntity>();
    }

    public virtual async Task<TEntity> Create(TEntity entity)
    {
        await dbSet.AddAsync(entity).ConfigureAwait(false);
        await dbContext.SaveChangesAsync().ConfigureAwait(false);

        return entity;
    }

    public virtual async Task<IEnumerable<TEntity>> Create(IEnumerable<TEntity> entities)
    {
        await dbSet.AddRangeAsync(entities).ConfigureAwait(false);
        await dbContext.SaveChangesAsync().ConfigureAwait(false);

        return entities;
    }

    public virtual async Task<T> RunInTransaction<T>(Func<Task<T>> operation)
    {
        var executionStrategy = dbContext.Database.CreateExecutionStrategy();

        return await executionStrategy.ExecuteAsync(
            async () =>
            {
                await using var transaction = await dbContext.Database.BeginTransactionAsync().ConfigureAwait(false);
                try
                {
                    var result = await operation().ConfigureAwait(false);
                    await transaction.CommitAsync().ConfigureAwait(false);
                    return result;
                }
                catch
                {
                    await transaction.RollbackAsync().ConfigureAwait(false);
                    throw;
                }
            });
    }

    public virtual async Task RunInTransaction(Func<Task> operation)
    {
        var executionStrategy = dbContext.Database.CreateExecutionStrategy();

        await executionStrategy.ExecuteAsync(
            async () =>
            {
                await using var transaction = await dbContext.Database.BeginTransactionAsync().ConfigureAwait(false);
                try
                {
                    await operation().ConfigureAwait(false);
                    await transaction.CommitAsync().ConfigureAwait(false);
                }
                catch
                {
                    await transaction.RollbackAsync().ConfigureAwait(false);
                    throw;
                }
            });
    }

    public virtual async Task Delete(TEntity entity)
    {
        dbContext.Entry(entity).State = EntityState.Deleted;
        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public virtual IQueryable<TEntity> GetByFilter(
        Expression<Func<TEntity, bool>> whereExpression,
        string includeProperties = "",
        Func<IQueryable<TEntity>, IQueryable<TEntity>> includeExpression = null)
    {
        IQueryable<TEntity> query = dbSet.Where(whereExpression);

        if (includeExpression != null)
            query = includeExpression(query);

        return query.IncludeProperties(includeProperties);
    }

    public virtual Task<TEntity> GetById(TKey id) =>
        dbSet.FirstOrDefaultAsync(x => x.Id.Equals(id));

    public virtual Task<TEntity> GetByIdWithDetails(
        TKey id,
        string includeProperties = "",
        Func<IQueryable<TEntity>, IQueryable<TEntity>> includeExpression = null)
    {
        var query = dbSet.Where(x => x.Id.Equals(id));

        if (includeExpression != null)
            query = includeExpression(query);

        return query.IncludeProperties(includeProperties).FirstOrDefaultAsync();
    }

    public virtual async Task<TEntity> Update(TEntity entity)
    {
        dbContext.Entry(entity).State = EntityState.Modified;
        await dbContext.SaveChangesAsync().ConfigureAwait(false);
        return entity;
    }

    public async Task<TEntity> ReadAndUpdateWith<TDto>(TDto dto, Func<TDto, TEntity, TEntity> map)
        where TDto : IDto<TEntity, TKey>
    {
        ArgumentNullException.ThrowIfNull(dto);

        var entity = await GetById(dto.Id).ConfigureAwait(false);

        if (entity is null)
        {
            throw new DbUpdateConcurrencyException($"Update failed. {typeof(TEntity).Name} with Id = {dto.Id} not found.");
        }

        return await Update(map(dto, entity)).ConfigureAwait(false);
    }

    public virtual Task<int> Count(Expression<Func<TEntity, bool>> whereExpression = null)
    {
        return whereExpression == null
            ? dbSet.CountAsync()
            : dbSet.CountAsync(whereExpression);
    }

    public virtual Task<bool> Any(Expression<Func<TEntity, bool>> whereExpression = null)
    {
        return whereExpression == null
            ? dbSet.AnyAsync()
            : dbSet.AnyAsync(whereExpression);
    }

    public virtual IQueryable<TEntity> Get(
        int skip = 0,
        int take = 0,
        string includeProperties = "",
        Expression<Func<TEntity, bool>> whereExpression = null,
        IEnumerable<(Expression<Func<TEntity, object>> Key, SortDirection Direction)> orderBy = null)
    {
        IQueryable<TEntity> query = dbSet;

        if (whereExpression != null)
            query = query.Where(whereExpression);

        if (orderBy != null && orderBy.Any())
        {
            IOrderedQueryable<TEntity> orderedData = null;

            foreach (var sort in orderBy)
            {
                if (orderedData == null)
                {
                    orderedData = sort.Direction == SortDirection.Ascending
                        ? query.OrderBy(sort.Key)
                        : query.OrderByDescending(sort.Key);
                }
                else
                {
                    orderedData = sort.Direction == SortDirection.Ascending
                        ? orderedData.ThenBy(sort.Key)
                        : orderedData.ThenByDescending(sort.Key);
                }
            }
            query = orderedData;
        }

        if (skip > 0) query = query.Skip(skip);
        if (take > 0) query = query.Take(take);

        return query;
    }

    public Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess = true,
        CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public int SaveChanges(bool acceptAllChangesOnSuccess = true)
    {
        return dbContext.SaveChanges();
    }
}
