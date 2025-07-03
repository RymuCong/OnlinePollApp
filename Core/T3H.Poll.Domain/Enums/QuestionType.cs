namespace T3H.Poll.Domain.Enums;

public enum QuestionType
{
    SingleChoice = 1,
    MultipleChoice = 2, 
    ShortText = 3,    // If you're using "ShortText" in API
    LongText = 4,     // If you're using "LongText" in API
    Rating = 5,
    YesNo = 6,
    Ranking = 7
}