namespace GreyAnnouncer.AnnouncerAPI;

public enum ValidationState
{
    Success,
    IndividualCooldown,
    DisabledByConfig,
    ComponentsNotInitialized,
    InvalidKey,
    ConfigNotLoaded
}