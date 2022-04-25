using Adliance.Project.Server.Core.Interfaces;

namespace Adliance.Project.Server.Infrastructure.Database;

public class Repository<T> : ReadonlyRepository<T>, IRepository<T> where T : class, IEntity
{
    private readonly Db _db;

    public Repository(Db db) : base(db)
    {
        _db = db;
    }

    public async Task<T> Add(T entity)
    {
        _db.Set<T>().Add(entity);
        await _db.SaveChangesAsync();
        return entity;
    }

    public async Task Update(T entity)
    {
        _db.Set<T>().Update(entity);
        await _db.SaveChangesAsync();
    }

    public async Task Delete(T entity)
    {
        _db.Set<T>().Remove(entity);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteRange(IEnumerable<T> entities)
    {
        _db.Set<T>().RemoveRange(entities);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteRange(IQueryable<T> entities)
    {
        _db.Set<T>().RemoveRange(entities);
        await _db.SaveChangesAsync();
    }


    protected override bool UseNoTracking { get; set; } = false;
}