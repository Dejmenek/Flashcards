using Flashcards.DataAccess;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Flashcards.IntegrationTests;
public class BaseIntegrationTest : IClassFixture<TestClassFixture>, IDisposable
{
    protected readonly IServiceScope _scope;
    protected readonly DataContext _dbContext;
    private bool _disposed = false;

    protected BaseIntegrationTest(TestClassFixture fixture)
    {
        _scope = fixture.TestHost.Services.CreateScope();
        var config = _scope.ServiceProvider.GetRequiredService<IConfiguration>();
        _dbContext = new DataContext(config);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _scope?.Dispose();
            }

            _disposed = true;
        }
    }

    protected async Task InitializeDatabaseAsync() => await _dbContext.Init();
}