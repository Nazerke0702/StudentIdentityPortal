using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace StudentIdentityPortal.Tests;

public sealed class PortalFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _database;

    public PortalFactory()
    {
        var name = $"IdentityTests-{Guid.NewGuid():N}";
        _database = new SqliteConnection($"Data Source={name};Mode=Memory;Cache=Shared");
        _database.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _database.ConnectionString,
                ["Admin:Email"] = "must-not-seed@example.test",
                ["Admin:Password"] = $"Aa1!{Guid.NewGuid():N}"
            }));
        builder.ConfigureLogging(logging => logging.ClearProviders());
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) _database.Dispose();
    }
}
