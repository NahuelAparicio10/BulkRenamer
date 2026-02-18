namespace BulkRenamer.Core.Enums;
public static class MatchModeItems
{
    public static MatchMode[] All { get; } = (MatchMode[])Enum.GetValues(typeof(MatchMode));
}

public static class ApplyModeItems
{
    public static ApplyMode[] All { get; } = (ApplyMode[])Enum.GetValues(typeof(ApplyMode));
}

public static class ReplaceModeItems
{
    public static ReplaceMode[] All { get; } = (ReplaceMode[])Enum.GetValues(typeof(ReplaceMode));
}
