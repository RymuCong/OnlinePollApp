using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace T3H.Poll.Persistence.MappingConfigurations;

public class PollConfiguration : IEntityTypeConfiguration<Domain.Entities.Poll>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Poll> builder)
    {
        builder.ToTable("Polls");
        builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
    }
}