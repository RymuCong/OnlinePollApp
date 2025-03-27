using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace T3H.Poll.Persistence.MappingConfigurations;

public class UserClaimConfiguration : IEntityTypeConfiguration<UserClaim>
{
    public void Configure(EntityTypeBuilder<UserClaim> builder)
    {
        builder.ToTable("UserClaims");
        builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
    }
}
