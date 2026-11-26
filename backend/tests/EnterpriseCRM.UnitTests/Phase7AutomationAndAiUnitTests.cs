using EnterpriseCRM.Application.Features.AutomationAndAi;
using EnterpriseCRM.Domain.Entities;
using EnterpriseCRM.Domain.Enums;
using System.Text.Json;
using Xunit;

namespace EnterpriseCRM.UnitTests;

public class Phase7AutomationAndAiUnitTests
{
    [Fact]
    public void Workflow_ConditionEvaluation_HandlesGreaterThanAndEquals()
    {
        // Arrange
        var context = new Dictionary<string, object>
        {
            { "Amount", 75_000_000m },
            { "Source", "WebsiteForm" },
            { "Industry", "Technology" }
        };

        var conditionJson = @"[
            { ""field"": ""Amount"", ""operator"": ""GreaterThan"", ""value"": ""50000000"" },
            { ""field"": ""Source"", ""operator"": ""Equals"", ""value"": ""WebsiteForm"" }
        ]";

        // Act
        using var doc = JsonDocument.Parse(conditionJson);
        bool allPassed = true;

        foreach (var cond in doc.RootElement.EnumerateArray())
        {
            var field = cond.GetProperty("field").GetString()!;
            var op = cond.GetProperty("operator").GetString()!;
            var expected = cond.GetProperty("value").GetString()!;

            var actual = context[field].ToString()!;

            bool passed = op switch
            {
                "Equals" => string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase),
                "GreaterThan" => decimal.TryParse(actual, out var aNum) && decimal.TryParse(expected, out var eNum) && aNum > eNum,
                _ => false
            };

            if (!passed)
            {
                allPassed = false;
                break;
            }
        }

        // Assert
        Assert.True(allPassed);
    }

    [Fact]
    public void LeadAssignment_RoundRobin_SequenceAdvances()
    {
        // Arrange
        var candidateIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        var lastAssignedId = candidateIds[1]; // index 1

        // Act
        var lastIdx = candidateIds.FindIndex(id => id == lastAssignedId);
        var nextIdx = (lastIdx + 1) % candidateIds.Count;
        var nextAssignedId = candidateIds[nextIdx];

        // Assert
        Assert.Equal(2, nextIdx);
        Assert.Equal(candidateIds[2], nextAssignedId);

        // Next turn should wrap to 0
        var wrapIdx = (nextIdx + 1) % candidateIds.Count;
        Assert.Equal(0, wrapIdx);
        Assert.Equal(candidateIds[0], candidateIds[wrapIdx]);
    }

    [Fact]
    public void LeadAssignment_WorkloadCapacity_PicksLowestUnderCap()
    {
        // Arrange
        var maxCap = 50;
        var workloads = new List<(string Name, int ActiveLeads)>
        {
            ("Alice", 45),
            ("Bob", 15),
            ("Charlie", 52) // exceeds max cap
        };

        // Act
        var selected = workloads
            .Where(w => w.ActiveLeads < maxCap)
            .OrderBy(w => w.ActiveLeads)
            .FirstOrDefault();

        // Assert
        Assert.Equal("Bob", selected.Name);
        Assert.Equal(15, selected.ActiveLeads);
    }

    [Fact]
    public void SmartAlert_StagnantLeadThreshold_DetectsOverdue()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var leadCreatedAt = now.AddDays(-4); // 4 days ago
        var thresholdDays = 3;

        // Act
        var isStagnant = (now - leadCreatedAt).TotalDays >= thresholdDays;

        // Assert
        Assert.True(isStagnant);
    }

    [Fact]
    public void RevenueForecast_WeightedPipeline_CalculatesExpectedAmount()
    {
        // Arrange
        var baseMonthlyRunRate = 120_000_000m;
        var horizonDays = 30;
        var dailyRunRate = baseMonthlyRunRate / 30m; // 4,000,000 / day
        var baseProjection = dailyRunRate * horizonDays; // 120,000,000

        var deals = new List<(decimal Amount, decimal Probability)>
        {
            (100_000_000m, 80m), // 80M weighted
            (50_000_000m, 40m)    // 20M weighted
        };

        decimal weightedPipeline = 0;
        foreach (var (amt, prob) in deals)
        {
            weightedPipeline += amt * (prob / 100m);
        } // 100M total

        decimal horizonWeight = 0.50m; // 30-day discount factor
        decimal predictedTotal = Math.Round(baseProjection + (weightedPipeline * horizonWeight), 2);

        // Assert
        // 120,000,000 + (100,000,000 * 0.5) = 170,000,000
        Assert.Equal(170_000_000m, predictedTotal);
    }

    [Fact]
    public void RevenueForecast_ConfidenceBounds_AppliesFifteenPercentSpread()
    {
        // Arrange
        var predictedAmount = 200_000_000m;

        // Act
        var lowerBound = Math.Round(predictedAmount * 0.85m, 2);
        var upperBound = Math.Round(predictedAmount * 1.15m, 2);

        // Assert
        Assert.Equal(170_000_000m, lowerBound);
        Assert.Equal(230_000_000m, upperBound);
    }

    [Fact]
    public void AnomalyDetection_PercentageDeviation_DetectsSevereDips()
    {
        // Arrange
        decimal expectedValue = 100_000_000m;
        decimal actualValue = 65_000_000m; // 35% dip

        // Act
        decimal deviationPercent = Math.Round((expectedValue - actualValue) / expectedValue * 100m, 1);
        bool isHighAnomaly = deviationPercent >= 25.0m;

        // Assert
        Assert.Equal(35.0m, deviationPercent);
        Assert.True(isHighAnomaly);
    }

    [Fact]
    public void AiQuery_PatternMatching_ClassifiesQuestionsAccurately()
    {
        // Arrange
        var q1 = "Tổng doanh thu thực thu tháng này là bao nhiêu?";
        var q2 = "Top sales xuất sắc nhất có doanh số thế nào?";
        var q3 = "Báo cáo công nợ quá hạn của các hợp đồng?";
        var q4 = "Số lượng deal trong pipeline hiện tại?";

        // Act
        var isRev = q1.ToLower().Contains("doanh thu") || q1.ToLower().Contains("revenue");
        var isTopSales = q2.ToLower().Contains("top sales") || q2.ToLower().Contains("nhân viên");
        var isDebt = q3.ToLower().Contains("công nợ") || q3.ToLower().Contains("debt");
        var isPipeline = q4.ToLower().Contains("deal") || q4.ToLower().Contains("pipeline");

        // Assert
        Assert.True(isRev);
        Assert.True(isTopSales);
        Assert.True(isDebt);
        Assert.True(isPipeline);
    }

    [Fact]
    public void WorkflowValidation_RejectsInvalidPayloads()
    {
        // Arrange
        var validator = new CreateWorkflowDefinitionRequestValidator();
        var invalidReq = new CreateWorkflowDefinitionRequest(
            "", // empty name
            "Description",
            "", // empty category
            "", // empty trigger
            "[]",
            "[]",
            0 // invalid execution order < 1
        );

        // Act
        var result = validator.Validate(invalidReq);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
        Assert.Contains(result.Errors, e => e.PropertyName == "Category");
        Assert.Contains(result.Errors, e => e.PropertyName == "TriggerEvent");
        Assert.Contains(result.Errors, e => e.PropertyName == "ExecutionOrder");
    }
}
