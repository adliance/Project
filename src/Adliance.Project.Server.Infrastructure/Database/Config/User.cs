using Adliance.Project.Server.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adliance.Project.Server.Infrastructure.Database.Config;

public class User : IEntityTypeConfiguration<Adliance.Project.Server.Core.Models.User>
{
    public void Configure(EntityTypeBuilder<Adliance.Project.Server.Core.Models.User> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Upn).HasMaxLength(100).IsRequired();
        builder.HasIndex(x => x.Upn);
    }
}