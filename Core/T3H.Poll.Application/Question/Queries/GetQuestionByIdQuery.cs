// namespace T3H.Poll.Application.Question.Queries;
//
// using System;
// using System.Linq;
// using System.Threading;
// using System.Threading.Tasks;
// using T3H.Poll.Application.Common.Queries;
// using T3H.Poll.Application.Question.DTOs;
// using T3H.Poll.Domain.Identity;
// using Application.Common.Exceptions;
//
// public class GetQuestionByIdQuery : IQuery<QuestionDto>
// {
//     public Guid Id { get; set; }
// }
//
// internal class GetQuestionByIdQueryHandler : IQueryHandler<GetQuestionByIdQuery, QuestionDto>
// {
//     private readonly IUnitOfWork _unitOfWork;
//     private readonly ICurrentUser _currentUser;
//
//     public GetQuestionByIdQueryHandler(
//         IUnitOfWork unitOfWork,
//         ICurrentUser currentUser)
//     {
//         _unitOfWork = unitOfWork;
//         _currentUser = currentUser;
//     }
//
//     public async Task<QuestionDto> HandleAsync(GetQuestionByIdQuery query, CancellationToken cancellationToken = default)
//     {
//         // Get question
//         var question = await _unitOfWork.Repository<Domain.Entities.Question>().GetByIdAsync(query.Id, cancellationToken);
//         if (question == null)
//         {
//             throw new NotFoundException($"Câu hỏi với ID {query.Id} không tìm thấy");
//         }
//
//         // Check if poll exists and user has access
//         var poll = await _unitOfWork.Repository<Domain.Entities.Poll>().GetByIdAsync(question.VoteId, cancellationToken);
//         if (poll == null)
//         {
//             throw new NotFoundException($"Poll with ID {question.VoteId} not found");
//         }
//
//         // If poll is not public, check if user is the creator
//         if (!poll.IsPublic && poll.CreatorId != _currentUser.UserId)
//         {
//             throw new ForbiddenException("Bạn không có quyền xem câu hỏi này");
//         }
//
//         // Get choices for the question
//         var choices = await _unitOfWork.Repository<Domain.Entities.Choice>()
//             .FindAsync(c => c.QuestionId == query.Id && c.IsActive == true, cancellationToken);
//
//         // Map to DTO
//         var result = new QuestionDto
//         {
//             Id = question.Id,
//             VoteId = question.VoteId,
//             QuestionText = question.QuestionText,
//             QuestionType = question.QuestionType,
//             IsRequired = question.IsRequired,
//             QuestionOrder = question.QuestionOrder,
//             MediaUrl = question.MediaUrl,
//             Settings = question.Settings,
//             CreatedDateTime = question.CreatedDateTime,
//             UserNameCreated = question.UserNameCreated,
//             UpdatedDateTime = question.UpdatedDateTime,
//             UserNameUpdated = question.UserNameUpdated,
//             Choices = choices
//                 .Select(c => new ChoiceDto
//                 {
//                     Id = c.Id,
//                     QuestionId = c.QuestionId,
//                     ChoiceText = c.ChoiceText,
//                     ChoiceOrder = c.ChoiceOrder,
//                     IsCorrect = c.IsCorrect,
//                     MediaUrl = c.MediaUrl,
//                     IsActive = c.IsActive,
//                     CreatedDateTime = c.CreatedDateTime,
//                     UserNameCreated = c.UserNameCreated,
//                     UpdatedDateTime = c.UpdatedDateTime,
//                     UserNameUpdated = c.UserNameUpdated
//                 })
//                 .OrderBy(c => c.ChoiceOrder)
//                 .ToList()
//         };
//
//         return result;
//     }
// }
