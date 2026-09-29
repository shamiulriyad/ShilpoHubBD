using ShilpoHubBD.Application.DTOs.CounterfeitDetection;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.Infrastructure.CounterfeitDetection;

namespace ShilpoHubBD.UnitTests.Features.CounterfeitDetection.Infrastructure;

[Trait("Feature", "CounterfeitDetection")]
[Trait("Layer", "Infrastructure")]
public class RuleBasedCounterfeitDetectionProviderTests
{
    private readonly RuleBasedCounterfeitDetectionProvider _provider = new();

    private static CounterfeitCheckContext MakeContext(
        HandmadeVerificationStatus handmade = HandmadeVerificationStatus.Verified,
        ProductApprovalStatus approval = ProductApprovalStatus.Approved,
        decimal price = 1000, decimal categoryAveragePrice = 1000, int categorySampleSize = 0,
        int reviewCount = 5, int salesCount = 5) => new()
    {
        ProductName = "x", HandmadeVerificationStatus = handmade, ApprovalStatus = approval,
        Price = price, CategoryAveragePrice = categoryAveragePrice, CategorySampleSize = categorySampleSize,
        ReviewCount = reviewCount, SalesCount = salesCount,
    };

    [Fact]
    public void Check_NoRiskSignals_ReturnsLowWithZeroScoreAndAPlaceholderMessage()
    {
        var (score, level, signals) = _provider.Check(MakeContext());

        Assert.Equal(0, score);
        Assert.Equal("Low", level);
        Assert.Equal("No counterfeit risk signals detected.", Assert.Single(signals));
    }

    [Fact]
    public void Check_FailedHandmadeVerification_AddsFortyPointsAndASignal()
    {
        var (score, level, signals) = _provider.Check(MakeContext(handmade: HandmadeVerificationStatus.Rejected));

        Assert.Equal(40, score);
        Assert.Equal("Medium", level);
        Assert.Contains("Failed handmade verification.", signals);
    }

    [Fact]
    public void Check_RejectedListing_AddsThirtyPointsAndASignal()
    {
        var (score, _, signals) = _provider.Check(MakeContext(approval: ProductApprovalStatus.Rejected));

        Assert.Equal(30, score);
        Assert.Contains("Listing was rejected at admin approval.", signals);
    }

    [Fact]
    public void Check_PriceFarBelowCategoryAverageWithEnoughSampleSize_AddsTwentyFivePoints()
    {
        var (score, _, signals) = _provider.Check(MakeContext(price: 300, categoryAveragePrice: 1000, categorySampleSize: 5));

        Assert.Equal(25, score);
        Assert.Contains(signals, s => s.Contains("far below the category average"));
    }

    [Fact]
    public void Check_PriceFarBelowAverageButSampleSizeTooSmall_DoesNotFlagPrice()
    {
        var (score, _, signals) = _provider.Check(MakeContext(price: 300, categoryAveragePrice: 1000, categorySampleSize: 4));

        Assert.Equal(0, score);
        Assert.DoesNotContain(signals, s => s.Contains("category average"));
    }

    [Fact]
    public void Check_HighSalesWithNoReviews_AddsFifteenPoints()
    {
        var (score, _, signals) = _provider.Check(MakeContext(salesCount: 21, reviewCount: 0));

        Assert.Equal(15, score);
        Assert.Contains("Significant sales volume with no reviews.", signals);
    }

    [Fact]
    public void Check_AllSignalsTriggered_ClampsScoreAtOneHundredAndReturnsHigh()
    {
        var context = MakeContext(
            handmade: HandmadeVerificationStatus.Rejected, approval: ProductApprovalStatus.Rejected,
            price: 100, categoryAveragePrice: 1000, categorySampleSize: 10, salesCount: 50, reviewCount: 0);

        var (score, level, signals) = _provider.Check(context);

        Assert.Equal(100, score);
        Assert.Equal("High", level);
        Assert.Equal(4, signals.Count);
    }

    [Fact]
    public void Check_ScoreExactlyThirty_IsMedium()
    {
        // Rejected approval alone scores exactly 30, the low edge of the Medium band.
        var (score, level, _) = _provider.Check(MakeContext(approval: ProductApprovalStatus.Rejected));

        Assert.Equal(30, score);
        Assert.Equal("Medium", level);
    }

    [Fact]
    public void Check_ScoreBelowSixty_IsStillMedium()
    {
        // Rejected approval (30) + underpriced (25) = 55, still under the High threshold of 60.
        var (score, level, _) = _provider.Check(MakeContext(approval: ProductApprovalStatus.Rejected, price: 300, categoryAveragePrice: 1000, categorySampleSize: 5));

        Assert.Equal(55, score);
        Assert.Equal("Medium", level);
    }

    [Fact]
    public void Check_ScoreAtLeastSixty_IsHigh()
    {
        // Rejected handmade (40) + rejected approval (30) = 70, over the High threshold of 60.
        var (score, level, _) = _provider.Check(MakeContext(handmade: HandmadeVerificationStatus.Rejected, approval: ProductApprovalStatus.Rejected));

        Assert.Equal(70, score);
        Assert.Equal("High", level);
    }
}
