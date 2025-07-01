// using T3H.Poll.Application.Common.Commands;
// using Application.Common.Exceptions;
// using T3H.Poll.Domain.Identity;
// using T3H.Poll.Infrastructure.Caching;
//
// namespace T3H.Poll.Application.Question.Commands;
//
// public class UpdateQuestionCommand : ICommand<bool>
// {
//     public Guid Id { get; set; }
//     public string QuestionText { get; set; }
//     public string QuestionType { get; set; }
//     public bool IsRequired { get; set; }
//     public int QuestionOrder { get; set; }
//     public string MediaUrl { get; set; }
//     public string Settings { get; set; }
//     public List<UpdateChoiceModel> Choices { get; set; } = new List<UpdateChoiceModel>();
// }
//
// public class UpdateChoiceModel
// {
//     public Guid? Id { get; set; } // Optional for new choices
//     public string ChoiceText { get; set; }
//     public int? ChoiceOrder { get; set; }
//     public bool? IsCorrect { get; set; }
//     public string MediaUrl { get; set; }
//     public bool? IsActive { get; set; } = true;
// }
//
// public class UpdateQuestionValidator
// {
//     public static void Validate(UpdateQuestionCommand command)
//     {
//         ValidationException.Requires(command.Id != Guid.Empty, "Question ID không được để trống.");
//         ValidationException.NotNullOrWhiteSpace(command.QuestionText, "Nội dung câu hỏi không được để trống.");
//         ValidationException.NotNullOrWhiteSpace(command.QuestionType, "Loại câu hỏi không được để trống.");
//         
//         // Validate question type
//         string[] validTypes = { "MultipleChoice", "SingleChoice", "TextInput", "Rating", "YesNo", "Ranking" };
//         if (!validTypes.Contains(command.QuestionType))
//         {
//             throw new ValidationException($"Loại câu hỏi không hợp lệ. Loại câu hỏi phải là một trong: {string.Join(", ", validTypes)}");
//         }
//         
//         // Validate choices for question types that require them
//         if (new[] { "MultipleChoice", "SingleChoice", "Ranking" }.Contains(command.QuestionType))
//         {
//             if (command.Choices == null || !command.Choices.Any())
//             {
//                 throw new ValidationException($"Câu hỏi loại {command.QuestionType} phải có ít nhất một lựa chọn.");
//             }
//             
//             foreach (var choice in command.Choices)
//             {
//                 ValidationException.NotNullOrWhiteSpace(choice.ChoiceText, "Nội dung lựa chọn không được để trống.");
//             }
//         }
//     }
// }
//
// internal class UpdateQuestionCommandHandler : ICommandHandler<UpdateQuestionCommand, bool>
// {
//     private readonly ICrudService<Domain.Entities.Question> _questionService;
//     private readonly ICrudService<Domain.Entities.Choice> _choiceService;
//     private readonly IUnitOfWork _unitOfWork;
//     private readonly ICurrentUser _currentUser;
//     private readonly RedisCacheService _cacheService;
//
//     public UpdateQuestionCommandHandler(
//         ICrudService<Domain.Entities.Question> questionService,
//         ICrudService<Domain.Entities.Choice> choiceService,
//         IUnitOfWork unitOfWork,
//         ICurrentUser currentUser,
//         RedisCacheService cacheService)
//     {
//         _questionService = questionService;
//         _choiceService = choiceService;
//         _unitOfWork = unitOfWork;
//         _currentUser = currentUser;
//         _cacheService = cacheService;
//     }
//
//     public async Task<bool> HandleAsync(UpdateQuestionCommand command, CancellationToken cancellationToken = default)
//     {
//         UpdateQuestionValidator.Validate(command);
//
//         // Get existing question
//         var existingQuestion = await _questionService.GetByIdAsync(command.Id, cancellationToken);
//
//         if (existingQuestion == null)
//         {
//             throw new NotFoundException($"Câu hỏi với ID {command.Id} không tìm thấy");
//         }
//
//         // Check if user is the creator of the poll
//         var pollId = existingQuestion.VoteId;
//         var poll = await _unitOfWork.Repository<Domain.Entities.Poll>().GetByIdAsync(pollId, cancellationToken);
//         
//         if (poll == null)
//         {
//             throw new NotFoundException($"Poll with ID {pollId} not found");
//         }
//
//         // Authorization check - only allow the creator to update poll questions
//         if (poll.CreatorId != _currentUser.UserId)
//         {
//             throw new ForbiddenException("Bạn chỉ có thể cập nhật câu hỏi cho poll mà bạn đã tạo");
//         }
//
//         using (await _unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted, cancellationToken))
//         {
//             // Update question properties
//             existingQuestion.UpdateQuestion(
//                 command.QuestionText,
//                 command.QuestionType,
//                 command.IsRequired,
//                 command.QuestionOrder,
//                 command.MediaUrl,
//                 command.Settings,
//                 _currentUser.UserName
//             );
//
//             await _questionService.UpdateAsync(existingQuestion, cancellationToken);
//
//             // Get existing choices
//             var existingChoices = await _unitOfWork.Repository<Domain.Entities.Choice>()
//                 .FindAsync(c => c.QuestionId == command.Id, cancellationToken);
//             var existingChoiceDict = existingChoices.ToDictionary(c => c.Id, c => c);
//
//             // Process choices
//             foreach (var choiceModel in command.Choices)
//             {
//                 if (choiceModel.Id.HasValue && existingChoiceDict.ContainsKey(choiceModel.Id.Value))
//                 {
//                     // Update existing choice
//                     var existingChoice = existingChoiceDict[choiceModel.Id.Value];
//                     existingChoice.UpdateChoice(
//                         choiceModel.ChoiceText,
//                         choiceModel.ChoiceOrder,
//                         choiceModel.IsCorrect,
//                         choiceModel.MediaUrl,
//                         _currentUser.UserName
//                     );
//                     
//                     if (choiceModel.IsActive.HasValue && !choiceModel.IsActive.Value)
//                     {
//                         existingChoice.DeactivateChoice();
//                     }
//                     
//                     await _choiceService.UpdateAsync(existingChoice, cancellationToken);
//                     
//                     // Remove from dictionary to track which ones were processed
//                     existingChoiceDict.Remove(choiceModel.Id.Value);
//                 }
//                 else
//                 {
//                     // Create new choice
//                     var newChoice = new Domain.Entities.Choice(
//                         command.Id,
//                         choiceModel.ChoiceText,
//                         choiceModel.ChoiceOrder,
//                         choiceModel.IsCorrect,
//                         choiceModel.MediaUrl,
//                         _currentUser.UserName
//                     );
//                     
//                     await _choiceService.AddAsync(newChoice, cancellationToken);
//                 }
//             }
//
//             // Deactivate choices that were not included in the update
//             foreach (var unusedChoice in existingChoiceDict.Values)
//             {
//                 unusedChoice.DeactivateChoice();
//                 await _choiceService.UpdateAsync(unusedChoice, cancellationToken);
//             }
//
//             await _unitOfWork.CommitTransactionAsync(cancellationToken);
//             
//             // Clear cache
//             var redisKey = $"GetQuestionsByPollId:{existingQuestion.VoteId}";
//             await _cacheService.RemoveAsync(redisKey);
//             
//             return true;
//         }
//     }
// }
