using System;

public static class GuidPrefixAdder
{
    private const string Prefix = "GreyAnnouncer_";

    public static string AddPrefixToGUID(params string[] GUIDs)
    {
        if (GUIDs == null || GUIDs.Length == 0)
            throw new ArgumentException("At least one key must be provided.", nameof(GUIDs));

        string combined = string.Join("_", GUIDs).ToUpper();
        return $"{Prefix}{combined}";
    }
}