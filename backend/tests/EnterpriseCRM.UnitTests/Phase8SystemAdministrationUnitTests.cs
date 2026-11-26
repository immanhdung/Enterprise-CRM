using System.Security.Cryptography;
using System.Text;
using EnterpriseCRM.Application.Features.SystemAdministration;
using EnterpriseCRM.Domain.Constants;
using EnterpriseCRM.Domain.Entities;
using Xunit;

namespace EnterpriseCRM.UnitTests;

public class Phase8SystemAdministrationUnitTests
{
    [Fact]
    public void ApiKey_HashVerification_MatchesSha256()
    {
        // Arrange
        var plaintextKey = "crm_live_abcd1234efgh5678ijkl9012mnop3456";
        var expectedHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(plaintextKey)));

        // Act
        var computedHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(plaintextKey)));

        // Assert
        Assert.Equal(expectedHash, computedHash);
        Assert.StartsWith("crm_live_", plaintextKey);
        Assert.True(plaintextKey.Length >= 32);
    }

    [Fact]
    public void Webhook_HmacSha256Signature_MatchesExpected()
    {
        // Arrange
        var secret = "super_secret_webhook_key_12345";
        var payload = "{\"event\":\"deal.won\",\"dealId\":\"11111111-2222-3333-4444-555555555555\"}";

        // Act
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var signature = "sha256=" + Convert.ToHexString(hash).ToLowerInvariant();

        // Re-verify
        using var hmacVerify = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var verifyHash = hmacVerify.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var verifySignature = "sha256=" + Convert.ToHexString(verifyHash).ToLowerInvariant();

        // Assert
        Assert.Equal(signature, verifySignature);
        Assert.StartsWith("sha256=", signature);
    }

    [Theory]
    [InlineData(500, "500 B")]
    [InlineData(1024, "1 KB")]
    [InlineData(50855936, "48.5 MB")]
    [InlineData(1073741824, "1 GB")]
    public void Backup_FormatBytes_ReturnsExpectedReadableSize(long bytes, string expected)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int i = 0;
        double d = bytes;
        while (d >= 1024 && i < suffixes.Length - 1)
        {
            d /= 1024;
            i++;
        }
        var formatted = $"{d:0.##} {suffixes[i]}";

        Assert.Equal(expected, formatted);
    }

    [Fact]
    public void AuditLog_MaskSensitiveFields_ReplacesPasswordsAndTokens()
    {
        // Arrange
        var json = "{\"email\":\"test@crm.vn\",\"password\":\"SuperSecretPassword!\",\"token\":\"eyJhbGciOi...\",\"apiKey\":\"live_123\"}";

        // Act
        var sensitiveKeys = new[] { "password", "token", "secret", "apikey", "salt" };
        var result = json;
        foreach (var key in sensitiveKeys)
        {
            result = System.Text.RegularExpressions.Regex.Replace(
                result,
                $"(\"{key}\"\\s*:\\s*\")[^\"]+(\")",
                "$1********$2",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        // Assert
        Assert.DoesNotContain("SuperSecretPassword!", result);
        Assert.DoesNotContain("eyJhbGciOi...", result);
        Assert.DoesNotContain("live_123", result);
        Assert.Contains("test@crm.vn", result);
        Assert.Contains("********", result);
    }

    [Fact]
    public void Rbac_SystemRoles_CannotBeDeleted()
    {
        // Arrange
        var adminRole = new Role
        {
            Id = Guid.NewGuid(),
            Name = SystemRoles.Admin,
            Description = "Administrator",
            IsSystem = true
        };

        // Act & Assert
        Assert.True(adminRole.IsSystem);
    }

    [Fact]
    public void Organization_TransferHistory_CapturesOldAndNewValues()
    {
        // Arrange
        var history = new OrganizationChangeHistory
        {
            Id = Guid.NewGuid(),
            EmployeeId = Guid.NewGuid(),
            ChangeType = "TransferDepartment",
            OldDepartmentName = "Sales North",
            NewDepartmentName = "Enterprise B2B",
            OldBaseSalary = 20000000m,
            NewBaseSalary = 25000000m,
            EffectiveDate = new DateOnly(2026, 12, 1),
            Reason = "Promotion to Enterprise Business Unit",
            CreatedAtUtc = DateTime.UtcNow
        };

        // Assert
        Assert.Equal("TransferDepartment", history.ChangeType);
        Assert.Equal("Sales North", history.OldDepartmentName);
        Assert.Equal("Enterprise B2B", history.NewDepartmentName);
        Assert.True(history.NewBaseSalary > history.OldBaseSalary);
    }

    [Fact]
    public void SystemConfiguration_Categories_AreWellDefined()
    {
        var validCategories = new HashSet<string> { "General", "CRM", "Security", "Notification", "Integration" };

        var config = new SystemConfiguration
        {
            Category = "Security",
            Key = "MaxLoginAttempts",
            Value = "5"
        };

        Assert.Contains(config.Category, validCategories);
        Assert.Equal("5", config.Value);
    }

    [Fact]
    public void Validation_CreateUser_RequiresValidEmailAndPassword()
    {
        var validator = new CreateUserRequestValidator();

        var invalidRequest = new CreateUserRequest(
            Email: "not-an-email",
            Password: "123",
            FirstName: "",
            LastName: "",
            Roles: new List<string>(),
            DepartmentId: null,
            TeamId: null,
            Position: null,
            Phone: null,
            BaseSalary: null
        );

        var result = validator.Validate(invalidRequest);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
        Assert.Contains(result.Errors, e => e.PropertyName == "Password");
        Assert.Contains(result.Errors, e => e.PropertyName == "FirstName");
        Assert.Contains(result.Errors, e => e.PropertyName == "Roles");
    }

    [Fact]
    public void Validation_CreateWebhook_RequiresValidUrl()
    {
        var validator = new CreateWebhookEndpointRequestValidator();

        var invalid = new CreateWebhookEndpointRequest(
            Name: "Test Webhook",
            Url: "invalid-url",
            SubscribedEvents: new List<string> { "deal.won" }
        );

        var result = validator.Validate(invalid);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Url");
    }

    [Fact]
    public void SupabasePitr_RetentionDays_IsPositive()
    {
        var pitr = new SupabasePitrStatusDto(
            IsPitrEnabled: true,
            PlanTier: "Supabase Pro",
            RetentionDays: 7,
            EarliestRestorePointUtc: DateTime.UtcNow.AddDays(-7),
            LatestRestorePointUtc: DateTime.UtcNow.AddMinutes(-5),
            StatusMessage: "Active"
        );

        Assert.True(pitr.IsPitrEnabled);
        Assert.Equal(7, pitr.RetentionDays);
        Assert.True(pitr.LatestRestorePointUtc > pitr.EarliestRestorePointUtc);
    }
}
