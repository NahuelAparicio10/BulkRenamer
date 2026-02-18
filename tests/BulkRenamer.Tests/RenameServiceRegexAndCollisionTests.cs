using BulkRenamer.Core.Enums;
using BulkRenamer.Core.Models;
using BulkRenamer.Core.Services;

namespace BulkRenamer.Tests;

/// Tests regex replace and collision detection logic.
public sealed class RenameServiceRegexAndCollisionTests
{
    private readonly IRenameService _sut = new RenameService();

    [Fact]
    public void Regex_Anywhere_ReplacesPattern()
    {
        var settings = new RenameSettings
        {
            ReplaceMode = ReplaceMode.Regex,
            ApplyMode   = ApplyMode.Anywhere,
            FindText    = @"_LOD\d+",
            ReplaceText = string.Empty
        };
        var files    = new[] { @"C:\Assets\SM_Weapon_LOD0.fbx" };
        var previews = _sut.BuildPreview(files, settings);

        Assert.Equal("SM_Weapon", previews[0].NewName);
    }

    [Fact]
    public void Regex_PrefixOnly_AnchorsPatternAutomatically()
    {
        var settings = new RenameSettings
        {
            ReplaceMode = ReplaceMode.Regex,
            ApplyMode   = ApplyMode.PrefixOnly,
            FindText    = @"SM_",
            ReplaceText = "Hero_"
        };
        // The internal SM_ should NOT be replaced because ApplyMode anchors to prefix.
        var files    = new[] { @"C:\Assets\SM_Wep_SM_01.fbx" };
        var previews = _sut.BuildPreview(files, settings);

        Assert.Equal("Hero_Wep_SM_01", previews[0].NewName);
    }

    [Fact]
    public void Regex_InvalidPattern_ReturnsOriginalName()
    {
        var settings = new RenameSettings
        {
            ReplaceMode = ReplaceMode.Regex,
            FindText    = "[invalid(",  // intentionally broken pattern
            ReplaceText = "x"
        };
        var files    = new[] { @"C:\Assets\SM_Weapon.fbx" };
        var previews = _sut.BuildPreview(files, settings);

        // Must not throw — returns NoChange instead.
        Assert.Equal(RenamePreviewStatus.NoChange, previews[0].Status);
    }

    [Fact]
    public void Collision_DetectedWhenTargetNameAlreadyExists()
    {
        // Two files in the same folder where renaming the first produces the second's name.
        var files = new[]
        {
            @"C:\Assets\SM_Weapon.fbx",
            @"C:\Assets\Hero_Weapon.fbx"  // target name already exists
        };
        var settings = new RenameSettings
        {
            FindText    = "SM_",
            ReplaceText = "Hero_",
            ApplyMode   = ApplyMode.PrefixOnly
        };

        var previews = _sut.BuildPreview(files, settings);
        var collided = previews.First(p => p.OldName == "SM_Weapon");

        Assert.Equal(RenamePreviewStatus.Collision, collided.Status);
    }

    [Fact]
    public void NoCollision_WhenFilesAreInDifferentFolders()
    {
        // Same name collision should NOT trigger across different directories.
        var files = new[]
        {
            @"C:\Assets\Weapons\SM_Weapon.fbx",
            @"C:\Assets\Heroes\Hero_Weapon.fbx"
        };
        var settings = new RenameSettings
        {
            FindText    = "SM_",
            ReplaceText = "Hero_",
            ApplyMode   = ApplyMode.PrefixOnly
        };

        var previews = _sut.BuildPreview(files, settings);
        var weaponPreview = previews.First(p => p.OldName == "SM_Weapon");

        Assert.Equal(RenamePreviewStatus.WillRename, weaponPreview.Status);
    }

    [Fact]
    public void NoChange_WhenComputedNameIsIdentical()
    {
        var settings = new RenameSettings
        {
            FindText    = "NotPresent",
            ReplaceText = "Hero_"
        };
        var files    = new[] { @"C:\Assets\SM_Weapon.fbx" };
        var previews = _sut.BuildPreview(files, settings);

        Assert.Equal(RenamePreviewStatus.NoChange, previews[0].Status);
    }
}
