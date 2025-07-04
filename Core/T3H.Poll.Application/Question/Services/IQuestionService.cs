﻿namespace T3H.Poll.Application.Question.Services;

public interface IQuestionService : ICrudService<Domain.Entities.Question>
{
    Task<List<Domain.Entities.Question>> GetQuestionsWithChoicesByIdsAsync(List<Guid> questionIds, CancellationToken cancellationToken = default);
    Task HardDeleteQuestionsAndChoicesAsync(List<Domain.Entities.Question> questions, CancellationToken cancellationToken = default);
    Task<List<Domain.Entities.Question>> GetQuestionsByIdsAsync(List<Guid> questionIds, CancellationToken cancellationToken = default);
    Task<Domain.Entities.Question?> GetQuestionWithPollAsync(Guid questionId, CancellationToken cancellationToken = default);
    Task<List<Domain.Entities.Question>> GetRelatedQuestionsAsync(Guid pollId, Guid excludeQuestionId, CancellationToken cancellationToken = default);
    Task<List<Domain.Entities.Question>> GetQuestionsByPollIdAsync(Guid pollId, CancellationToken cancellationToken = default);
    Task SoftDeleteQuestionsAsync(List<Guid> questionIds, CancellationToken cancellationToken = default);
    Task<int> GetNextQuestionOrderAsync(Guid pollId, CancellationToken cancellationToken = default);
    Task<bool> IsQuestionOrderExistsAsync(Guid pollId, int questionOrder, CancellationToken cancellationToken = default);
    Task<bool> IsQuestionOrderExistsAsync(Guid pollId, int questionOrder, Guid? excludeQuestionId = null, CancellationToken cancellationToken = default);
}