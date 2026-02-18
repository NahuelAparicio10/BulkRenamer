using BulkRenamer.Core.Enums;
using BulkRenamer.Core.Models;
using BulkRenamer.Core.Services;

namespace BulkRenamer.Tests;

// Tests that the filter correctly includes or excludes files before renaming.
public sealed class RenameServiceFilterTests
{
    private readonly IRenameService _sut = new RenameService();

    private static RenameSettings FilterSettings(MatchMode match, string filterText, bool caseSensitive = true) => new()
    {
        UseFilter     = true,
        FilterMatch   = match,
        FilterText    = filterText,
        CaseSensitive = caseSensitive,
        FindText      = "x",  // irrelevant for filter tests
        ReplaceText   = "y"
    };

    [Theory]
    [InlineData("SM_Weapon_01", true)]
    [InlineData("Weapon_Idle",  true)]
    [InlineData("Hero_Run",     false)]
    public void Contains_IncludesOnlyMatchingFiles(string fileName, bool shouldInclude)
    {
        var settings = FilterSettings(MatchMode.Contains, "Weapon");
        var files    = new[] { $@"C:\Assets\{fileName}.fbx" };

        var previews = _sut.BuildPreview(files, settings);

        Assert.Equal(shouldInclude, previews.Count == 1);
    }

    [Theory]
    [InlineData("SM_Weapon", true)]
    [InlineData("Weapon_SM", false)]
    public void StartsWith_IncludesOnlyPrefixMatches(string fileName, bool shouldInclude)
    {
        var settings = FilterSettings(MatchMode.StartsWith, "SM_");
        var files    = new[] { $@"C:\Assets\{fileName}.fbx" };

        var previews = _sut.BuildPreview(files, settings);

        Assert.Equal(shouldInclude, previews.Count == 1);
    }

    [Theory]
    [InlineData("Weapon_LOD0", true)]
    [InlineData("LOD0_Weapon", false)]
    public void EndsWith_IncludesOnlySuffixMatches(string fileName, bool shouldInclude)
    {
        var settings = FilterSettings(MatchMode.EndsWith, "LOD0");
        var files    = new[] { $@"C:\Assets\{fileName}.fbx" };

        var previews = _sut.BuildPreview(files, settings);

        Assert.Equal(shouldInclude, previews.Count == 1);
    }

    [Fact]
    public void CaseInsensitive_MatchesRegardlessOfCase()
    {
        var settings = FilterSettings(MatchMode.Contains, "weapon", caseSensitive: false);
        var files    = new[] { @"C:\Assets\SM_WEAPON_01.fbx" };

        var previews = _sut.BuildPreview(files, settings);

        Assert.Single(previews);
    }

    [Fact]
    public void FilterDisabled_IncludesAllFiles()
    {
        var settings = new RenameSettings { UseFilter = false, FindText = "x", ReplaceText = "y" };
        var files    = new[] { @"C:\Assets\Hero.fbx", @"C:\Assets\Villain.fbx" };

        var previews = _sut.BuildPreview(files, settings);

        Assert.Equal(2, previews.Count);
    }
}
