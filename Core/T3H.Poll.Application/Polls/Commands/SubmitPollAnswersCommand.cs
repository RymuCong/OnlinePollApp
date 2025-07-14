using Microsoft.EntityFrameworkCore;
using T3H.Poll.Application.Polls.DTOs;

namespace T3H.Poll.Application.Polls.Commands;

public class SubmitPollAnswersCommand : ICommand
{
    public Guid PollId { get; set; }
    public string? AccessCode { get; set; }
    public string? ParticipantEmail { get; set; }
    public string? ParticipantName { get; set; }
    public List<AnswerDto> Answers { get; set; } = new();
}

internal class SubmitPollAnswersCommandHandler : ICommandHandler<SubmitPollAnswersCommand>
{
    private readonly ICrudService<PollAnalytics> _analyticsService;
    private readonly ICrudService<Domain.Entities.Poll> _pollService;
    private readonly ICrudService<PollSubmission> _submissionService;

    public SubmitPollAnswersCommandHandler(
        ICrudService<Domain.Entities.Poll> pollService,
        ICrudService<PollSubmission> submissionService,
        ICrudService<PollAnalytics> analyticsService
    )
    {
        _pollService = pollService;
        _submissionService = submissionService;
        _analyticsService = analyticsService;
    }

    public async Task HandleAsync(SubmitPollAnswersCommand request, CancellationToken cancellationToken)
    {
        var validationErrors = new List<string>();

        // Validate poll exists and is accessible
        var poll = await _pollService.GetQueryableSet()
            .Include(p => p.Questions)
            .ThenInclude(q => q.Choices)
            .FirstOrDefaultAsync(p => p.Id == request.PollId, cancellationToken);

        if (poll == null) throw new NotFoundException("Poll not found");

        // Validate poll access and status
        await ValidatePollAccess(poll, request.AccessCode, validationErrors);
        if (validationErrors.Any())
            throw new ValidationException(string.Join("; ", validationErrors));

        // Validate poll timing and status
        ValidatePollTiming(poll, validationErrors);
        if (validationErrors.Any())
            throw new ValidationException(string.Join("; ", validationErrors));

        // Check for duplicate submissions if not allowed
        if (!poll.IsMultipleVotesAllowed && !string.IsNullOrEmpty(request.ParticipantEmail))
        {
            var existingSubmission = await _submissionService.GetQueryableSet()
                .FirstOrDefaultAsync(s => s.PollId == request.PollId &&
                                          s.ParticipantEmail == request.ParticipantEmail, cancellationToken);

            if (existingSubmission != null) validationErrors.Add("You have already submitted answers for this poll");
        }

        // Check voting cooldown
        if (poll.VotingCooldownMinutes > 0 && !string.IsNullOrEmpty(request.ParticipantEmail))
        {
            var lastSubmission = await _submissionService.GetQueryableSet()
                .Where(s => s.PollId == request.PollId && s.ParticipantEmail == request.ParticipantEmail)
                .OrderByDescending(s => s.SubmittedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (lastSubmission != null)
            {
                var timeSinceLastSubmission = DateTimeOffset.UtcNow - lastSubmission.SubmittedAt;
                if (timeSinceLastSubmission.TotalMinutes < poll.VotingCooldownMinutes)
                {
                    var remainingMinutes = poll.VotingCooldownMinutes - (int)timeSinceLastSubmission.TotalMinutes;
                    validationErrors.Add($"Please wait {remainingMinutes} more minutes before submitting again");
                }
            }
        }

        // Validate answers
        ValidateAnswers(poll, request.Answers, validationErrors);

        // Throw validation exception if there are any errors
        if (validationErrors.Any()) throw new ValidationException(string.Join("; ", validationErrors));

        // Create submission
        var submission = new PollSubmission
        {
            Id = Guid.NewGuid(),
            PollId = request.PollId,
            ParticipantEmail = request.ParticipantEmail,
            ParticipantName = request.ParticipantName,
            SubmittedAt = DateTimeOffset.UtcNow,
            Answers = request.Answers.Select(a => new PollAnswer
            {
                Id = Guid.NewGuid(),
                QuestionId = a.QuestionId,
                QuestionType = a.QuestionType.ToString(),
                TextAnswer = a.TextAnswer,
                SingleChoiceId = a.SingleChoiceId,
                AnsweredAt = DateTimeOffset.UtcNow,
                SelectedChoices = a.SelectedChoiceIds?.Select((choiceId, index) => new PollAnswerChoice
                {
                    Id = Guid.NewGuid(),
                    ChoiceId = choiceId,
                    RankOrder = index + 1, // For ranking questions
                    CreatedAt = DateTimeOffset.UtcNow
                }).ToList() ?? new List<PollAnswerChoice>()
            }).ToList()
        };

        await _submissionService.AddAsync(submission, cancellationToken);
        await _pollService.UpdateAsync(poll, cancellationToken);

        // Update poll analytics
        await UpdatePollAnalytics(request.PollId, cancellationToken);
    }

    private async Task ValidatePollAccess(Domain.Entities.Poll poll, string? accessCode, List<string> validationErrors)
    {
        if (!poll.IsPublic && string.IsNullOrEmpty(accessCode))
        {
            validationErrors.Add("Access code is required for this poll");
            return;
        }

        if (!poll.IsPublic && !string.IsNullOrEmpty(poll.AccessCode) && poll.AccessCode != accessCode)
            validationErrors.Add("Invalid access code");
    }

    private void ValidatePollTiming(Domain.Entities.Poll poll, List<string> validationErrors)
    {
        var now = DateTime.UtcNow;

        if (!poll.IsActive) validationErrors.Add("This poll is not active");

        if (poll.StartTime > now) validationErrors.Add("This poll has not started yet");

        if (poll.EndTime.HasValue && poll.EndTime < now) validationErrors.Add("This poll has ended");
    }

    private void ValidateAnswers(Domain.Entities.Poll poll, List<AnswerDto> answers, List<string> validationErrors)
    {
        var questions = poll.Questions?.Where(q => q.IsActive).ToList() ?? new List<Domain.Entities.Question>();
        var requiredQuestions = questions.Where(q => q.IsRequired).ToList();

        // Check if all required questions are answered
        foreach (var requiredQuestion in requiredQuestions)
        {
            var answer = answers.FirstOrDefault(a => a.QuestionId == requiredQuestion.Id);
            if (answer == null || IsAnswerEmpty(answer))
                validationErrors.Add($"Question '{requiredQuestion.QuestionText}' is required");
        }

        // Validate each answer
        foreach (var answer in answers)
        {
            var question = questions.FirstOrDefault(q => q.Id == answer.QuestionId);
            if (question == null)
            {
                validationErrors.Add($"Question with ID {answer.QuestionId} not found");
                continue;
            }

            ValidateAnswerForQuestion(question, answer, validationErrors);
        }
    }

    private bool IsAnswerEmpty(AnswerDto answer)
    {
        return string.IsNullOrEmpty(answer.TextAnswer) &&
               answer.SingleChoiceId == null &&
               (answer.SelectedChoiceIds == null || !answer.SelectedChoiceIds.Any());
    }

    private void ValidateAnswerForQuestion(Domain.Entities.Question question, AnswerDto answer,
        List<string> validationErrors)
    {
        var choices = question.Choices?.Where(c => c.IsActive == true).ToList() ?? new List<Domain.Entities.Choice>();

        switch (question.QuestionType)
        {
            case QuestionType.SingleChoice:
                ValidateSingleChoiceAnswer(question, answer, choices, validationErrors);
                break;

            case QuestionType.MultipleChoice:
                ValidateMultipleChoiceAnswer(question, answer, choices, validationErrors);
                break;

            case QuestionType.TextInput:
                ValidateTextInputAnswer(question, answer, validationErrors);
                break;

            case QuestionType.Rating:
                ValidateRatingAnswer(question, answer, choices, validationErrors);
                break;

            case QuestionType.YesNo:
                ValidateYesNoAnswer(question, answer, validationErrors);
                break;

            case QuestionType.Ranking:
                ValidateRankingAnswer(question, answer, choices, validationErrors);
                break;

            default:
                validationErrors.Add($"Unknown question type for question '{question.QuestionText}'");
                break;
        }
    }

    private void ValidateSingleChoiceAnswer(Domain.Entities.Question question, AnswerDto answer,
        List<Domain.Entities.Choice> choices, List<string> validationErrors)
    {
        // For SingleChoice, check both Answer field and SingleChoiceId
        if (answer.SingleChoiceId == null && string.IsNullOrEmpty(answer.Answer?.ToString()))
        {
            validationErrors.Add($"Please select an option for question '{question.QuestionText}'");
            return;
        }

        // If Answer is provided as string (choice ID), convert to Guid
        if (answer.SingleChoiceId == null && !string.IsNullOrEmpty(answer.Answer?.ToString()))
        {
            if (Guid.TryParse(answer.Answer.ToString(), out var choiceId))
            {
                answer.SingleChoiceId = choiceId;
            }
            else
            {
                validationErrors.Add($"Invalid choice format for question '{question.QuestionText}'");
                return;
            }
        }

        if (!choices.Any(c => c.Id == answer.SingleChoiceId))
            validationErrors.Add($"Invalid choice selected for question '{question.QuestionText}'");
    }

    private void ValidateMultipleChoiceAnswer(Domain.Entities.Question question, AnswerDto answer,
        List<Domain.Entities.Choice> choices, List<string> validationErrors)
    {
        if (answer.SelectedChoiceIds == null || !answer.SelectedChoiceIds.Any())
        {
            validationErrors.Add($"Please select at least one option for question '{question.QuestionText}'");
            return;
        }

        if (answer.SelectedChoiceIds.Any(id => !choices.Any(c => c.Id == id)))
            validationErrors.Add($"Invalid choice(s) selected for question '{question.QuestionText}'");
    }

    private void ValidateTextInputAnswer(Domain.Entities.Question question, AnswerDto answer,
        List<string> validationErrors)
    {
        // For TextInput, check both TextAnswer and Answer fields
        var textValue = answer.TextAnswer ?? answer.Answer?.ToString();

        if (string.IsNullOrEmpty(textValue))
            validationErrors.Add($"Please provide an answer for question '{question.QuestionText}'");
    }

    private void ValidateRatingAnswer(Domain.Entities.Question question, AnswerDto answer,
        List<Domain.Entities.Choice> choices, List<string> validationErrors)
    {
        if (answer.SingleChoiceId == null && string.IsNullOrEmpty(answer.Answer?.ToString()))
        {
            validationErrors.Add($"Please select a rating for question '{question.QuestionText}'");
            return;
        }

        // If Answer is provided as string (choice ID), convert to Guid
        if (answer.SingleChoiceId == null && !string.IsNullOrEmpty(answer.Answer?.ToString()))
        {
            if (Guid.TryParse(answer.Answer.ToString(), out var choiceId))
            {
                answer.SingleChoiceId = choiceId;
            }
            else
            {
                validationErrors.Add($"Invalid rating format for question '{question.QuestionText}'");
                return;
            }
        }

        if (!choices.Any(c => c.Id == answer.SingleChoiceId))
            validationErrors.Add($"Invalid rating selected for question '{question.QuestionText}'");
    }

    private void ValidateYesNoAnswer(Domain.Entities.Question question, AnswerDto answer, List<string> validationErrors)
    {
        // For YesNo, check Answer field for "yes"/"no" values
        var answerValue = answer.Answer?.ToString()?.ToLower();

        if (string.IsNullOrEmpty(answerValue) || (answerValue != "yes" && answerValue != "no"))
            validationErrors.Add($"Please select Yes or No for question '{question.QuestionText}'");
    }

    private void ValidateRankingAnswer(Domain.Entities.Question question, AnswerDto answer,
        List<Domain.Entities.Choice> choices, List<string> validationErrors)
    {
        if (answer.SelectedChoiceIds == null || !answer.SelectedChoiceIds.Any())
        {
            validationErrors.Add($"Please rank the options for question '{question.QuestionText}'");
            return;
        }

        if (answer.SelectedChoiceIds.Any(id => !choices.Any(c => c.Id == id)))
        {
            validationErrors.Add($"Invalid ranking choices selected for question '{question.QuestionText}'");
            return;
        }

        // Validate that all choices are ranked (if required)
        if (answer.SelectedChoiceIds.Count != choices.Count)
        {
            validationErrors.Add($"Please rank all options for question '{question.QuestionText}'");
            return;
        }

        // Validate no duplicate rankings
        if (answer.SelectedChoiceIds.Count != answer.SelectedChoiceIds.Distinct().Count())
            validationErrors.Add($"Each option can only be ranked once for question '{question.QuestionText}'");
    }

    private async Task UpdatePollAnalytics(Guid pollId, CancellationToken cancellationToken)
    {
        var analytics = await _analyticsService.GetQueryableSet()
            .FirstOrDefaultAsync(a => a.PollId == pollId, cancellationToken);

        if (analytics == null)
        {
            // Create new analytics record
            analytics = new PollAnalytics(
                pollId,
                0,
                1,
                null,
                null,
                string.Empty,
                "System"
            );

            await _analyticsService.AddAsync(analytics, cancellationToken);
        }
        else
        {
            // Update existing analytics
            var newTotalVotes = (analytics.TotalVotes ?? 0) + 1;
            analytics.UpdateAnalytics(
                analytics.TotalViews,
                newTotalVotes,
                analytics.AverageCompletionTime,
                analytics.CompletionRate,
                analytics.DemographicsData ?? string.Empty,
                "System"
            );

            await _analyticsService.UpdateAsync(analytics, cancellationToken);
        }
    }
}