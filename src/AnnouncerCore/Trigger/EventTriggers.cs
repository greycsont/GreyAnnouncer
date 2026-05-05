namespace GreyAnnouncer.AnnouncerCore;

public static class EventTriggers
{
    public static readonly EventTrigger<int> StyleHUD = new();
    public static readonly EventTrigger<int> FinalRank = new();
    public static readonly EventTrigger<int> GunControl = new();
}
