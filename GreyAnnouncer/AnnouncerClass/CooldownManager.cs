using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace greycsont.GreyAnnouncer;

public class CooldownManager
{
    private Dictionary<string, float> m_IndividualCooldowns;

    #region Public API
    public CooldownManager(string[] audioCategories)
    {
        foreach (var category in audioCategories)
        {
            m_IndividualCooldowns[category] = 0f;
        }
    }

    public bool IsIndividualCooldownActive(string category)
    {
        return m_IndividualCooldowns[category] > 0f;
    }

    public bool IsSharedCooldownActive()
    {
        return AnnouncerManager.sharedCooldown > 0f;
    }

    public void StartIndividualCooldown(string category, float duration)
    {
        CoroutineRunner.Instance.StartCoroutine(CooldownCoroutine(value => m_IndividualCooldowns[category] = value, duration));
    }

    public void StartSharedCooldown(float duration)
    {
        CoroutineRunner.Instance.StartCoroutine(CooldownCoroutine(value => AnnouncerManager.sharedCooldown = value, duration));
    }

    public void ResetCooldowns()
    {
        m_IndividualCooldowns.Clear();
    }
    #endregion


    #region Private Methods
    private IEnumerator CooldownCoroutine(Action<float> setCooldown, float initialCooldown)
    {
        if (initialCooldown <= 0)
        {
            setCooldown(0);
            yield break;
        }

        float cooldown = initialCooldown;
        float waitTime = cooldown * 3 / 4f;
        float deltaTimeTime = cooldown * 1 / 4f;
        setCooldown(cooldown);

        yield return new WaitForSeconds(waitTime);

        float timePassed = 0f;
        while (timePassed < deltaTimeTime)
        {
            timePassed += Time.deltaTime;
            cooldown -= Time.deltaTime;
            setCooldown(cooldown);
            yield return null;
        }

        setCooldown(0);
    }
    #endregion
}
