using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adliance.Project.Server.Core.Interfaces;

namespace Adliance.Project.Server.Core.Test;

public class MockedReadonlyRepository<T> : IReadonlyRepository<T> where T : class, IEntity
{
    private readonly List<T> _content;

    public MockedReadonlyRepository(List<T> content)
    {
        _content = content;
    }

    public async Task<T?> ById(Guid id)
    {
        return await Task.FromResult(_content.FirstOrDefault(x => x.Id == id));
    }

    public IQueryable<T> Query()
    {
        return _content.AsQueryable();
    }
}