using Adliance.Project.Server.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adliance.Project.Server.Infrastructure.Database.Config;

public class ApiKey : IEntityTypeConfiguration<Adliance.Project.Server.Core.Models.ApiKey>
{
    public void Configure(EntityTypeBuilder<Adliance.Project.Server.Core.Models.ApiKey> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Key).HasMaxLength(100).IsRequired();
        builder.HasIndex(x => x.Key);
    }
}