using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using T3H.Poll.Domain.Entities;

namespace T3H.Poll.Persistence.MappingConfigurations;

public class PollAnswerConfiguration : IEntityTypeConfiguration<PollAnswer>
{
    public void Configure(EntityTypeBuilder<PollAnswer> builder)
    {
        builder.ToTable("PollAnswer");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.QuestionType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.TextAnswer)
            .HasMaxLength(4000);

        builder.Property(x => x.AnsweredAt)
            .IsRequired();

        // Ignore the NotMapped helper property
        builder.Ignore(x => x.SelectedChoiceIds);

        builder.HasIndex(x => x.AnsweredAt);

        // Relationships - ALL NoAction to avoid cascade conflicts
        builder.HasOne(x => x.Submission)
            .WithMany(x => x.Answers)
            .HasForeignKey(x => x.SubmissionId)
            .OnDelete(DeleteBehavior.NoAction); // Changed to NoAction

        builder.HasOne(x => x.Question)
            .WithMany()
            .HasForeignKey(x => x.QuestionId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.SingleChoice)
            .WithMany()
            .HasForeignKey(x => x.SingleChoiceId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure the one-to-many relationship with PollAnswerChoice
        builder.HasMany(x => x.SelectedChoices)
            .WithOne(x => x.PollAnswer)
            .HasForeignKey(x => x.PollAnswerId)
            .OnDelete(DeleteBehavior.Cascade); // Only this one can be Cascade
    }
}