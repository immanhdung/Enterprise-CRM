using EnterpriseCRM.Application.Common.Models;
using Xunit;

namespace EnterpriseCRM.UnitTests;

public class HealthModelTests
{
    [Fact]
    public void ApiResponse_Succeeded_ShouldReturnSuccessStatus()
    {
        // Arrange
        var healthDto = new SystemHealthDto
        {
            Status = "Healthy",
            Environment = "Development",
            Version = "1.0.0"
        };

        // Act
        var response = ApiResponse<SystemHealthDto>.Succeeded(healthDto, "API is running");

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal("Healthy", response.Data.Status);
        Assert.Equal("Development", response.Data.Environment);
        Assert.Null(response.Errors);
    }
}
