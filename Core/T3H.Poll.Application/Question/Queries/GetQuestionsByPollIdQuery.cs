// using Application.Common.Exceptions;
//
// namespace T3H.Poll.Application.Question.Queries;
//
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading;
// using System.Threading.Tasks;
// using T3H.Poll.Application.Common.Queries;
// using T3H.Poll.Application.Question.DTOs;
// using T3H.Poll.Domain.Identity;
// using T3H.Poll.Infrastructure.Caching;
// using Application.Common.Exceptions;
//
// public class GetQuestionsByPollIdQuery : IQuery<List<QuestionDto>>
// {
//     public Guid PollId { get; set; }
// }
//
// internal class GetQuestionsByPollIdQueryHandler : IQueryHandler<GetQuestionsByPollIdQuery, List<QuestionDto>>
// {
//     private readonly IUnitOfWork _unitOfWork;
//     private readonly ICurrentUser _currentUser;
//     private readonly RedisCacheService _cacheService;
//
//     public GetQuestionsByPollIdQueryHandler(
//         IUnitOfWork unitOfWork,
//         ICurrentUser currentUser,
//         RedisCacheService cacheService)
//     {
//         _unitOfWork = unitOfWork;
//         _currentUser = currentUser;
//         _cacheService = cacheService;
//     }
//
//     public async Task<List<QuestionDto>> HandleAsync(GetQuestionsByPollIdQuery query, CancellationToken cancellationToken = default)
//     {
//         // Check for cached result
//         var cacheKey = $"GetQuestionsByPollId:{query.PollId}";
//         var cachedResult = await _cacheService.GetAsync<List<QuestionDto>>(cacheKey);
//         if (cachedResult != null)
//         {
//             return cachedResult;
//         }
//
//         // Check if poll exists and user has access
//         var poll = await _unitOfWork.Repository<Domain.Entities.Poll>().GetByIdAsync(query.PollId, cancellationToken);
//         if (poll == null)
//         {
//             throw new NotFoundException($"Poll with ID {query.PollId} not found");
//         }
//
//         // If poll is not public, check if user is the creator
//         if (!poll.IsPublic && poll.CreatorId != _currentUser.UserId)
//         {
//             throw new ForbiddenException("You do not have permission to view questions for this poll");
//         }
//
//         // Get questions for the poll
//         var questions = await _unitOfWork.Repository<Domain.Entities.Question>()
//             .FindAsync(q => q.VoteId == query.PollId, cancellationToken);
//
//         // Get choices for all questions in one query
//         var questionIds = questions.Select(q => q.Id).ToList();
//         var choices = await _unitOfWork.Repository<Domain.Entities.Choice>()
//             .FindAsync(c => questionIds.Contains(c.QuestionId) && c.IsActive == true, cancellationToken);
//
//         // Map to DTOs
//         var result = questions.Select(q => new QuestionDto
//         {
//             Id = q.Id,
//             VoteId = q.VoteId,
//             QuestionText = q.QuestionText,
//             QuestionType = q.QuestionType,
//             IsRequired = q.IsRequired,
//             QuestionOrder = q.QuestionOrder,
//             MediaUrl = q.MediaUrl,
//             Settings = q.Settings,
//             CreatedDateTime = q.CreatedDateTime,
//             UserNameCreated = q.UserNameCreated,
//             UpdatedDateTime = q.UpdatedDateTime,
//             UserNameUpdated = q.UserNameUpdated,
//             Choices = choices
//                 .Where(c => c.QuestionId == q.Id)
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
//         })
//         .OrderBy(q => q.QuestionOrder)
//         .ToList();
//
//         // Cache the result
//         await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(10));
//
//         return result;
//     }
// }
