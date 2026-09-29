using ShilpoHubBD.Application.Common;

namespace ShilpoHubBD.UnitTests.Features.Platform.Common;

[Trait("Feature", "Platform")]
[Trait("Layer", "Helper")]
public class SlugGeneratorTests
{
    [Theory]
    [InlineData("Jamdani Saree", "jamdani-saree")]
    [InlineData("  Nakshi  Kantha  ", "nakshi-kantha")]
    [InlineData("Sylhet's Best!", "sylhet-s-best")]
    [InlineData("100% Cotton", "100-cotton")]
    [InlineData("UPPER CASE", "upper-case")]
    public void Generate_TypicalNames_ProducesTheExpectedSlug(string input, string expected)
        => Assert.Equal(expected, SlugGenerator.Generate(input));

    [Fact]
    public void Generate_MultipleConsecutiveSeparators_CollapseToOneHyphen()
        => Assert.Equal("a-b", SlugGenerator.Generate("a   ---   b"));

    [Fact]
    public void Generate_LeadingAndTrailingPunctuation_IsTrimmed()
        => Assert.Equal("middle", SlugGenerator.Generate("---middle---"));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("!!!")]
    public void Generate_NoAlphanumericCharacters_FallsBackToItem(string input)
        => Assert.Equal("item", SlugGenerator.Generate(input));

    [Fact]
    public void Generate_AlreadySlugLike_IsUnchanged()
        => Assert.Equal("already-a-slug", SlugGenerator.Generate("already-a-slug"));

    [Fact]
    public void Generate_UnicodeLetters_AreKeptButCombiningVowelSignsAreNot()
        // char.IsLetterOrDigit treats Bengali vowel signs (a "Mark, Nonspacing" category, e.g. the "া"
        // in "দানি") as punctuation, not letters, so they are dropped along with the actual separators.
        => Assert.Equal("জ-মদ-ন-শ-ড", SlugGenerator.Generate("জামদানি শাড়ি"));

    [Fact]
    public void Generate_IsIdempotent()
    {
        var once = SlugGenerator.Generate("Jamdani Saree — Premium!");

        Assert.Equal(once, SlugGenerator.Generate(once));
    }
}
