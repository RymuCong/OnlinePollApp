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
    public Dictionary<Guid, UpdateQuestionDto> QuestionUpdates { get; set; }

    public UpdateQuestionCommand(Guid pollId, Dictionary<Guid, UpdateQuestionDto> questionUpdates)
    {
        PollId = pollId;
        QuestionUpdates = questionUpdates ?? throw new ArgumentNullException(nameof(questionUpdates));

        if (!questionUpdates.Any())
            throw new ArgumentException("At least one question update must be provided", nameof(questionUpdates));
    }
}

public class UpdateQuestionValidator
{
    private static readonly string[] ValidQuestionTypes = Enum.GetNames(typeof(QuestionType));

    public static void Validate(UpdateQuestionCommand command)
    {
        ValidationException.Requires(command.PollId != Guid.Empty, "Poll ID không được để trống.");
        ValidationException.Requires(command.QuestionUpdates != null && command.QuestionUpdates.Any(), "Phải có ít nhất một câu hỏi cần cập nhật.");

        foreach (var (questionId, updateDto) in command.QuestionUpdates)
        {
            ValidationException.Requires(questionId != Guid.Empty, "Question ID không được để trống.");
            
            // Only validate if QuestionType is provided
            if (!string.IsNullOrWhiteSpace(updateDto.QuestionType))
            {
                if (!Enum.TryParse<QuestionType>(updateDto.QuestionType, true, out _))
                {
                    throw new ValidationException($"Loại câu hỏi không hợp lệ. Loại câu hỏi phải là một trong: {string.Join(", ", ValidQuestionTypes)}");
                }
            }

            // Only validate choices if they are provided
            if (updateDto.Choices != null)
            {
                foreach (var choice in updateDto.Choices)
                {
                    if (!string.IsNullOrWhiteSpace(choice.ChoiceText) && string.IsNullOrWhiteSpace(choice.ChoiceText.Trim()))
                    {
                        throw new ValidationException("Nội dung lựa chọn không được để trống.");
                    }
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

        var questionIds = command.QuestionUpdates.Keys.ToList();
        var existingQuestions = await _questionService.GetQuestionsByIdsAsync(questionIds, cancellationToken);

        // Check if all questions belong to the specified poll
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
        var notFoundIds = questionIds.Except(foundQuestionIds).ToList();
        if (notFoundIds.Any())
        {
            throw new NotFoundException($"Không tìm thấy câu hỏi với ID [{string.Join(", ", notFoundIds)}]");
        }

        using (await _unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted, cancellationToken))
        {
            foreach (var (questionId, updateDto) in command.QuestionUpdates)
            {
                var existingQuestion = existingQuestions.First(q => q.Id == questionId);

                // Only update properties that are provided (not null)
                var questionText = updateDto.QuestionText ?? existingQuestion.QuestionText;
                var questionType = existingQuestion.QuestionType;
                
                if (!string.IsNullOrWhiteSpace(updateDto.QuestionType))
                {
                    if (!Enum.TryParse<QuestionType>(updateDto.QuestionType, true, out questionType))
                    {
                        throw new ValidationException($"Loại câu hỏi không hợp lệ: {updateDto.QuestionType}");
                    }
                }

                var isRequired = updateDto.IsRequired ?? existingQuestion.IsRequired;
                var questionOrder = updateDto.QuestionOrder ?? existingQuestion.QuestionOrder;
                var mediaUrl = updateDto.MediaUrl ?? existingQuestion.MediaUrl;
                var settings = updateDto.Settings ?? existingQuestion.Settings;

                existingQuestion.UpdateQuestion(
                    questionText,
                    questionType,
                    isRequired,
                    questionOrder,
                    mediaUrl,
                    settings
                );

                // Handle soft delete/activate
                if (updateDto.IsActive.HasValue)
                {
                    if (!updateDto.IsActive.Value)
                    {
                        existingQuestion.IsDeleted = true;
                    }
                    else
                    {
                        existingQuestion.IsDeleted = false;
                    }
                }

                await _questionService.UpdateAsync(existingQuestion, cancellationToken);

                // Update choices if provided
                if (updateDto.Choices != null)
                {
                    await UpdateQuestionChoices(existingQuestion.Id, updateDto.Choices, cancellationToken);
                }
            }

            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
    }

    private async Task UpdateQuestionChoices(Guid questionId, ICollection<UpdateChoiceDto> choiceUpdates, CancellationToken cancellationToken)
    {
        if (!choiceUpdates.Any())
            return;

        var existingChoiceIds = choiceUpdates
            .Where(c => c.Id.HasValue)
            .Select(c => c.Id.Value)
            .ToList();

        Dictionary<Guid, Domain.Entities.Choice> existingChoicesDict = new();

        if (existingChoiceIds.Any())
        {
            var existingChoices = await _choiceService.GetQueryableSet()
                .Where(c => existingChoiceIds.Contains(c.Id))
                .ToListAsync(cancellationToken);

            var choicesNotInQuestion = existingChoices
                .Where(c => c.QuestionId != questionId)
                .Select(c => c.Id)
                .ToList();

            if (choicesNotInQuestion.Any())
            {
                throw new ValidationException($"Các lựa chọn với ID [{string.Join(", ", choicesNotInQuestion)}] không thuộc về câu hỏi {questionId}");
            }

            existingChoicesDict = existingChoices.ToDictionary(c => c.Id, c => c);
        }

        foreach (var choiceUpdate in choiceUpdates)
        {
            if (choiceUpdate.Id.HasValue && existingChoicesDict.TryGetValue(choiceUpdate.Id.Value, out var existingChoice))
            {
                // Update existing choice with only provided properties
                var choiceText = choiceUpdate.ChoiceText ?? existingChoice.ChoiceText;
                var choiceOrder = choiceUpdate.ChoiceOrder ?? existingChoice.ChoiceOrder;
                var isCorrect = choiceUpdate.IsCorrect ?? existingChoice.IsCorrect;
                var mediaUrl = choiceUpdate.MediaUrl ?? existingChoice.MediaUrl;

                existingChoice.UpdateChoice(choiceText, choiceOrder, isCorrect, mediaUrl);

                // Handle soft delete/activate
                if (choiceUpdate.IsActive.HasValue)
                {
                    if (!choiceUpdate.IsActive.Value)
                    {
                        existingChoice.IsDeleted = true;
                    }
                    else
                    {
                        existingChoice.IsDeleted = false;
                    }
                }

                await _choiceService.UpdateAsync(existingChoice, cancellationToken);
            }
            else if (!choiceUpdate.Id.HasValue)
            {
                // Create new choice - all required properties must be provided
                if (string.IsNullOrWhiteSpace(choiceUpdate.ChoiceText))
                {
                    throw new ValidationException("ChoiceText is required for new choices");
                }

                var newChoice = new Domain.Entities.Choice(
                    questionId,
                    choiceUpdate.ChoiceText,
                    choiceUpdate.ChoiceOrder ?? 0,
                    choiceUpdate.IsCorrect ?? false,
                    choiceUpdate.MediaUrl ?? string.Empty
                );

                await _choiceService.AddAsync(newChoice, cancellationToken);
            }
        }
    }
}