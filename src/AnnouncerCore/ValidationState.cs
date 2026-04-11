namespace GreyAnnouncer.AnnouncerCore;

public enum ValidationState
{
    Success,
    IndividualCooldown,
    DisabledByConfig,
    ComponentsNotInitialized,
    InvalidKey,
    ConfigNotLoaded
}