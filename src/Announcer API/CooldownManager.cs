using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GreyAnnouncer.AnnouncerAPI;

public class CooldownManager : ICooldownManager
{
    private Dictionary<string, float> _individualCooldowns = new Dictionary<string, float>();

    public CooldownManager(string[] audioCategories)
    {
        foreach (var category in audioCategories)
        {
            _individualCooldowns[category] = 0f;
        }
    }

    public bool IsIndividualCooldownActive(string category)
    {
        return _individualCooldowns[category] > 0f;
    }

    public bool IsSharedCooldownActive()
    {
        return AnnouncerManager.sharedCooldown > 0f;
    }

    public void StartCooldowns(string category, float duration)
    {
        StartIndividualCooldown(category, duration);
        StartSharedCooldown(InstanceConfig.sharedPlayCooldown.Value);
    }

    public void ResetCooldowns()
    {
        var keys = _individualCooldowns.Keys.ToList();
        foreach (var key in keys)
        {
            _individualCooldowns[key] = 0f;
        }
    }

    private void StartIndividualCooldown(string category, float duration)
    {
        CoroutineRunner.Instance.StartCoroutine(CooldownCoroutine(value => _individualCooldowns[category] = value, duration));
    }

    private void StartSharedCooldown(float duration)
    {
        CoroutineRunner.Instance.StartCoroutine(CooldownCoroutine(value => AnnouncerManager.sharedCooldown = value, duration));
    }




    private IEnumerator CooldownCoroutine(Action<float> setCooldown, float initialCooldown)
    {
        if (initialCooldown <= 0)
        {
            setCooldown(0);
            yield break;
        }

        float cooldown      = initialCooldown;
        float waitTime      = cooldown * 3 / 4f;
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
