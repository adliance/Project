using Adliance.Project.Server.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Adliance.Project.Server.Infrastructure.Database;

public class ReadonlyRepository<T> : IReadonlyRepository<T> where T : class, IEntity
{
    private readonly Db _db;

    public ReadonlyRepository(Db db)
    {
        _db = db;
    }

    public async Task<T?> ById(Guid id)
    {
        // we use SingleOrDefault here (instead of FirstOrDefault) to really enforce that an ID has to be unique
        // something is very very wrong if for same reason there are two entities with the same ID ;) and we really want an exception in this case
        return await Query().SingleOrDefaultAsync(x => x.Id == id);
    }

    public IQueryable<T> Query()
    {
        var query = _db.Set<T>().AsQueryable();
        if (UseNoTracking) query = query.AsNoTracking();
        return query;
    }

    protected virtual bool UseNoTracking { get; set; } = true;
}