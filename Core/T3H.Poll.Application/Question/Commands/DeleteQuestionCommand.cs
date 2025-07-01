// using T3H.Poll.Application.Common.Commands;
// using Application.Common.Exceptions;
// using T3H.Poll.Domain.Identity;
// using T3H.Poll.Infrastructure.Caching;
//
// namespace T3H.Poll.Application.Question.Commands;
//
// public class DeleteQuestionCommand : ICommand<bool>
// {
//     public Guid Id { get; set; }
// }
//
// internal class DeleteQuestionCommandHandler : ICommandHandler<DeleteQuestionCommand, bool>
// {
//     private readonly ICrudService<Domain.Entities.Question> _questionService;
//     private readonly IUnitOfWork _unitOfWork;
//     private readonly ICurrentUser _currentUser;
//     private readonly RedisCacheService _cacheService;
//
//     public DeleteQuestionCommandHandler(
//         ICrudService<Domain.Entities.Question> questionService,
//         IUnitOfWork unitOfWork,
//         ICurrentUser currentUser,
//         RedisCacheService cacheService)
//     {
//         _questionService = questionService;
//         _unitOfWork = unitOfWork;
//         _currentUser = currentUser;
//         _cacheService = cacheService;
//     }
//
//     public async Task<bool> HandleAsync(DeleteQuestionCommand command, CancellationToken cancellationToken = default)
//     {
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
//         // Authorization check - only allow the creator to delete poll questions
//         if (poll.CreatorId != _currentUser.UserId)
//         {
//             throw new ForbiddenException("Bạn chỉ có thể xóa câu hỏi cho poll mà bạn đã tạo");
//         }
//
//         using (await _unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted, cancellationToken))
//         {
//             // Get all choices for this question
//             var choices = await _unitOfWork.Repository<Domain.Entities.Choice>()
//                 .FindAsync(c => c.QuestionId == command.Id, cancellationToken);
//
//             // Delete all choices first
//             foreach (var choice in choices)
//             {
//                 await _unitOfWork.Repository<Domain.Entities.Choice>().DeleteAsync(choice, cancellationToken);
//             }
//
//             // Delete the question
//             await _questionService.DeleteAsync(existingQuestion, cancellationToken);
//             await _unitOfWork.CommitTransactionAsync(cancellationToken);
//             
//             // Clear cache
//             var redisKey = $"GetQuestionsByPollId:{pollId}";
//             await _cacheService.RemoveAsync(redisKey);
//             
//             return true;
//         }
//     }
// }
