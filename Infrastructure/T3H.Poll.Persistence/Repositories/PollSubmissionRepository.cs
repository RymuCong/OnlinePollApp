namespace T3H.Poll.Persistence.Repositories;

public class PollSubmissionRepository : Repository<PollSubmission, Guid>, IPollSubmissionRepository
{
    public PollSubmissionRepository(CrmDbContext context, IDateTimeProvider dateTimeProvider) 
        : base(context, dateTimeProvider)
    {
    }

    public async Task<PollSubmission?> GetByIdWithAnswersAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(s => s.Answers)
            .ThenInclude(a => a.Question)
            .Include(s => s.Answers)
            .ThenInclude(a => a.SingleChoice)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<PollSubmission>> GetSubmissionsByPollIdAsync(Guid pollId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(s => s.Answers)
            .Where(s => s.PollId == pollId)
            .OrderByDescending(s => s.SubmittedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<PollSubmission?> GetSubmissionByPollAndEmailAsync(Guid pollId, string email, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(s => s.Answers)
            .FirstOrDefaultAsync(s => s.PollId == pollId && s.ParticipantEmail == email, cancellationToken);
    }

    public async Task<int> GetSubmissionCountByPollIdAsync(Guid pollId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .CountAsync(s => s.PollId == pollId, cancellationToken);
    }

    public async Task<bool> HasUserSubmittedAsync(Guid pollId, string email, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AnyAsync(s => s.PollId == pollId && s.ParticipantEmail == email, cancellationToken);
    }
}
