using BulkRenamer.Core.Enums;
using BulkRenamer.Core.Models;
using BulkRenamer.Core.Services;

namespace BulkRenamer.Tests;

// Tests all combinations of ApplyMode with plain-text replace.
public sealed class RenameServicePlainTextTests
{
    private readonly IRenameService _sut = new RenameService();

    private static string[] SingleFile(string name) =>
        new[] { $@"C:\Assets\{name}.fbx" };

    private static RenameSettings PlainSettings(
        ApplyMode apply,
        string find,
        string replace,
        bool caseSensitive = true) => new()
    {
        ReplaceMode   = ReplaceMode.PlainText,
        ApplyMode     = apply,
        FindText      = find,
        ReplaceText   = replace,
        CaseSensitive = caseSensitive
    };

    [Fact]
    public void Anywhere_ReplacesAllOccurrences()
    {
        var settings = PlainSettings(ApplyMode.Anywhere, "SM_", "Hero_");
        var previews = _sut.BuildPreview(SingleFile("SM_Wep_SM_01"), settings);

        Assert.Equal("Hero_Wep_Hero_01", previews[0].NewName);
    }

    [Fact]
    public void PrefixOnly_ReplacesOnlyLeadingMatch()
    {
        var settings = PlainSettings(ApplyMode.PrefixOnly, "SM_", "Hero_");
        var previews = _sut.BuildPreview(SingleFile("SM_Wep_SM_01"), settings);

        // Only the leading SM_ should be replaced, the internal one stays.
        Assert.Equal("Hero_Wep_SM_01", previews[0].NewName);
    }

    [Fact]
    public void SuffixOnly_ReplacesOnlyTrailingMatch()
    {
        var settings = PlainSettings(ApplyMode.SuffixOnly, "_LOD0", string.Empty);
        var previews = _sut.BuildPreview(SingleFile("SM_Weapon_LOD0"), settings);

        Assert.Equal("SM_Weapon", previews[0].NewName);
    }

    [Fact]
    public void PrefixOnly_NoMatch_LeavesNameUnchanged()
    {
        var settings = PlainSettings(ApplyMode.PrefixOnly, "SM_", "Hero_");
        var previews = _sut.BuildPreview(SingleFile("Weapon_SM_01"), settings);

        Assert.Equal(RenamePreviewStatus.NoChange, previews[0].Status);
    }

    [Fact]
    public void CaseInsensitive_Anywhere_Replaces()
    {
        var settings = PlainSettings(ApplyMode.Anywhere, "sm_", "Hero_", caseSensitive: false);
        var previews = _sut.BuildPreview(SingleFile("SM_Weapon"), settings);

        Assert.Equal("Hero_Weapon", previews[0].NewName);
    }

    [Fact]
    public void EmptyFindText_LeavesNameUnchanged()
    {
        var settings = PlainSettings(ApplyMode.Anywhere, string.Empty, "Hero_");
        var previews = _sut.BuildPreview(SingleFile("SM_Weapon"), settings);

        Assert.Equal(RenamePreviewStatus.NoChange, previews[0].Status);
    }
}
