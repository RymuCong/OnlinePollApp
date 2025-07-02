using Application.Common.Exceptions;
using T3H.Poll.Domain.Identity;
using T3H.Poll.Application.Polls.Services;
using T3H.Poll.Application.Question.Services;

namespace T3H.Poll.Application.Question.Commands;

public class DeleteQuestionCommand : ICommand
{
    public Guid PollId { get; set; }
    public List<Guid> QuestionIds { get; set; }

    public DeleteQuestionCommand(Guid pollId, List<Guid> questionIds)
    {
        PollId = pollId;
        QuestionIds = questionIds ?? throw new ArgumentNullException(nameof(questionIds), "Danh sách Id không được để trống.");
    }
}

public class DeleteQuestionCommandValidator
{
    public static void Validate(DeleteQuestionCommand request)
    {
        if (request.PollId == Guid.Empty)
        {
            throw new ValidationException("Poll ID không được để trống.");
        }
        
        if (request.QuestionIds == null || request.QuestionIds.Count == 0)
        {
            throw new ValidationException("Danh sách Question ID không được để trống.");
        }
    }
}

internal class DeleteQuestionCommandHandler : ICommandHandler<DeleteQuestionCommand>
{
    private readonly IQuestionService _questionService;
    private readonly IPollService _pollService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    
    public DeleteQuestionCommandHandler(
        IQuestionService questionService,
        IPollService pollService,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _questionService = questionService;
        _pollService = pollService;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task HandleAsync(DeleteQuestionCommand command, CancellationToken cancellationToken = default)
    {
        DeleteQuestionCommandValidator.Validate(command);

        // First check if poll exists and user is authorized
        var poll = await _pollService.GetByIdAsync(command.PollId, cancellationToken);
        if (poll == null)
        {
            throw new NotFoundException($"Poll với ID {command.PollId} không tìm thấy.");
        }

        if (poll.CreatorId != _currentUser.UserId)
        {
            throw new ForbiddenException("Bạn chỉ có thể xóa câu hỏi cho poll mà bạn đã tạo");
        }

        // Get questions with choices, filtering by both question IDs and poll ID
        var questions = await _questionService.GetQuestionsWithChoicesByIdsAsync(command.QuestionIds, cancellationToken);
        
        // Filter questions that actually belong to the specified poll
        var questionsInPoll = questions.Where(q => q.PollId == command.PollId).ToList();

        if (questionsInPoll.Count == 0)
        {
            throw new ValidationException($"Không tìm thấy câu hỏi nào thuộc poll {command.PollId} cần xóa!");
        }

        // Check if some questions don't belong to this poll
        if (questionsInPoll.Count != command.QuestionIds.Count)
        {
            var foundQuestionIds = questionsInPoll.Select(q => q.Id).ToList();
            var notFoundIds = command.QuestionIds.Except(foundQuestionIds).ToList();
            throw new ValidationException($"Các câu hỏi với ID [{string.Join(", ", notFoundIds)}] không tồn tại trong poll {command.PollId}.");
        }

        using (await _unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted, cancellationToken))
        {
            await _questionService.SoftDeleteQuestionsAndChoicesAsync(questionsInPoll, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
    }
}