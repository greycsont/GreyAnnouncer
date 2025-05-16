public static class GuidPrefixAdder //this motherfucker should be added to all items, but idk how to decoupe the code
{
    public static string AddPrefixToGUID(string rankKey)
    {
        return $"GreyAnnouncer_{rankKey.ToUpper()}";
    }

    public static string RemovePrefixFromGUID(string guid)
    {
        if (guid.StartsWith("GreyAnnouncer_"))
        {
            return guid.Substring("GreyAnnouncer_".Length);
        }
        return guid;
    }
}