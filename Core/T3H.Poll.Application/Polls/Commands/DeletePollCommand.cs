using Application.Common.Exceptions;
using T3H.Poll.Application.Choice.Services;
using T3H.Poll.Application.Polls.Services;
using T3H.Poll.Application.Question.Services;
using T3H.Poll.Domain.Identity;

namespace T3H.Poll.Application.Polls.Commands;

public class DeletePollCommand : ICommand
{
    public Guid PollId { get; set; }

    public DeletePollCommand(Guid pollId)
    {
        PollId = pollId;
    }

    public static void Validate(DeletePollCommand request)
    {
        if (request.PollId == Guid.Empty)
        {
            throw new ValidationException("Poll ID không hợp lệ.");
        }
    }
}

public class DeletePollCommandHandler : ICommandHandler<DeletePollCommand>
{
    private readonly IPollService _pollService;
    private readonly IQuestionService _questionService;
    private readonly IChoiceService _choiceService;
    private readonly ICurrentUser _currentUser;

    public DeletePollCommandHandler(
        IPollService pollService,
        IQuestionService questionService,
        IChoiceService choiceService,
        ICurrentUser currentUser)
    {
        _pollService = pollService;
        _questionService = questionService;
        _choiceService = choiceService;
        _currentUser = currentUser;
    }

    public async Task HandleAsync(DeletePollCommand command, CancellationToken cancellationToken = default)
    {
        DeletePollCommand.Validate(command);

        // Get poll to verify existence and ownership
        var poll = await _pollService.GetByIdAsync(command.PollId, cancellationToken);
        if (poll == null)
        {
            throw new NotFoundException($"Không tìm thấy poll với ID {command.PollId}");
        }

        // Check if current user is the creator
        if (poll.CreatorId != _currentUser.UserId)
        {
            throw new ForbiddenException("Bạn chỉ có thể xóa poll do bạn tạo ra");
        }

        // Check if poll is already deleted
        if (poll.IsDeleted)
        {
            throw new ValidationException("Poll đã được xóa trước đó");
        }

        // Soft delete poll and all related data
        await SoftDeletePollAndRelatedDataAsync(command.PollId, cancellationToken);
    }

    private async Task SoftDeletePollAndRelatedDataAsync(Guid pollId, CancellationToken cancellationToken)
    {
        // Get all questions in the poll
        var questions = await _questionService.GetQuestionsByPollIdAsync(pollId, cancellationToken);

        // Soft delete all choices for each question
        foreach (var question in questions)
        {
            var choices = await _choiceService.GetChoicesByQuestionIdAsync(question.Id, cancellationToken);
            if (choices.Any())
            {
                await _choiceService.SoftDeleteChoicesAsync(choices.Select(c => c.Id).ToList(), cancellationToken);
            }
        }

        // Soft delete all questions
        if (questions.Any())
        {
            await _questionService.SoftDeleteQuestionsAsync(questions.Select(q => q.Id).ToList(), cancellationToken);
        }

        // Soft delete the poll
        await _pollService.SoftDeleteAsync(pollId, cancellationToken);
    }
}