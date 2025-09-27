using Common.Parser;

namespace Common.Tests;

public class TitleParserTests
{
    [Fact]
    public void ExtractTitle_ReturnsCorrectTitle_ForStandardInput()
    {
        var input = "Small.Things.Like.These.2024.2160p.4K.WEB.x265.10bit.AAC5.1-[YTS.MX].mkv";
        var expected = "Small Things Like These";
        var result = TitleParser.ExtractTitle(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ExtractTitle_ReturnsCorrectTitle_ForInputWithYearInTitle()
    {
        var input = "2001.A.Space.Odyssey.1968.2160p.4K.BluRay.x265.10bit.AAC5.1-[YTS.MX].mkv";
        var expected = "2001 A Space Odyssey";
        var result = TitleParser.ExtractTitle(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ExtractTitle_ReturnsCorrectTitle_ForInputWithoutTags()
    {
        var input = "The.Wild.Robot.2024.mkv";
        var expected = "The Wild Robot";
        var result = TitleParser.ExtractTitle(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ExtractTitle_ReturnsEmptyString_ForInputWithOnlyTags()
    {
        var input = "2160p.4K.WEB.x265.10bit.AAC5.1-[YTS.MX].mkv";
        var expected = string.Empty;
        var result = TitleParser.ExtractTitle(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ExtractTitle_IgnoresNonAlphanumericCharacters()
    {
        var input = "Some.Movie!@#.2023.1080p.BluRay.x264.mkv";
        var expected = "Some Movie";
        var result = TitleParser.ExtractTitle(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ExtractTitle_ReturnsCorrectTitle_ForInputWithMultipleSpaces()
    {
        var input = "A.Movie.With..Extra.Dots.2023.1080p.BluRay.x264.mkv";
        var expected = "A Movie With Extra Dots";
        var result = TitleParser.ExtractTitle(input);
        Assert.Equal(expected, result);
    }

}