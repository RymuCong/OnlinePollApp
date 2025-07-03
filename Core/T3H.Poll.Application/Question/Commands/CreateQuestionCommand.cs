using T3H.Poll.Domain.Identity;
using Application.Common.Exceptions;
using T3H.Poll.Application.Question.DTOs;

namespace T3H.Poll.Application.Question.Commands;

public class CreateQuestionCommand : ICommand
{
    public Guid PollId { get; set; }
    public ICollection<QuestionRequest> QuestionRequests { get; set; }
    
    public CreateQuestionCommand(Guid pollId, ICollection<QuestionRequest> questionRequests)
    {
        PollId = pollId;
        QuestionRequests = questionRequests ?? throw new ArgumentNullException(nameof(questionRequests), "Question requests collection cannot be null");
        
        if (!questionRequests.Any())
            throw new ArgumentException("At least one question must be provided", nameof(questionRequests));
    }
}

public class CreateQuestionValidator
{
    private static readonly string[] ValidQuestionTypes = Enum.GetNames(typeof(QuestionType));
    
    private static readonly QuestionType[] TypesRequiringChoices = { 
        QuestionType.SingleChoice, 
        QuestionType.MultipleChoice, 
        QuestionType.Ranking, 
        QuestionType.Rating,
        QuestionType.YesNo,
        QuestionType.LongText,
        QuestionType.ShortText
    };
    
    public static void Validate(CreateQuestionCommand command)
    {
        ValidationException.Requires(command.PollId != Guid.Empty, "Vote ID không được để trống.");
        ValidationException.Requires(command.QuestionRequests != null && command.QuestionRequests.Any(), "Phải có ít nhất một câu hỏi.");
        
        foreach (var questionRequest in command.QuestionRequests)
        {
            ValidationException.NotNullOrWhiteSpace(questionRequest.QuestionText, "Nội dung câu hỏi không được để trống.");
            ValidationException.NotNullOrWhiteSpace(questionRequest.QuestionType, "Loại câu hỏi không được để trống.");

            // Parse the string to enum
            if (!Enum.TryParse<QuestionType>(questionRequest.QuestionType, true, out QuestionType questionType))
            {
                throw new ValidationException($"Loại câu hỏi không hợp lệ. Loại câu hỏi phải là một trong: {string.Join(", ", ValidQuestionTypes)}");
            }

            // Use the parsed enum value for comparison
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

internal class CreateQuestionCommandHandler : ICommandHandler<CreateQuestionCommand>
{
    private readonly ICrudService<Domain.Entities.Question> _questionService;
    private readonly ICrudService<Domain.Entities.Choice> _choiceService;
    private readonly ICrudService<Domain.Entities.Poll> _pollService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
 
    public CreateQuestionCommandHandler(
        IUnitOfWork unitOfWork,
        ICrudService<Domain.Entities.Question> questionService,
        ICrudService<Domain.Entities.Choice> choiceService,
        ICrudService<Domain.Entities.Poll> pollService,
        ICurrentUser currentUser)
    {
        _unitOfWork = unitOfWork;
        _questionService = questionService;
        _choiceService = choiceService;
        _pollService = pollService;
        _currentUser = currentUser;
    }

    public async Task HandleAsync(CreateQuestionCommand command, CancellationToken cancellationToken = default)
    {
        CreateQuestionValidator.Validate(command);

        // Check if poll exists and user is authorized
        var poll = await _pollService.GetByIdAsync(command.PollId, cancellationToken);
        if (poll == null)
        {
            throw new NotFoundException($"Poll with ID {command.PollId} not found");
        }

        // Only creator of the poll can add questions
        if (poll.CreatorId != _currentUser.UserId)
        {
            throw new ForbiddenException("Bạn chỉ có thể tạo câu hỏi cho poll mà bạn đã tạo");
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
                
                // Create question
                var question = new Domain.Entities.Question(
                    command.PollId,
                    questionRequest.QuestionText,
                    questionType,
                    questionRequest.IsRequired,
                    questionRequest.QuestionOrder,
                    questionRequest.MediaUrl ?? string.Empty,
                    questionRequest.Settings ?? string.Empty
                );
                
                await _questionService.AddAsync(question);
                
                // Create choices if provided
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
                        
                        await _choiceService.AddAsync(choice);
                    }
                }
            }
            
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
    }
}