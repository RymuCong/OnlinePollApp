using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FDS.CRM.Persistence.MappingConfigurations;

public class UserTokenConfiguration : IEntityTypeConfiguration<UserToken>
{
    public void Configure(EntityTypeBuilder<UserToken> builder)
    {
        builder.ToTable("UserTokens");
        builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
    }
}
