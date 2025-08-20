using Flashcards.DataAccess;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Flashcards.IntegrationTests;
public class BaseIntegrationTest : IClassFixture<TestClassFixture>, IDisposable
{
    protected readonly IServiceScope _scope;
    protected readonly DataContext _dbContext;

    protected BaseIntegrationTest(TestClassFixture fixture)
    {
        _scope = fixture.TestHost.Services.CreateScope();
        var config = _scope.ServiceProvider.GetRequiredService<IConfiguration>();
        _dbContext = new DataContext(config);
    }

    public void Dispose() => _scope?.Dispose();

    protected async Task InitializeDatabaseAsync() => await _dbContext.Init();
}