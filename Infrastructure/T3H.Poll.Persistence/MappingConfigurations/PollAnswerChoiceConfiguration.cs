using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using T3H.Poll.Domain.Entities;

namespace T3H.Poll.Persistence.MappingConfigurations;

public class PollAnswerChoiceConfiguration : IEntityTypeConfiguration<PollAnswerChoice>
{
    public void Configure(EntityTypeBuilder<PollAnswerChoice> builder)
    {
        builder.ToTable("PollAnswerChoice");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.RankOrder)
            .IsRequired(false);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // Create unique index to prevent duplicate choices in same answer
        builder.HasIndex(x => new { x.PollAnswerId, x.ChoiceId })
            .IsUnique();

        // Relationships
        builder.HasOne(x => x.PollAnswer)
            .WithMany(x => x.SelectedChoices)
            .HasForeignKey(x => x.PollAnswerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Choice)
            .WithMany()
            .HasForeignKey(x => x.ChoiceId)
            .OnDelete(DeleteBehavior.NoAction); // Changed from Cascade to NoAction
    }
}