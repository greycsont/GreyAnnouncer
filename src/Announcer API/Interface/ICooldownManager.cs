
namespace GreyAnnouncer.AnnouncerAPI;

public interface ICooldownManager
{
    public bool IsIndividualCooldownActive(string category);
    public void StartCooldowns(string category, float duration);
    public void ResetCooldowns();

}