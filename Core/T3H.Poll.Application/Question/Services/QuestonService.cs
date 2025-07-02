using Microsoft.EntityFrameworkCore;

namespace T3H.Poll.Application.Question.Services;

public class QuestionService : CrudService<Domain.Entities.Question>, IQuestionService
{
    public QuestionService(IRepository<Domain.Entities.Question, Guid> repository, Dispatcher dispatcher) : base(repository, dispatcher)
    {
    }

    public async Task<List<Domain.Entities.Question>> GetQuestionsWithChoicesByIdsAsync(List<Guid> questionIds, CancellationToken cancellationToken = default)
    {
        return await GetQueryableSet()
            .Include(q => q.Choices)
            .Where(q => questionIds.Contains(q.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task SoftDeleteQuestionsAsync(List<Domain.Entities.Question> questions, CancellationToken cancellationToken = default)
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
    
    public async Task<List<Domain.Entities.Question>> GetQuestionsByIdsAsync(List<Guid> questionIds, CancellationToken cancellationToken = default)
    {
        return await GetQueryableSet()
            .Where(q => questionIds.Contains(q.Id))
            .ToListAsync(cancellationToken);
    }
}