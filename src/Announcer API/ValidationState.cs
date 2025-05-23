namespace GreyAnnouncer.AnnouncerAPI;

public enum ValidationState
{
    Success,
    AudioFailedLoading,
    SharedCooldown,
    IndividualCooldown,
    DisabledByConfig,
    ClipNotFound,
    ValidationError,
    ComponentsNotInitialized,
    InvalidKey
}