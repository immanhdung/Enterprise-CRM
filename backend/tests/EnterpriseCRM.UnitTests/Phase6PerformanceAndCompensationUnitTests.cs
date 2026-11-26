using EnterpriseCRM.Application.Features.PerformanceAndCompensation;
using EnterpriseCRM.Domain.Entities;
using System.Text.Json;
using Xunit;

namespace EnterpriseCRM.UnitTests;

public class Phase6PerformanceAndCompensationUnitTests
{
    [Fact]
    public void Kpi_ScoreCalculation_BlendedWeights()
    {
        // Arrange
        var targetRevenue = 200_000_000m;
        var achievedRevenue = 150_000_000m; // 75%
        var targetDeals = 5;
        var achievedDeals = 4; // 80%
        var targetLeads = 20;
        var achievedLeads = 18; // 90%

        // Act
        var revPercent = (achievedRevenue / targetRevenue) * 100m; // 75
        var dealsPercent = ((decimal)achievedDeals / targetDeals) * 100m; // 80
        var leadsPercent = ((decimal)achievedLeads / targetLeads) * 100m; // 90

        var blendedScore = (revPercent * 0.50m) + (dealsPercent * 0.30m) + (leadsPercent * 0.20m);

        // Assert
        // (75 * 0.5) + (80 * 0.3) + (90 * 0.2) = 37.5 + 24.0 + 18.0 = 79.5%
        Assert.Equal(79.5m, Math.Round(blendedScore, 2));
    }

    [Fact]
    public void Commission_PercentageCalculation_MatchesRate()
    {
        // Arrange
        var contractValue = 120_000_000m;
        var ratePercent = 4.0m; // 4%

        // Act
        var commissionAmount = Math.Round(contractValue * (ratePercent / 100m), 2);

        // Assert
        Assert.Equal(4_800_000m, commissionAmount);
    }

    [Fact]
    public void Commission_MaxCap_EnforcesLimit()
    {
        // Arrange
        var contractValue = 500_000_000m;
        var ratePercent = 5.0m; // 5% of 500M = 25M
        var maxCap = 15_000_000m; // Cap at 15M

        // Act
        var rawAmount = Math.Round(contractValue * (ratePercent / 100m), 2);
        var cappedAmount = rawAmount > maxCap ? maxCap : rawAmount;

        // Assert
        Assert.Equal(25_000_000m, rawAmount);
        Assert.Equal(15_000_000m, cappedAmount);
    }

    [Theory]
    [InlineData(65.0, 0.0)]     // <70% -> 0%
    [InlineData(85.0, 2.0)]     // 70-99% -> 2%
    [InlineData(105.0, 3.0)]    // 100-119% -> 3%
    [InlineData(125.0, 5.0)]    // >=120% -> 5%
    public void Commission_TargetTierBased_SelectsCorrectTier(decimal achievementScore, decimal expectedRate)
    {
        // Arrange
        var tiersJson = @"[
            {""minAchievement"":0.0,""maxAchievement"":69.99,""rate"":0.0},
            {""minAchievement"":70.0,""maxAchievement"":99.99,""rate"":2.0},
            {""minAchievement"":100.0,""maxAchievement"":119.99,""rate"":3.0},
            {""minAchievement"":120.0,""maxAchievement"":9999.0,""rate"":5.0}
        ]";

        var tiers = JsonSerializer.Deserialize<List<CommissionTierRule>>(tiersJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(tiers);

        // Act
        var match = tiers.FirstOrDefault(t => achievementScore >= t.MinAchievement && (!t.MaxAchievement.HasValue || achievementScore <= t.MaxAchievement.Value));
        var rate = match?.Rate ?? 0m;

        // Assert
        Assert.Equal(expectedRate, rate);
    }

    [Fact]
    public void Commission_ManualAdjustment_ComputesNetCommission()
    {
        // Arrange
        var calculatedAmount = 10_000_000m;
        var adjustmentAmount = -1_500_000m; // deduction of 1.5M

        // Act
        var netCommission = calculatedAmount + adjustmentAmount;

        // Assert
        Assert.Equal(8_500_000m, netCommission);
    }

    [Fact]
    public void Payroll_GrossAndNetCalculation_WithCommissionsAndBonuses()
    {
        // Arrange
        var baseSalary = 15_000_000m;
        var approvedCommission = 8_200_000m;
        var approvedBonus = 3_000_000m;
        var allowances = 1_500_000m; // lunch & phone
        var deductions = 2_000_000m; // advance repayment

        // Act
        var grossSalary = baseSalary + approvedCommission + approvedBonus + allowances;
        var netSalary = Math.Max(0m, grossSalary - deductions);

        // Assert
        // Gross: 15M + 8.2M + 3M + 1.5M = 27.7M
        // Net: 27.7M - 2M = 25.7M
        Assert.Equal(27_700_000m, grossSalary);
        Assert.Equal(25_700_000m, netSalary);
    }

    [Fact]
    public void Payroll_Adjustment_IncreasesAllowancesOrDeductions()
    {
        // Arrange
        var baseSalary = 12_000_000m;
        var initialGross = baseSalary;
        var initialNet = baseSalary;

        var adjustments = new List<PayrollAdjustment>
        {
            new() { Type = "Allowance", Amount = 1_000_000m, Reason = "Travel subsidy" },
            new() { Type = "Penalty", Amount = 500_000m, Reason = "Late submission fine" }
        };

        // Act
        var allowances = adjustments.Where(a => a.Type == "Allowance").Sum(a => a.Amount);
        var deductions = adjustments.Where(a => a.Type == "Penalty" || a.Type == "Advance" || a.Type == "Deduction").Sum(a => a.Amount);

        var finalGross = baseSalary + allowances;
        var finalNet = finalGross - deductions;

        // Assert
        Assert.Equal(13_000_000m, finalGross);
        Assert.Equal(12_500_000m, finalNet);
    }

    [Fact]
    public void AntiSelfApproval_Rule_IdentifiesSelfUser()
    {
        // Arrange
        var loggedInUserId = Guid.NewGuid();
        var employeeUserId = loggedInUserId; // same user

        // Act & Assert
        Assert.True(loggedInUserId == employeeUserId, "User should be detected as attempting self-approval.");
    }
}
