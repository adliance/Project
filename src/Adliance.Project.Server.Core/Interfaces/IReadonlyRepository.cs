namespace Adliance.Project.Server.Core.Interfaces;

public interface IReadonlyRepository<T> where T : class, IEntity
{
    Task<T?> ById(Guid id);
    IQueryable<T> Query();
}