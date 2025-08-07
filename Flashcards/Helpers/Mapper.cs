using Flashcards.Models;

namespace Flashcards.Helpers;

public static class Mapper
{
    public static BaseCardDTO ToCardDTO(BaseCard card)
    {
        return card switch
        {
            Flashcard flashcard => ToFlashcardDTO(flashcard),
            ClozeCard clozeCard => ToClozeCardDTO(clozeCard),
            FillInCard fillInCard => ToFillInCardDTO(fillInCard),
            MultipleChoiceCard multipleChoiceCard => ToMultipleChoiceCardDTO(multipleChoiceCard),
            _ => throw new ArgumentException($"Unknown card type: {card.GetType().Name}", nameof(card))
        };
    }

    public static FlashcardDTO ToFlashcardDTO(Flashcard flashcard)
    {
        return new FlashcardDTO
        {
            Id = flashcard.Id,
            CardType = flashcard.CardType,
            Front = flashcard.Front ?? string.Empty,
            Back = flashcard.Back ?? string.Empty,
        };
    }

    public static ClozeCardDTO ToClozeCardDTO(ClozeCard clozeCard)
    {
        return new ClozeCardDTO
        {
            Id = clozeCard.Id,
            CardType = clozeCard.CardType,
            ClozeText = clozeCard.ClozeText ?? string.Empty,
        };
    }

    public static FillInCardDTO ToFillInCardDTO(FillInCard fillInCard)
    {
        return new FillInCardDTO
        {
            Id = fillInCard.Id,
            CardType = fillInCard.CardType,
            FillInText = fillInCard.FillInText ?? string.Empty,
            Answer = fillInCard.Answer != null ? fillInCard.Answer.Split(';').ToList() : new()
        };
    }

    public static MultipleChoiceCardDTO ToMultipleChoiceCardDTO(MultipleChoiceCard multipleChoiceCard)
    {
        return new MultipleChoiceCardDTO
        {
            Id = multipleChoiceCard.Id,
            CardType = multipleChoiceCard.CardType,
            Question = multipleChoiceCard.Question ?? string.Empty,
            Choices = multipleChoiceCard.Choices != null ? multipleChoiceCard.Choices.Split(';').ToList() : new(),
            Answer = multipleChoiceCard.Answer != null ? multipleChoiceCard.Answer.Split(';').ToList() : new(),
        };
    }

    public static StackDTO ToStackDTO(Stack stack)
    {
        return new StackDTO
        {
            Name = stack.Name,
        };
    }

    public static StudySessionDTO ToStudySessionDTO(StudySession studysession)
    {
        return new StudySessionDTO
        {
            Id = studysession.Id,
            Date = studysession.Date,
            Score = studysession.Score,
        };
    }
}
