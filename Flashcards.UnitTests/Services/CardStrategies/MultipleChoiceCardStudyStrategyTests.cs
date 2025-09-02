using Flashcards.Services.CardStrategies;

namespace Flashcards.UnitTests.Services.CardStrategies;
public class MultipleChoiceCardStudyStrategyTests
{
    [Theory]
    [InlineData(new string[] { "A", "B" }, new string[] { "A", "B" }, true)]
    [InlineData(new string[] { "B", "A" }, new string[] { "A", "B" }, true)]
    [InlineData(new string[] { "A" }, new string[] { "A", "B" }, false)]
    [InlineData(new string[] { "A", "B", "C" }, new string[] { "A", "B" }, false)]
    [InlineData(new string[] { "A", "C" }, new string[] { "A", "B" }, false)]
    [InlineData(new string[] { }, new string[] { }, true)]
    [InlineData(new string[] { "A" }, new string[] { }, false)]
    public void IsCorrectMultipleChoiceCardAnswer_ShouldReturnCorrectResult_WhenComparingAnswers(string[] userAnswers, string[] correctAnswers, bool expected)
    {
        // Arrange
        var userAnswersList = userAnswers.ToList();
        var correctAnswersList = correctAnswers.ToList();

        // Use reflection to access the private method
        var method = typeof(MultipleChoiceCardStudyStrategy).GetMethod("IsCorrectMultipleChoiceCardAnswer",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        // Act
        var result = (bool)method!.Invoke(null, new object[] { userAnswersList, correctAnswersList });

        // Assert
        Assert.Equal(expected, result);
    }
}