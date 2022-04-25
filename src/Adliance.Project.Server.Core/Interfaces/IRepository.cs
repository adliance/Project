namespace Adliance.Project.Server.Core.Interfaces;

public interface IRepository<T> : IReadonlyRepository<T> where T : class, IEntity
{
    Task<T> Add(T entity);
    Task Update(T entity);
    Task Delete(T entity);
    Task DeleteRange(IEnumerable<T> entities);
    Task DeleteRange(IQueryable<T> entities);
}