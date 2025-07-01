using T3H.Poll.Application.Common.Commands;
using Application.Common.Exceptions;
using T3H.Poll.Application.Question.DTOs;
using T3H.Poll.Domain.Identity;
using T3H.Poll.Infrastructure.Caching;

namespace T3H.Poll.Application.Question.Commands;

public class UpdateQuestionCommand : ICommand
{
    public Guid PollId { get; set; }
    public ICollection<QuestionUpdateRequest> QuestionRequests { get; set; }

    public UpdateQuestionCommand(Guid pollId, ICollection<QuestionUpdateRequest> questionRequests)
    {
        PollId = pollId;
        QuestionRequests = questionRequests ?? throw new ArgumentNullException(nameof(questionRequests), "Question requests collection cannot be null");

        if (!questionRequests.Any())
            throw new ArgumentException("At least one question must be provided", nameof(questionRequests));
    }
}

public class QuestionUpdateRequest : QuestionRequest
{
    public Guid? Id { get; set; } // Null for new questions, existing ID for updates
    public bool? IsActive { get; set; } // To handle soft delete
}

public class UpdateQuestionValidator
{
    private static readonly string[] ValidQuestionTypes = Enum.GetNames(typeof(QuestionType));

    private static readonly QuestionType[] TypesRequiringChoices = {
        QuestionType.SingleChoice,
        QuestionType.MultiChoice,
        QuestionType.Ranking,
        QuestionType.Rating,
        QuestionType.YesNo,
        QuestionType.LongText,
        QuestionType.ShortText
    };

    public static void Validate(UpdateQuestionCommand command)
    {
        ValidationException.Requires(command.PollId != Guid.Empty, "Poll ID không được để trống.");
        ValidationException.Requires(command.QuestionRequests != null && command.QuestionRequests.Any(), "Phải có ít nhất một câu hỏi.");

        foreach (var questionRequest in command.QuestionRequests)
        {
            ValidationException.NotNullOrWhiteSpace(questionRequest.QuestionText, "Nội dung câu hỏi không được để trống.");
            ValidationException.NotNullOrWhiteSpace(questionRequest.QuestionType, "Loại câu hỏi không được để trống.");

            if (!Enum.TryParse<QuestionType>(questionRequest.QuestionType, true, out QuestionType questionType))
            {
                throw new ValidationException($"Loại câu hỏi không hợp lệ. Loại câu hỏi phải là một trong: {string.Join(", ", ValidQuestionTypes)}");
            }

            if (TypesRequiringChoices.Contains(questionType))
            {
                if (questionRequest.Choices == null || !questionRequest.Choices.Any())
                {
                    throw new ValidationException($"Câu hỏi loại {questionType} phải có ít nhất một lựa chọn.");
                }

                foreach (var choice in questionRequest.Choices)
                {
                    ValidationException.NotNullOrWhiteSpace(choice.ChoiceText, "Nội dung lựa chọn không được để trống.");
                }
            }
        }
    }
}

internal class UpdateQuestionCommandHandler : ICommandHandler<UpdateQuestionCommand>
{
    private readonly ICrudService<Domain.Entities.Question> _questionService;
    private readonly ICrudService<Domain.Entities.Choice> _choiceService;
    private readonly ICrudService<Domain.Entities.Poll> _pollService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly RedisCacheService _cacheService;

    public UpdateQuestionCommandHandler(
        IUnitOfWork unitOfWork,
        ICrudService<Domain.Entities.Question> questionService,
        ICrudService<Domain.Entities.Choice> choiceService,
        ICrudService<Domain.Entities.Poll> pollService,
        ICurrentUser currentUser,
        RedisCacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _questionService = questionService;
        _choiceService = choiceService;
        _pollService = pollService;
        _currentUser = currentUser;
        _cacheService = cacheService;
    }

