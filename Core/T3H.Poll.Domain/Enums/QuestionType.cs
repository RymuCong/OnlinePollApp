namespace T3H.Poll.Domain.Enums;

public enum QuestionType
{
    SingleChoice = 1,
    MultiChoice = 2,  // Match your API's "MultiChoice" instead of "MultipleChoice" 
    ShortText = 3,    // If you're using "ShortText" in API
    LongText = 4,     // If you're using "LongText" in API
    Rating = 5,
    YesNo = 6,
    Ranking = 7
}