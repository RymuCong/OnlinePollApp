using Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using T3H.Poll.Application.Choice.Services;
using T3H.Poll.Application.Polls.Services;
using T3H.Poll.Application.Question.DTOs;
using T3H.Poll.Application.Question.Services;
using T3H.Poll.Domain.Identity;

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
            
            // Validate question order
            if (updateDto.QuestionOrder.HasValue && updateDto.QuestionOrder.Value < 0)
            {
                throw new ValidationException("Thứ tự câu hỏi không được nhỏ hơn 0.");
            }
            
            // Only validate if QuestionType is provided
            if (!string.IsNullOrWhiteSpace(updateDto.QuestionType))
            {
                if (!Enum.TryParse<QuestionType>(updateDto.QuestionType, true, out QuestionType questionType))
                {
                    throw new ValidationException($"Loại câu hỏi không hợp lệ. Loại câu hỏi phải là một trong: {string.Join(", ", ValidQuestionTypes)}");
                }

                // Validate choices based on question type
                if (updateDto.Choices != null)
                {
                    if (questionType == QuestionType.TextInput && updateDto.Choices.Any())
                    {
                        throw new ValidationException($"Câu hỏi loại {questionType} không được có lựa chọn.");
                    }
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

        // Validate question orders before processing
        var questionOrdersToAssign = new Dictionary<Guid, int>();
        var usedOrders = new HashSet<int>();
        
        // Collect all current orders of questions being updated (to exclude from conflict check)
        var currentQuestionOrders = existingQuestions
            .Where(q => questionIds.Contains(q.Id))
            .Select(q => q.QuestionOrder)
            .ToHashSet();

        foreach (var (questionId, updateDto) in command.QuestionUpdates)
        {
            var existingQuestion = existingQuestions.First(q => q.Id == questionId);
            var requestedOrder = updateDto.QuestionOrder ?? existingQuestion.QuestionOrder;

            int finalQuestionOrder;

            if (requestedOrder == 0)
            {
                // Auto-assign next available order
                finalQuestionOrder = await _questionService.GetNextQuestionOrderAsync(command.PollId, cancellationToken);

                // Check if this order conflicts with other questions being updated in this batch
                while (usedOrders.Contains(finalQuestionOrder))
                {
                    finalQuestionOrder++;
                }
            }
            else
            {
                // Validate order is >= 1
                if (requestedOrder < 1)
                {
                    throw new ValidationException($"Thứ tự câu hỏi phải lớn hơn 0. Giá trị nhận được: {requestedOrder}");
                }

                finalQuestionOrder = requestedOrder;

                // Check if this order already exists in the poll 
                // EXCLUDING current question AND other questions being updated in this batch
                if (finalQuestionOrder != existingQuestion.QuestionOrder)
                {
                    var orderExistsInOtherQuestions = await _questionService.GetQueryableSet()
                        .Where(q => q.PollId == command.PollId && 
                                   q.QuestionOrder == finalQuestionOrder && 
                                   q.IsDeleted != true &&
                                   !questionIds.Contains(q.Id)) // Exclude questions being updated
                        .AnyAsync(cancellationToken);

                    if (orderExistsInOtherQuestions)
                    {
                        throw new ValidationException($"Thứ tự câu hỏi {finalQuestionOrder} đã tồn tại trong poll này.");
                    }
                }

                // Check if this order conflicts with other questions being updated in this batch
                if (usedOrders.Contains(finalQuestionOrder))
                {
                    throw new ValidationException($"Thứ tự câu hỏi {finalQuestionOrder} bị trùng lặp trong yêu cầu cập nhật.");
                }
            }

            questionOrdersToAssign[questionId] = finalQuestionOrder;
            usedOrders.Add(finalQuestionOrder);
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
                var questionOrder = questionOrdersToAssign[questionId]; // Use the validated order
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

                // Update choices if provided and question type is not TextInput
                if (updateDto.Choices != null && questionType != QuestionType.TextInput)
                {
                    await UpdateQuestionChoices(questionId, updateDto.Choices, cancellationToken);
                }
            }

            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
    }

    private async Task UpdateQuestionChoices(Guid questionId, ICollection<UpdateChoiceDto> choiceUpdates, CancellationToken cancellationToken)
    {
        if (!choiceUpdates.Any())
            return;

        // Separate choices into existing and new (temp) choices
        var existingChoiceIds = choiceUpdates
            .Where(c => !string.IsNullOrEmpty(c.Id) && !IsTemporaryId(c.Id))
            .Select(c => Guid.Parse(c.Id))
            .ToList();

        var tempChoices = choiceUpdates
            .Where(c => !string.IsNullOrEmpty(c.Id) && IsTemporaryId(c.Id))
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

        // Get max choice order for auto-assignment
        var maxChoiceOrder = await _choiceService.GetQueryableSet()
            .Where(c => c.QuestionId == questionId && c.IsDeleted != true)
            .MaxAsync(c => (int?)c.ChoiceOrder, cancellationToken) ?? 0;

        foreach (var choiceUpdate in choiceUpdates)
        {
            if (!string.IsNullOrEmpty(choiceUpdate.Id) && IsTemporaryId(choiceUpdate.Id))
            {
                // This is a new choice with temporary ID
                if (string.IsNullOrWhiteSpace(choiceUpdate.ChoiceText))
                {
                    throw new ValidationException("ChoiceText bắt buộc phải có khi tạo lựa chọn mới");
                }

                // Auto-assign choice order if not provided or is 0
                var choiceOrder = choiceUpdate.ChoiceOrder ?? 0;
                if (choiceOrder == 0)
                {
                    choiceOrder = ++maxChoiceOrder; // Auto-assign next available order
                }

                var newChoice = new Domain.Entities.Choice(
                    questionId,
                    choiceUpdate.ChoiceText,
                    choiceOrder,
                    choiceUpdate.IsCorrect ?? false,
                    choiceUpdate.MediaUrl ?? string.Empty
                );

                await _choiceService.AddAsync(newChoice, cancellationToken);
            }
            else if (!string.IsNullOrEmpty(choiceUpdate.Id) && Guid.TryParse(choiceUpdate.Id, out var choiceId) && existingChoicesDict.TryGetValue(choiceId, out var existingChoice))
            {
                // Update existing choice
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
            else if (!string.IsNullOrEmpty(choiceUpdate.Id))
            {
                // Invalid choice ID
                throw new ValidationException($"Không tìm thấy lựa chọn với ID {choiceUpdate.Id}");
            }
            else
            {
                // No ID provided - this should not happen based on your requirement
                throw new ValidationException("Tất cả các lựa chọn phải có ID");
            }
        }
    }

    private static bool IsTemporaryId(string id)
    {
        return id.StartsWith("temp-", StringComparison.OrdinalIgnoreCase);
    }
}