    public async Task HandleAsync(UpdateQuestionCommand command, CancellationToken cancellationToken = default)
    {
        UpdateQuestionValidator.Validate(command);

        // Check if poll exists and user is authorized
        var poll = await _pollService.GetByIdAsync(command.PollId, cancellationToken);
        if (poll == null)
        {
            throw new NotFoundException($"Poll with ID {command.PollId} not found");
        }

        // Only creator of the poll can update questions
        if (poll.CreatorId != _currentUser.UserId)
        {
            throw new ForbiddenException("Bạn chỉ có thể cập nhật câu hỏi cho poll mà bạn đã tạo");
        }

        using (await _unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted, cancellationToken))
        {
            foreach (var questionRequest in command.QuestionRequests)
            {
                // Parse string to enum
                if (!Enum.TryParse<QuestionType>(questionRequest.QuestionType, true, out QuestionType questionType))
                {
                    throw new ValidationException($"Invalid question type: {questionRequest.QuestionType}");
                }

                if (questionRequest.Id.HasValue)
                {
                    // Update existing question
                    var existingQuestion = await _questionService.GetByIdAsync(questionRequest.Id.Value, cancellationToken);
                    if (existingQuestion == null)
                    {
                        throw new NotFoundException($"Question with ID {questionRequest.Id.Value} not found");
                    }

                    existingQuestion.UpdateQuestion(
                        questionRequest.QuestionText,
                        questionType,
                        questionRequest.IsRequired,
                        questionRequest.QuestionOrder,
                        questionRequest.MediaUrl ?? string.Empty,
                        questionRequest.Settings ?? string.Empty
                    );

                    if (questionRequest.IsActive.HasValue && !questionRequest.IsActive.Value)
                    {
                        existingQuestion.DeactivateQuestion();
                    }

                    await _questionService.UpdateAsync(existingQuestion, cancellationToken);
                    
                    // Update choices for existing question
                    await UpdateQuestionChoices(existingQuestion.Id, questionRequest.Choices, cancellationToken);
                }
                else
                {
                    // Create new question
                    var question = new Domain.Entities.Question(
                        command.PollId,
                        questionRequest.QuestionText,
                        questionType,
                        questionRequest.IsRequired,
                        questionRequest.QuestionOrder,
                        questionRequest.MediaUrl ?? string.Empty,
                        questionRequest.Settings ?? string.Empty
                    );

                    await _questionService.AddAsync(question, cancellationToken);

                    // Create choices for new question
                    if (questionRequest.Choices != null && questionRequest.Choices.Any())
                    {
                        foreach (var choiceModel in questionRequest.Choices)
                        {
                            var choice = new Domain.Entities.Choice(
                                question.Id,
                                choiceModel.ChoiceText,
                                choiceModel.ChoiceOrder,
                                choiceModel.IsCorrect,
                                choiceModel.MediaUrl ?? string.Empty
                            );

                            await _choiceService.AddAsync(choice, cancellationToken);
                        }
                    }
                }
            }

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // Clear cache
            var redisKey = $"GetQuestionsByPollId:{command.PollId}";
            await _cacheService.RemoveAsync(redisKey);
        }
    }

    private async Task UpdateQuestionChoices(Guid questionId, ICollection<ChoiceRequest>? choiceRequests, CancellationToken cancellationToken)
    {
        if (choiceRequests == null || !choiceRequests.Any())
            return;

        foreach (var choiceModel in choiceRequests)
        {
            if (choiceModel.Id.HasValue)
            {
                // Update existing choice
                var existingChoice = await _choiceService.GetByIdAsync(choiceModel.Id.Value, cancellationToken);
                if (existingChoice != null)
                {
                    existingChoice.UpdateChoice(
                        choiceModel.ChoiceText,
                        choiceModel.ChoiceOrder,
                        choiceModel.IsCorrect,
                        choiceModel.MediaUrl ?? string.Empty
                    );

                    if (choiceModel.IsActive.HasValue && !choiceModel.IsActive.Value)
                    {
                        existingChoice.DeactivateChoice();
                    }

                    await _choiceService.UpdateAsync(existingChoice, cancellationToken);
                }
            }
            else
            {
                // Create new choice
                var newChoice = new Domain.Entities.Choice(
                    questionId,
                    choiceModel.ChoiceText,
                    choiceModel.ChoiceOrder,
                    choiceModel.IsCorrect,
                    choiceModel.MediaUrl ?? string.Empty
                );

                await _choiceService.AddAsync(newChoice, cancellationToken);
            }
        }
    }
}