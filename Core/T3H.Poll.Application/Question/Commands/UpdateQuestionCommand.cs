using T3H.Poll.Application.Common.Commands;
using Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using T3H.Poll.Application.Choice.Services;
using T3H.Poll.Application.Polls.Services;
using T3H.Poll.Application.Question.DTOs;
using T3H.Poll.Application.Question.Services;
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
    private readonly IQuestionService _questionService;
    private readonly IChoiceService _choiceService;
    private readonly IPollService _pollService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public UpdateQuestionCommandHandler(
        IUnitOfWork unitOfWork,
        IQuestionService questionService,
        IChoiceService choiceService,
        IPollService pollService,
        ICurrentUser currentUser)
    {
        _unitOfWork = unitOfWork;
        _questionService = questionService;
        _choiceService = choiceService;
        _pollService = pollService;
        _currentUser = currentUser;
    }

    public async Task HandleAsync(UpdateQuestionCommand command, CancellationToken cancellationToken = default)
    {
        UpdateQuestionValidator.Validate(command);

        // Check if poll exists and user is authorized
        var poll = await _pollService.GetByIdAsync(command.PollId, cancellationToken);
        if (poll == null)
        {
            throw new NotFoundException($"Poll với ID {command.PollId} không tìm thấy");
        }

        if (poll.CreatorId != _currentUser.UserId)
        {
            throw new ForbiddenException("Bạn chỉ có thể cập nhật câu hỏi cho poll mà bạn đã tạo");
        }

        // Validate existing questions belong to the poll
        var existingQuestionIds = command.QuestionRequests
            .Where(q => q.Id.HasValue)
            .Select(q => q.Id.Value)
            .ToList();

        if (existingQuestionIds.Any())
        {
            var existingQuestions = await _questionService.GetQuestionsByIdsAsync(existingQuestionIds, cancellationToken);

            // Check if all existing questions belong to the specified poll
            var questionsNotInPoll = existingQuestions
                .Where(q => q.PollId != command.PollId)
                .Select(q => q.Id)
                .ToList();

            if (questionsNotInPoll.Any())
            {
                throw new ValidationException($"Các câu hỏi với ID [{string.Join(", ", questionsNotInPoll)}] không thuộc về poll {command.PollId}");
            }

            // Check if some question IDs don't exist
            var foundQuestionIds = existingQuestions.Select(q => q.Id).ToList();
            var notFoundIds = existingQuestionIds.Except(foundQuestionIds).ToList();
            if (notFoundIds.Any())
            {
                throw new NotFoundException($"Không tìm thấy câu hỏi với ID [{string.Join(", ", notFoundIds)}]");
            }
        }

        using (await _unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted, cancellationToken))
        {
            foreach (var questionRequest in command.QuestionRequests)
            {
                // Parse string to enum
                if (!Enum.TryParse<QuestionType>(questionRequest.QuestionType, true, out QuestionType questionType))
                {
                    throw new ValidationException($"Loại câu hỏi không hợp lệ: {questionRequest.QuestionType}");
                }

                if (questionRequest.Id.HasValue)
                {
                    // Update existing question (already validated above)
                    var existingQuestion = await _questionService.GetByIdAsync(questionRequest.Id.Value, cancellationToken);
                    
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
                    // Create new question (always belongs to the specified poll)
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
        }
    }

    private async Task UpdateQuestionChoices(Guid questionId, ICollection<ChoiceRequest>? choiceRequests, CancellationToken cancellationToken)
    {
        if (choiceRequests == null || !choiceRequests.Any())
            return;

        // Similar validation needed for choices belonging to the question
        var existingChoiceIds = choiceRequests
            .Where(c => c.Id.HasValue)
            .Select(c => c.Id.Value)
            .ToList();

        if (existingChoiceIds.Any())
        {
            var existingChoices = await _choiceService.GetQueryableSet()
                .Where(c => existingChoiceIds.Contains(c.Id))
                .ToListAsync(cancellationToken);

            // Check if all existing choices belong to the specified question
            var choicesNotInQuestion = existingChoices
                .Where(c => c.QuestionId != questionId)
                .Select(c => c.Id)
                .ToList();

            if (choicesNotInQuestion.Any())
            {
                throw new ValidationException($"Các lựa chọn với ID [{string.Join(", ", choicesNotInQuestion)}] không thuộc về câu hỏi {questionId}");
            }
        }

        foreach (var choiceModel in choiceRequests)
        {
            if (choiceModel.Id.HasValue)
            {
                // Update existing choice (already validated above)
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
                // Create new choice (always belongs to the specified question)
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