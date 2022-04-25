using System.Reflection;
using Adliance.Project.Server.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Adliance.Project.Server.Infrastructure.Database;

public sealed class Db : DbContext
{
    public Db(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}