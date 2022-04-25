using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adliance.Project.Server.Infrastructure.Database.Config;

public class Article : IEntityTypeConfiguration<Adliance.Project.Server.Core.Models.Article>
{
    public void Configure(EntityTypeBuilder<Adliance.Project.Server.Core.Models.Article> builder)
    {
        builder.ToTable(x => x.IsTemporal());
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.UpdatedApiKey).WithMany().HasForeignKey(x => x.UpdatedApiKeyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.UpdatedUser).WithMany().HasForeignKey(x => x.UpdatedUserId).OnDelete(DeleteBehavior.Restrict);
    }
}