namespace Flashcards.Models;

public abstract class BaseCard
{
    public int Id { get; set; }
    public int StackId { get; set; }
    public CardType CardType { get; set; }
}

public class Flashcard : BaseCard
{
    public string? Front { get; set; }
    public string? Back { get; set; }
    public Flashcard() => CardType = CardType.Flashcard;
}

public class ClozeCard : BaseCard
{
    public string? ClozeText { get; set; }
    public ClozeCard() => CardType = CardType.Cloze;
}

public class FillInCard : BaseCard
{
    public string? FillInText { get; set; }
    public List<string>? Answer { get; set; }
    public FillInCard() => CardType = CardType.FillIn;
}

public class MultipleChoiceCard : BaseCard
{
    public string? Question { get; set; }
    public List<string>? Choices { get; set; }
    public string? Answer { get; set; }
    public MultipleChoiceCard() => CardType = CardType.MultipleChoice;
}

public abstract record class BaseCardDTO
{
    public int Id { get; init; }
    public CardType CardType { get; init; }
}

public record class FlashcardDTO : BaseCardDTO
{
    public string Front { get; init; } = string.Empty;
    public string Back { get; init; } = string.Empty;
}

public record class ClozeCardDTO : BaseCardDTO
{
    public string ClozeText { get; init; } = string.Empty;
}

public record class FillInCardDTO : BaseCardDTO
{
    public string FillInText { get; init; } = string.Empty;
    public List<string> Answer { get; init; } = new();
}

public record class MultipleChoiceCardDTO : BaseCardDTO
{
    public string Question { get; init; } = string.Empty;
    public List<string> Choices { get; init; } = new();
    public string Answer { get; init; } = string.Empty;
}

public enum CardType
{
    Flashcard,
    Cloze,
    FillIn,
    MultipleChoice
}
