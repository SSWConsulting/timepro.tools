using FluentAssertions;
using SSW.TimePro.Cli.Infrastructure.Guides;
using Xunit;

using AccountingGuide = SSW.TimePro.Cli.Features.Accounting.AccountingGuide;
using DeveloperGuide = SSW.TimePro.Cli.Features.Developer.DeveloperGuide;

namespace SSW.TimePro.Cli.Tests.Features.Guides;

public class GuideRankingTests
{
    [Fact]
    public void DeveloperGuide_FiltersSuggestedUseCaseAsExactKeywordMatch()
    {
        var guide = DeveloperGuide.For("suggested");

        guide.MatchingGuides.Should().NotBeEmpty();
        guide.MatchingGuides[0].Title.Should().Be("Suggested timesheets missing");
        guide.MatchingGuides[0].MatchType.Should().Be("exact");
        guide.RecommendedCommands.Should().Contain(command => command.Contains("tp ts suggest", StringComparison.Ordinal));
        guide.RecommendedSkills.Should().Contain("timepro-dev-timesheet-diagnostics");
        guide.RecommendedSkills.Should().NotContain("timepro-dev-finance-diagnostics");
    }

    [Fact]
    public void DeveloperGuide_RanksContainsAllAboveContainsOne()
    {
        var guide = DeveloperGuide.For("refresh employee");

        guide.MatchingGuides.Should().NotBeEmpty();
        guide.MatchingGuides[0].Title.Should().Be("Suggested timesheets missing");
        guide.MatchingGuides[0].MatchType.Should().Be("contains-all");
    }

    [Fact]
    public void DeveloperGuide_ReturnsAtLeastOneWordMatches()
    {
        var guide = DeveloperGuide.For("suggested invoice");

        guide.MatchingGuides.Should().Contain(match => match.Title == "Suggested timesheets missing");
        guide.MatchingGuides.Should().Contain(match => match.Title == "Invoice tax bug");
        guide.MatchingGuides.Should().OnlyContain(match => match.MatchRank == 1);
    }

    [Fact]
    public void AccountingGuide_FiltersInvoiceReconciliationAsExactKeywordMatch()
    {
        var guide = AccountingGuide.For("invoice reconciliation");

        guide.MatchingGuides.Should().NotBeEmpty();
        guide.MatchingGuides[0].Title.Should().Be("Invoice evidence pack");
        guide.MatchingGuides[0].MatchType.Should().Be("exact");
        guide.RecommendedSkills.Should().Equal("timepro-accounting-cli");
    }

    [Fact]
    public void AccountingGuide_FiltersClientDebtAsContainsAll()
    {
        var guide = AccountingGuide.For("aged debtor client");

        guide.MatchingGuides.Should().NotBeEmpty();
        guide.MatchingGuides[0].Title.Should().Be("Client accounting position");
        guide.MatchingGuides[0].MatchType.Should().Be("contains-all");
    }

    [Fact]
    public void GuideRanking_ReturnsAllTopicsWithoutQuery()
    {
        var topics = new[]
        {
            new GuideDocument("dev", "one", "One", "First", ["first"], ["cmd one"], [], ["skill-one"], "test", "body"),
            new GuideDocument("dev", "two", "Two", "Second", ["second"], ["cmd two"], [], ["skill-two"], "test", "body")
        };

        var ranked = GuideRanking.Rank(null, topics);

        ranked.Select(match => match.Title).Should().Equal("One", "Two");
        ranked.Should().OnlyContain(match => match.MatchType == "default" && match.MatchRank == 0);
    }

    [Fact]
    public void GuideCatalog_LoadsEmbeddedIndex()
    {
        var guides = GuideCatalog.Load("dev");

        guides.Should().Contain(guide => guide.Slug == "suggested-timesheets-missing"
            && guide.Title == "Suggested timesheets missing"
            && guide.Commands.Any(command => command.Contains("tp ts suggest", StringComparison.Ordinal))
            && guide.Skills.Contains("timepro-dev-timesheet-diagnostics"));
    }
}
