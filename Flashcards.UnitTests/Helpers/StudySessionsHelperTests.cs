using Flashcards.Helpers;

namespace Flashcards.UnitTests.Helpers;
public class StudySessionsHelperTests
{
    [Theory]
    [InlineData(1, true, 2)]
    [InlineData(2, true, 3)]
    [InlineData(3, true, 3)]
    [InlineData(1, false, 1)]
    [InlineData(2, false, 1)]
    [InlineData(3, false, 1)]
    public void GetNextBox_ShouldReturnExpectedBox(int currentBox, bool isCorrect, int expectedBox)
    {
        // Act
        var nextBox = StudySessionsHelper.GetNextBox(isCorrect, currentBox);

        // Assert
        Assert.Equal(expectedBox, nextBox);
    }

    [Theory]
    [InlineData(1, true, 1)]
    [InlineData(2, true, 3)]
    [InlineData(3, true, 7)]
    [InlineData(1, false, 0)]
    [InlineData(2, false, 0)]
    [InlineData(3, false, 0)]
    public void GetNextReviewDate_ShouldReturnExpectedDate(int currentBox, bool isCorrect, int expectedDays)
    {
        // Arrange
        var before = DateTime.Now;

        // Act
        var nextReview = StudySessionsHelper.GetNextReviewDate(isCorrect, currentBox);

        // Assert
        if (isCorrect)
        {
            var delta = (nextReview.Date - before.Date).Days;
            Assert.Equal(expectedDays, delta);
        }
        else
        {
            Assert.Equal(before.Date, nextReview.Date);
        }
    }
}