using EnterpriseCRM.Domain.Constants;
using EnterpriseCRM.Domain.Entities;
using EnterpriseCRM.Domain.Enums;
using EnterpriseCRM.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace EnterpriseCRM.UnitTests;

public class SecurityUnitTests
{
    private readonly PasswordHasher _passwordHasher = new();
    private readonly JwtTokenGenerator _jwtTokenGenerator;

    public SecurityUnitTests()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"Jwt:Secret", "Super_Secret_Key_For_Unit_Testing_Enterprise_CRM_2026_At_Least_32_Chars!"},
            {"Jwt:Issuer", "EnterpriseCRM.TestIssuer"},
            {"Jwt:Audience", "EnterpriseCRM.TestAudience"},
            {"Jwt:AccessTokenExpirationMinutes", "45"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _jwtTokenGenerator = new JwtTokenGenerator(configuration);
    }

    [Fact]
    public void HashPassword_ShouldGenerateUniqueSaltAndHash()
    {
        // Arrange
        const string password = "TestPassword@2026";

        // Act
        var (hash1, salt1) = _passwordHasher.HashPassword(password);
        var (hash2, salt2) = _passwordHasher.HashPassword(password);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(hash1));
        Assert.False(string.IsNullOrWhiteSpace(salt1));
        Assert.NotEqual(salt1, salt2); // Salts must be uniquely generated per hash
        Assert.NotEqual(hash1, hash2); // Hashes must differ due to unique salt
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        const string password = "SecureSecretPassword#123";
        var (hash, salt) = _passwordHasher.HashPassword(password);

        // Act
        var isValid = _passwordHasher.VerifyPassword(password, hash, salt);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        const string password = "CorrectPassword@2026";
        const string wrongPassword = "WrongPassword@2026";
        var (hash, salt) = _passwordHasher.HashPassword(password);

        // Act
        var isValid = _passwordHasher.VerifyPassword(wrongPassword, hash, salt);

        // Assert
        Assert.False(isValid);
    }

    [Theory]
    [InlineData("", "hash", "salt")]
    [InlineData("pwd", "", "salt")]
    [InlineData("pwd", "hash", "")]
    [InlineData(null, "hash", "salt")]
    public void VerifyPassword_WithNullOrEmpty_ShouldReturnFalse(string? pwd, string? hash, string? salt)
    {
        // Act
        var isValid = _passwordHasher.VerifyPassword(pwd!, hash!, salt!);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void GenerateAccessToken_ShouldIncludeAllExpectedClaims()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "alexander.wright@enterprisecrm.local",
            FirstName = "Alexander",
            LastName = "Wright"
        };

        var roles = new[] { SystemRoles.Admin, SystemRoles.Manager };
        var permissions = new[] { Permissions.LeadsView, Permissions.DealsCreate, Permissions.InvoicesApprove };
        var deptId = Guid.NewGuid();
        var teamId = Guid.NewGuid();

        // Act
        var tokenString = _jwtTokenGenerator.GenerateAccessToken(
            user,
            roles,
            permissions,
            deptId,
            teamId,
            DataScope.Department);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(tokenString));

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(tokenString);

        Assert.Equal("EnterpriseCRM.TestIssuer", jwt.Issuer);
        Assert.Contains("EnterpriseCRM.TestAudience", jwt.Audiences);

        // Check Sub (UserId)
        var subClaim = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
        Assert.Equal(user.Id.ToString(), subClaim);

        // Check Email
        var emailClaim = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value;
        Assert.Equal(user.Email, emailClaim);

        // Check Department and Team claims
        Assert.Equal(deptId.ToString(), jwt.Claims.FirstOrDefault(c => c.Type == "department_id")?.Value);
        Assert.Equal(teamId.ToString(), jwt.Claims.FirstOrDefault(c => c.Type == "team_id")?.Value);
        Assert.Equal(DataScope.Department.ToString(), jwt.Claims.FirstOrDefault(c => c.Type == "data_scope")?.Value);

        // Check Roles
        var roleClaims = jwt.Claims.Where(c => c.Type == ClaimTypes.Role || c.Type == "role").Select(c => c.Value).Distinct().ToList();
        Assert.Contains(SystemRoles.Admin, roleClaims);
        Assert.Contains(SystemRoles.Manager, roleClaims);

        // Check Permissions
        var permClaims = jwt.Claims.Where(c => c.Type == "permission").Select(c => c.Value).ToList();
        Assert.Contains(Permissions.LeadsView, permClaims);
        Assert.Contains(Permissions.DealsCreate, permClaims);
        Assert.Contains(Permissions.InvoicesApprove, permClaims);
    }

    [Fact]
    public void GenerateRefreshToken_ShouldProduceHighEntropyUniqueTokens()
    {
        // Act
        var token1 = _jwtTokenGenerator.GenerateRefreshToken();
        var token2 = _jwtTokenGenerator.GenerateRefreshToken();

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(token1));
        Assert.False(string.IsNullOrWhiteSpace(token2));
        Assert.NotEqual(token1, token2);
        Assert.True(token1.Length >= 64);
    }
}
