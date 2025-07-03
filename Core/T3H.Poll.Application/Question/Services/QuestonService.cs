using Microsoft.EntityFrameworkCore;

namespace T3H.Poll.Application.Question.Services;

public class QuestionService : CrudService<Domain.Entities.Question>, IQuestionService
{
    private readonly IRepository<Domain.Entities.Choice, Guid> _choiceRepository;
    private readonly IRepository<Domain.Entities.Question, Guid> _questionRepository;

    public QuestionService(
        IRepository<Domain.Entities.Question, Guid> repository, 
        IRepository<Domain.Entities.Choice, Guid> choiceRepository,
        Dispatcher dispatcher) : base(repository, dispatcher)
    {
        _choiceRepository = choiceRepository;
        _questionRepository = repository;
    }

    public async Task<List<Domain.Entities.Question>> GetQuestionsWithChoicesByIdsAsync(List<Guid> questionIds, CancellationToken cancellationToken = default)
    {
        return await GetQueryableSet()
            .Include(q => q.Choices)
            .Where(q => questionIds.Contains(q.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task SoftDeleteQuestionsAndChoicesAsync(List<Domain.Entities.Question> questions, CancellationToken cancellationToken = default)
    {
        foreach (var question in questions)
        {
            // Soft delete all choices first
            foreach (var choice in question.Choices)
            {
                choice.IsDeleted = true;
                choice.UpdatedDateTime = DateTime.UtcNow;
            }

            // Soft delete the question
            question.IsDeleted = true;
            question.UpdatedDateTime = DateTime.UtcNow;
            await UpdateAsync(question, cancellationToken);
        }
    }

    public async Task HardDeleteQuestionsAndChoicesAsync(List<Domain.Entities.Question> questions, CancellationToken cancellationToken = default)
    {
        foreach (var question in questions)
        {
            // Mark choices for deletion
            foreach (var choice in question.Choices)
            {
                _choiceRepository.Delete(choice);
            }

            // Mark question for deletion
            _questionRepository.Delete(question);
        }
    
        // Save changes to actually execute the deletions
        await _questionRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<Domain.Entities.Question>> GetQuestionsByIdsAsync(List<Guid> questionIds, CancellationToken cancellationToken = default)
    {
        return await GetQueryableSet()
            .Where(q => questionIds.Contains(q.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<Domain.Entities.Question?> GetQuestionWithPollAsync(Guid questionId, CancellationToken cancellationToken = default)
    {
        return await GetQueryableSet()
            .Include(q => q.Poll)
            .FirstOrDefaultAsync(q => q.Id == questionId, cancellationToken);
    }

    public async Task<List<Domain.Entities.Question>> GetRelatedQuestionsAsync(Guid pollId, Guid excludeQuestionId, CancellationToken cancellationToken = default)
    {
        return await GetQueryableSet()
            .Where(q => q.PollId == pollId && q.Id != excludeQuestionId && q.IsActive == true)
            .OrderBy(q => q.QuestionOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Domain.Entities.Question>> GetQuestionsByPollIdAsync(Guid pollId, CancellationToken cancellationToken = default)
    {
        return await GetQueryableSet()
            .Where(q => q.PollId == pollId && q.IsDeleted != true)
            .ToListAsync(cancellationToken);
    }

    public async Task SoftDeleteQuestionsAsync(List<Guid> questionIds, CancellationToken cancellationToken = default)
    {
        var questions = await GetQueryableSet()
            .Where(q => questionIds.Contains(q.Id))
            .ToListAsync(cancellationToken);

        foreach (var question in questions)
        {
            question.IsDeleted = true;
            question.UpdatedDateTime = DateTimeOffset.UtcNow;

            await UpdateAsync(question, cancellationToken);
        }
    }
    
    public async Task<int> GetNextQuestionOrderAsync(Guid pollId, CancellationToken cancellationToken = default)
    {
        var maxOrder = await GetQueryableSet()
            .Where(q => q.PollId == pollId && q.IsDeleted != true)
            .MaxAsync(q => (int?)q.QuestionOrder, cancellationToken);
    
        return (maxOrder ?? 0) + 1;
    }

    public async Task<bool> IsQuestionOrderExistsAsync(Guid pollId, int questionOrder, CancellationToken cancellationToken = default)
    {
        return await GetQueryableSet()
            .AnyAsync(q => q.PollId == pollId && q.QuestionOrder == questionOrder && q.IsDeleted != true, cancellationToken);
    }
    
    public async Task<bool> IsQuestionOrderExistsAsync(Guid pollId, int questionOrder, Guid? excludeQuestionId = null, CancellationToken cancellationToken = default)
    {
        var query = GetQueryableSet()
            .Where(q => q.PollId == pollId && q.QuestionOrder == questionOrder && q.IsDeleted != true);

        if (excludeQuestionId.HasValue)
        {
            query = query.Where(q => q.Id != excludeQuestionId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }
}