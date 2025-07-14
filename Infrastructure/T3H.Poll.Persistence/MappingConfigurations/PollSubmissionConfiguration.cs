using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using T3H.Poll.Domain.Entities;

namespace T3H.Poll.Persistence.MappingConfigurations;

public class PollSubmissionConfiguration : IEntityTypeConfiguration<PollSubmission>
{
    public void Configure(EntityTypeBuilder<PollSubmission> builder)
    {
        builder.ToTable("PollSubmissions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ParticipantEmail)
            .HasMaxLength(255);

        builder.Property(x => x.ParticipantName)
            .HasMaxLength(255);

        builder.Property(x => x.IpAddress)
            .HasMaxLength(45);

        builder.Property(x => x.UserAgent)
            .HasMaxLength(500);

        builder.Property(x => x.SubmittedAt)
            .IsRequired();

        // Index for performance
        builder.HasIndex(x => x.SubmittedAt);
        builder.HasIndex(x => x.ParticipantEmail);

        // Relationships - IMPORTANT: Change this to NoAction
        builder.HasOne(x => x.Poll)
            .WithMany()
            .HasForeignKey(x => x.PollId)
            .OnDelete(DeleteBehavior.NoAction); // Changed from Cascade to NoAction

        builder.HasMany(x => x.Answers)
            .WithOne(x => x.Submission)
            .HasForeignKey(x => x.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}