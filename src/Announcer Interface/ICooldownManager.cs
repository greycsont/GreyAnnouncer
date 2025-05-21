interface ICooldownManager
{
    public bool IsIndividualCooldownActive(string category);
    public bool IsSharedCooldownActive();
    public void StartCooldowns(string category, float duration);
    public void ResetCooldowns();

}