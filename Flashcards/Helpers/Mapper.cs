using Flashcards.Models;

namespace Flashcards.Helpers;

public static class Mapper
{
    public static BaseCardDto ToCardDTO(BaseCard card)
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

    public static FlashcardDto ToFlashcardDTO(Flashcard flashcard)
    {
        return new FlashcardDto
        {
            Id = flashcard.Id,
            CardType = flashcard.CardType,
            Front = flashcard.Front ?? string.Empty,
            Back = flashcard.Back ?? string.Empty,
            Box = flashcard.Box,
            NextReviewDate = flashcard.NextReviewDate
        };
    }

    public static ClozeCardDto ToClozeCardDTO(ClozeCard clozeCard)
    {
        return new ClozeCardDto
        {
            Id = clozeCard.Id,
            CardType = clozeCard.CardType,
            ClozeText = clozeCard.ClozeText ?? string.Empty,
            Box = clozeCard.Box,
            NextReviewDate = clozeCard.NextReviewDate
        };
    }

    public static FillInCardDto ToFillInCardDTO(FillInCard fillInCard)
    {
        return new FillInCardDto
        {
            Id = fillInCard.Id,
            CardType = fillInCard.CardType,
            FillInText = fillInCard.FillInText ?? string.Empty,
            Answer = fillInCard.Answer != null ? fillInCard.Answer.Split(';').ToList() : new(),
            Box = fillInCard.Box,
            NextReviewDate = fillInCard.NextReviewDate
        };
    }

    public static MultipleChoiceCardDto ToMultipleChoiceCardDTO(MultipleChoiceCard multipleChoiceCard)
    {
        return new MultipleChoiceCardDto
        {
            Id = multipleChoiceCard.Id,
            CardType = multipleChoiceCard.CardType,
            Question = multipleChoiceCard.Question ?? string.Empty,
            Choices = multipleChoiceCard.Choices != null ? multipleChoiceCard.Choices.Split(';').ToList() : new(),
            Answer = multipleChoiceCard.Answer != null ? multipleChoiceCard.Answer.Split(';').ToList() : new(),
            Box = multipleChoiceCard.Box,
            NextReviewDate = multipleChoiceCard.NextReviewDate
        };
    }

    public static StackDto ToStackDTO(Stack stack)
    {
        return new StackDto
        {
            Name = stack.Name,
        };
    }

    public static List<BaseCardDto> ToCardDTOList(IEnumerable<BaseCard> cards)
    {
        return cards.Select(c => ToCardDTO(c)).ToList();
    }

    public static List<StudySessionDto> ToStudySessionDTOList(IEnumerable<StudySession> studySessions)
    {
        return studySessions.Select(s => ToStudySessionDTO(s)).ToList();
    }

    public static StudySessionDto ToStudySessionDTO(StudySession studysession)
    {
        return new StudySessionDto
        {
            Id = studysession.Id,
            Date = studysession.Date,
            Score = studysession.Score,
        };
    }
}