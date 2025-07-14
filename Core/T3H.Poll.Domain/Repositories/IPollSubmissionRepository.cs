using T3H.Poll.Domain.Entities;

namespace T3H.Poll.Domain.Repositories;

public interface IPollSubmissionRepository : IRepository<PollSubmission, Guid>
{
    Task<PollSubmission?> GetByIdWithAnswersAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<PollSubmission>> GetSubmissionsByPollIdAsync(Guid pollId, CancellationToken cancellationToken = default);
    Task<PollSubmission?> GetSubmissionByPollAndEmailAsync(Guid pollId, string email, CancellationToken cancellationToken = default);
    Task<int> GetSubmissionCountByPollIdAsync(Guid pollId, CancellationToken cancellationToken = default);
    Task<bool> HasUserSubmittedAsync(Guid pollId, string email, CancellationToken cancellationToken = default);
}
