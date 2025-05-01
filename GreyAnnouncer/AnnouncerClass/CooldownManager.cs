using System;
using System.Collections;
using UnityEngine;

namespace greycsont.GreyAnnouncer;

public class CooldownManager
{
    private float[] m_individualCooldowns;

    #region Public API
    public CooldownManager(int individualCount)
    {
        m_individualCooldowns = new float[individualCount];
    }

    public bool IsIndividualCooldownActive(int index)
    {
        return m_individualCooldowns[index] > 0f;
    }

    public bool IsSharedCooldownActive()
    {
        return AnnouncerManager.sharedCooldown > 0f;
    }

    public void StartIndividualCooldown(int index, float duration)
    {
        CoroutineRunner.Instance.StartCoroutine(CooldownCoroutine(value => m_individualCooldowns[index] = value, duration));
    }

    public void StartSharedCooldown(float duration)
    {
        CoroutineRunner.Instance.StartCoroutine(CooldownCoroutine(value => AnnouncerManager.sharedCooldown = value, duration));
    }

    public void ResetCooldowns()
    {
        Array.Clear(m_individualCooldowns, 0, m_individualCooldowns.Length);
    }
    #endregion

    
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
}
