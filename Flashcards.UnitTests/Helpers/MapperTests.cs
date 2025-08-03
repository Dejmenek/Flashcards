using Flashcards.Helpers;
using Flashcards.Models;

namespace Flashcards.UnitTests.Helpers;
public class MapperTests
{
    [Fact]
    public void ToFlashcardDTO_ShouldMapPropertiesCorrectly()
    {
        // Arrange
        var flashcard = new Flashcard
        {
            Id = 1,
            StackId = 2,
            Front = "Front Text",
            Back = "Back Text"
        };

        // Act
        var result = Mapper.ToFlashcardDTO(flashcard);

        // Assert
        Assert.Equal(flashcard.Id, result.Id);
        Assert.Equal(flashcard.Front, result.Front);
        Assert.Equal(flashcard.Back, result.Back);
        Assert.Equal(flashcard.CardType, result.CardType);
        Assert.Null(typeof(FlashcardDTO).GetProperty("StackId"));
    }

    [Fact]
    public void ToStackDTO_ShouldMapPropertiesCorrectly()
    {
        // Arrange
        var stack = new Stack
        {
            Id = 1,
            Name = "Test Stack"
        };

        // Act
        var result = Mapper.ToStackDTO(stack);

        // Assert
        Assert.Equal(stack.Name, result.Name);
        Assert.Null(typeof(StackDTO).GetProperty("Id"));
    }

    [Fact]
    public void ToStudySessionDTO_ShouldMapPropertiesCorrectly()
    {
        // Arrange
        var studySession = new StudySession
        {
            Id = 1,
            StackId = 2,
            Date = DateTime.Now,
            Score = 85
        };

        // Act
        var result = Mapper.ToStudySessionDTO(studySession);

        // Assert
        Assert.Equal(studySession.Id, result.Id);
        Assert.Equal(studySession.Date, result.Date);
        Assert.Equal(studySession.Score, result.Score);
        Assert.Null(typeof(StudySessionDTO).GetProperty("StackId"));
    }
}
