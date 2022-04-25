using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adliance.Project.Server.Infrastructure.Database.Config;

public class ApiCall : IEntityTypeConfiguration<Adliance.Project.Server.Core.Models.ApiCall>
{
    public void Configure(EntityTypeBuilder<Adliance.Project.Server.Core.Models.ApiCall> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.TimestampUtc);
        builder.HasOne(x => x.ApiKey).WithMany(x => x.ApiCalls).HasForeignKey(x => x.ApiKeyId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.User).WithMany(x => x.ApiCalls).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}