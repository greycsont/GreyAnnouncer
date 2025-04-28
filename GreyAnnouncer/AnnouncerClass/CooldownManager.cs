using System;
using System.Collections;
using UnityEngine;

namespace greycsont.GreyAnnouncer;

public class CooldownManager
{
    private float[] individualCooldowns;
    private float sharedCooldown;

    public CooldownManager(int individualCount)
    {
        individualCooldowns = new float[individualCount];
        sharedCooldown = 0f;
    }

    public bool IsIndividualCooldownActive(int index)
    {
        return individualCooldowns[index] > 0f;
    }

    public bool IsSharedCooldownActive()
    {
        return sharedCooldown > 0f;
    }

    public void StartIndividualCooldown(int index, float duration)
    {
        CoroutineRunner.Instance.StartCoroutine(CooldownCoroutine(value => individualCooldowns[index] = value, duration));
    }

    public void StartSharedCooldown(float duration)
    {
        CoroutineRunner.Instance.StartCoroutine(CooldownCoroutine(value => sharedCooldown = value, duration));
    }

    public void ResetCooldowns()
    {
        sharedCooldown = 0f;
        Array.Clear(individualCooldowns, 0, individualCooldowns.Length);
    }

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
