using System.IO;
using System.Text.RegularExpressions;
using BulkRenamer.Core.Enums;
using BulkRenamer.Core.Models;

namespace BulkRenamer.Core.Services;

public sealed class RenameService : IRenameService
{

    #region Public API

    public IReadOnlyList<RenamePreview> BuildPreview(IEnumerable<string> files, RenameSettings settings)
    {
        // Build a per-folder set of existing names so collision detection is O(1).
        var existingNames = BuildExistingNamesLookup(files);

        var previews = new List<RenamePreview>();

        foreach (var fullPath in files)
        {
            var oldName = Path.GetFileNameWithoutExtension(fullPath);

            if (settings.UseFilter && !MatchesFilter(oldName, settings))
                continue;

            var newName = ComputeNewName(oldName, settings);
            var status  = ResolveStatus(fullPath, oldName, newName, existingNames, settings);

            previews.Add(new RenamePreview(fullPath, oldName, newName, status));
        }

        return previews;
    }

    public int ApplyRenames(IReadOnlyList<RenamePreview> previews, RenameSettings settings)
    {
        var renamed = 0;

        foreach (var preview in previews)
        {
            if (settings.SkipIfNoChange && preview.Status == RenamePreviewStatus.NoChange)
                continue;

            if (settings.SkipIfNameCollision && preview.Status == RenamePreviewStatus.Collision)
                continue;

            var extension  = Path.GetExtension(preview.FullPath);
            var directory  = Path.GetDirectoryName(preview.FullPath) ?? string.Empty;
            var targetPath = Path.Combine(directory, preview.NewName + extension);

            try
            {
                File.Move(preview.FullPath, targetPath);
                renamed++;
            }
            catch (IOException ex)
            {
                // Log and continue — one failure must not abort the whole batch.
                Console.Error.WriteLine($"[RenameService] Failed to rename '{preview.FullPath}': {ex.Message}");
            }
        }

        return renamed;
    }

    #endregion

    #region Filter

    private static bool MatchesFilter(string name, RenameSettings settings)
    {
        if (string.IsNullOrEmpty(settings.FilterText)) return true;

        var comparison = settings.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

        return settings.FilterMatch switch
        {
            MatchMode.Contains   => name.Contains(settings.FilterText, comparison),
            MatchMode.StartsWith => name.StartsWith(settings.FilterText, comparison),
            MatchMode.EndsWith   => name.EndsWith(settings.FilterText, comparison),
            MatchMode.Exact      => string.Equals(name, settings.FilterText, comparison),
            _                    => true
        };
    }

    #endregion

    #region Name Computation

   private static string ComputeNewName(string oldName, RenameSettings settings)
    {
        if (string.IsNullOrEmpty(oldName)) return oldName;

        return settings.ReplaceMode == ReplaceMode.Regex ? ComputeRegex(oldName, settings) : ComputePlain(oldName, settings);
    }

    private static string ComputePlain(string oldName, RenameSettings settings)
    {
        var comparison = settings.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

        return settings.ApplyMode switch
        {
            ApplyMode.Anywhere   => ReplaceAnywhere(oldName, settings.FindText, settings.ReplaceText, comparison),
            ApplyMode.PrefixOnly => ReplacePrefix(oldName, settings.FindText, settings.ReplaceText, comparison),
            ApplyMode.SuffixOnly => ReplaceSuffix(oldName, settings.FindText, settings.ReplaceText, comparison),
            _                    => oldName
        };
    }

    private static string ComputeRegex(string oldName, RenameSettings settings)
    {
        if (string.IsNullOrEmpty(settings.FindText))
            return oldName;

        var options = settings.CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;

        // Anchor the pattern automatically based on ApplyMode.
        var pattern = settings.ApplyMode switch
        {
            ApplyMode.PrefixOnly => "^" + settings.FindText,
            ApplyMode.SuffixOnly => settings.FindText + "$",
            _                    => settings.FindText
        };

        try
        {
            return Regex.Replace(oldName, pattern, settings.ReplaceText ?? string.Empty, options);
        }
        catch (ArgumentException)
        {
            // Return original name if the pattern is invalid.
            return oldName;
        }
    }
#endregion

    #region Plain text Replace Helpers

    private static string ReplaceAnywhere(string input, string find, string replace, StringComparison comparison)
    {
        if (string.IsNullOrEmpty(find)) return input;

        // Use the fast built-in overload for ordinal (case-sensitive) replacements.
        if (comparison == StringComparison.Ordinal) return input.Replace(find, replace, StringComparison.Ordinal);

        // Fall back to Regex for case-insensitive plain-text replace.
        return Regex.Replace(input, Regex.Escape(find), replace ?? string.Empty, RegexOptions.IgnoreCase);
    }

    private static string ReplacePrefix(string input, string find, string replace, StringComparison comparison)
    {
        if (string.IsNullOrEmpty(find)) return input;

        return input.StartsWith(find, comparison) ? (replace ?? string.Empty) + input[find.Length..] : input;
    }

    private static string ReplaceSuffix(string input, string find, string replace, StringComparison comparison)
    {
        if (string.IsNullOrEmpty(find)) return input;

        return input.EndsWith(find, comparison) ? input[..^find.Length] + (replace ?? string.Empty) : input;
    }


    #endregion

    #region Collision Detection

    private static RenamePreviewStatus ResolveStatus(string fullPath, string oldName, string newName, Dictionary<string, HashSet<string>> existingNames, RenameSettings settings)
    {
        if (string.Equals(oldName, newName, StringComparison.Ordinal)) return RenamePreviewStatus.NoChange;

        var folder = Path.GetDirectoryName(fullPath) ?? string.Empty;

        if (existingNames.TryGetValue(folder, out var names) && names.Contains(newName)) return RenamePreviewStatus.Collision;

        return RenamePreviewStatus.WillRename;
    }

    /// Builds a lookup of folder → existing file names (without extension).
    private static Dictionary<string, HashSet<string>> BuildExistingNamesLookup(IEnumerable<string> files)
    {
        var lookup = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);

        foreach (var file in files)
        {
            var folder = Path.GetDirectoryName(file) ?? string.Empty;
            var name   = Path.GetFileNameWithoutExtension(file);

            if (!lookup.TryGetValue(folder, out var set))
            {
                set = new HashSet<string>(StringComparer.Ordinal);
                lookup[folder] = set;
            }

            set.Add(name);
        }

        return lookup;
    }

    #endregion

}
