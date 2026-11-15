using System;
using System.Threading.Tasks;
using Npgsql;
using Xunit;
using Xunit.Abstractions;

namespace EnterpriseCRM.IntegrationTests;

public class DatabaseConnectionTests
{
    private readonly ITestOutputHelper _output;

    public DatabaseConnectionTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task DiscoverRegionAndConnect()
    {
        var regions = new[]
        {
            "ap-southeast-1",
            "ap-northeast-1",
            "ap-northeast-2",
            "ap-south-1",
            "ap-southeast-2",
            "us-east-1",
            "us-east-2",
            "us-west-1",
            "us-west-2",
            "eu-central-1",
            "eu-west-1",
            "eu-west-2",
            "eu-west-3",
            "sa-east-1",
            "ca-central-1"
        };

        string? successfulRegion = null;
        string? versionResult = null;

        foreach (var r in regions)
        {
            var host = $"aws-0-{r}.pooler.supabase.com";
            var connStr = $"Host={host};Port=6543;Database=postgres;Username=postgres.ehmjgxwrmzlccnivqqxb;Password=enterprisecrm2026;SSL Mode=Require;Trust Server Certificate=true;Timeout=5;";

            try
            {
                await using var conn = new NpgsqlConnection(connStr);
                await conn.OpenAsync();

                await using var cmd = new NpgsqlCommand("SELECT version();", conn);
                var version = await cmd.ExecuteScalarAsync();

                successfulRegion = r;
                versionResult = version?.ToString();
                _output.WriteLine($"SUCCESS: Region is {r}! PostgreSQL: {versionResult}");
                break;
            }
            catch (PostgresException ex) when (ex.Message.Contains("tenant/user"))
            {
                _output.WriteLine($"Region {r}: tenant not in this region.");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Region {r}: {ex.Message}");
            }
        }

        Assert.NotNull(successfulRegion);
        Assert.NotNull(versionResult);
    }
}
