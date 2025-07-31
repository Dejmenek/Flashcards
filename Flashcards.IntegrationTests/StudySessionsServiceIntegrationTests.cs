using Flashcards.DataAccess.Interfaces;
using Flashcards.Helpers;
using Flashcards.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Flashcards.IntegrationTests;
public class StudySessionsServiceIntegrationTests : BaseIntegrationTest, IAsyncLifetime
{
    private readonly IStudySessionsService _studySessionsService;
    private readonly IStudySessionsRepository _studySessionsRepository;
    private readonly IUserInteractionService _userInteractionService;
    private readonly IStacksRepository _stacksRepository;

    public StudySessionsServiceIntegrationTests(TestClassFixture fixture) : base(fixture)
    {
        _studySessionsRepository = _scope.ServiceProvider.GetRequiredService<IStudySessionsRepository>();
        _studySessionsService = _scope.ServiceProvider.GetRequiredService<IStudySessionsService>();
        _userInteractionService = _scope.ServiceProvider.GetRequiredService<IUserInteractionService>();
        _stacksRepository = _scope.ServiceProvider.GetRequiredService<IStacksRepository>();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    public async Task InitializeAsync() => await InitializeDatabaseAsync();

    [Fact]
    public async Task RunStudySessionAsync_WithValidFlashcards_AddsStudySessionAndReturnsSuccess()
    {
        // Arrange
        var stackId = 3;
        var flashcards = (await _stacksRepository.GetFlashcardsByStackIdAsync(stackId)).Value;
        var flashcardDTOs = flashcards.Select(Mapper.ToFlashcardDTO).ToList();

        _userInteractionService.GetAnswer().Returns("Good morning", "Goodbye", "Please");

        // Act
        var result = await _studySessionsService.RunStudySessionAsync(flashcardDTOs, stackId);

        // Assert
        Assert.True(result.IsSuccess);

        var studySessionsResult = await _studySessionsRepository.GetAllStudySessionsAsync();
        Assert.True(studySessionsResult.IsSuccess);
        Assert.NotEmpty(studySessionsResult.Value);
        Assert.NotNull(studySessionsResult.Value);
        Assert.Equal(stackId, studySessionsResult.Value.First().StackId);
    }
}
