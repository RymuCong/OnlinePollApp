using T3H.Poll.Application.Choice.Services;
using T3H.Poll.Application.Question.DTOs;
using T3H.Poll.Application.Question.Services;

namespace T3H.Poll.Application.Question.Queries;

public class GetQuestionDetailQuery : IQuery<ResultModel<QuestionDetailDto>>
{
    public Guid QuestionId { get; set; }

    public GetQuestionDetailQuery(Guid questionId)
    {
        QuestionId = questionId;
    }

    public static void Validate(GetQuestionDetailQuery request)
    {
        if (request.QuestionId == Guid.Empty)
        {
            throw new ValidationException("QuestionId không hợp lệ.");
        }
    }
}

public class GetQuestionDetailQueryHandler : IQueryHandler<GetQuestionDetailQuery, ResultModel<QuestionDetailDto>>
{
    private readonly IQuestionService _questionService;
    private readonly IChoiceService _choiceService;

    public GetQuestionDetailQueryHandler(
        IQuestionService questionService,
        IChoiceService choiceService)
    {
        _questionService = questionService;
        _choiceService = choiceService;
    }

    public async Task<ResultModel<QuestionDetailDto>> HandleAsync(GetQuestionDetailQuery query, CancellationToken cancellationToken)
    {
        GetQuestionDetailQuery.Validate(query);

        // Get question details using service method
        var question = await _questionService.GetQuestionWithPollAsync(query.QuestionId, cancellationToken);

        if (question == null)
        {
            throw new NotFoundException($"Không tìm thấy question với ID {query.QuestionId}");
        }

        // Map basic question properties
        var questionResult = MapQuestionToDto(question);

        // Load related data using service methods
        await LoadRelatedDataAsync(questionResult, question, cancellationToken);

        return ResultModel<QuestionDetailDto>.Create(questionResult);
    }

    private QuestionDetailDto MapQuestionToDto(Domain.Entities.Question question)
    {
        return new QuestionDetailDto
        {
            Id = question.Id,
            PollId = question.PollId,
            QuestionText = question.QuestionText,
            QuestionType = question.QuestionType.ToString(),
            IsRequired = question.IsRequired,
            QuestionOrder = question.QuestionOrder,
            MediaUrl = question.MediaUrl,
            Settings = question.Settings,
            IsActive = question.IsActive,
            CreatedDateTime = question.CreatedDateTime.DateTime,
            UpdatedDateTime = question.UpdatedDateTime?.DateTime,
            PollTitle = question.Poll?.Title,
            PollDescription = question.Poll?.Description,
            PollCreatorId = question.Poll?.CreatorId,
            PollIsActive = question.Poll?.IsActive ?? false
        };
    }

    private async Task LoadRelatedDataAsync(QuestionDetailDto questionResult, Domain.Entities.Question question, CancellationToken cancellationToken)
    {
        // Load choices using service method
        questionResult.Choices = await GetQuestionChoicesAsync(question.Id, cancellationToken);

        // Load related questions using service method
        questionResult.RelatedQuestions = await GetRelatedQuestionsAsync(question.PollId, question.Id, cancellationToken);
    }

    private async Task<List<ChoiceDetailDto>> GetQuestionChoicesAsync(Guid questionId, CancellationToken cancellationToken)
    {
        var choices = await _choiceService.GetActiveChoicesByQuestionIdAsync(questionId, cancellationToken);

        return choices.Select(c => new ChoiceDetailDto
        {
            Id = c.Id,
            QuestionId = c.QuestionId,
            ChoiceText = c.ChoiceText,
            ChoiceOrder = c.ChoiceOrder,
            IsCorrect = c.IsCorrect,
            MediaUrl = c.MediaUrl,
            IsActive = c.IsActive,
            CreatedDateTime = c.CreatedDateTime.DateTime,
            UpdatedDateTime = c.UpdatedDateTime?.DateTime
        }).ToList();
    }

    private async Task<List<RelatedQuestionDto>> GetRelatedQuestionsAsync(Guid pollId, Guid currentQuestionId, CancellationToken cancellationToken)
    {
        var relatedQuestions = await _questionService.GetRelatedQuestionsAsync(pollId, currentQuestionId, cancellationToken);

        return relatedQuestions.Select(q => new RelatedQuestionDto
        {
            Id = q.Id,
            QuestionText = q.QuestionText,
            QuestionType = q.QuestionType.ToString(),
            QuestionOrder = q.QuestionOrder,
            IsRequired = q.IsRequired,
            IsActive = q.IsActive,
            CreatedDateTime = q.CreatedDateTime.DateTime
        }).ToList();
    }
}