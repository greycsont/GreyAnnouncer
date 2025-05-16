using HarmonyLib;

using UnityEngine;
using System.Collections.Generic;


    
namespace greycsont.GreyAnnouncer;

/* This patch is used to determine the changes of rankIndex
    The rankIndex is basically the pointer to another arrays, list sth.
    More information in the Announcer.cs 
    StyleHUD.cs -> RankAnnouncer.cs */

[HarmonyPatch(typeof(StyleHUD), "AscendRank")]  // For non-D ranks
public static class StyleHUDAscendRank_Patch
{
    static void Postfix(StyleHUD __instance)
    {  
        var rank = __instance.rankIndex;
        if (rank >= 0 && rank <= 7)
        {
            RankAnnouncerV2.PlayRankSound(rank);
        }
    }
}


[HarmonyPatch(typeof(StyleHUD), "ComboStart")]
public class StyleHUDComboStart_Patch
{
    static void Postfix(StyleHUD __instance)
    {
        var rank = __instance.rankIndex;
        if (rank == 0)
        {
            RankAnnouncerV2.PlayRankSound(0);
        }
    }
}


/*[HarmonyPatch(typeof(EnemyIdentifier))]
public class EnemyIdentifier_Patch
{
    public static void Zap(
		Vector3 position,
		float damage = 2f,
		List<GameObject> alreadyHitObjects = null,
		GameObject sourceWeapon = null,
		EnemyIdentifier sourceEid = null,
		Water sourceWater = null,
		bool waterOnly = false
	)
	{
		bool flag = false;
		// 检测电到的水以及玩家是否在水里
		if (sourceWater && sourceWater.isPlayerTouchingWater)
		{
			flag = true;
		}
		// 如果敌人在水里
		else if (sourceEid && sourceEid.touchingWaters.Count > 0)
		{
			// 遍历敌人所在的所有水域
			foreach (Water water in sourceEid.touchingWaters)
			{
				if (!(water == null) && water.isPlayerTouchingWater)
				{
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			// 对玩家造成50点伤害+50硬伤， 然后画线
			MonoSingleton<NewMovement>.Instance.GetHurt(50, true, 1f, false, false, 1f, false);
			LineRenderer lineRenderer = Object.Instantiate<LineRenderer>(MonoSingleton<DefaultReferenceManager>.Instance.electricLine, Vector3.Lerp(position, MonoSingleton<NewMovement>.Instance.transform.position, 0.5f), Quaternion.identity);
			lineRenderer.SetPosition(0, position);
			lineRenderer.SetPosition(1, MonoSingleton<NewMovement>.Instance.transform.position);
			Object.Instantiate<GameObject>(MonoSingleton<DefaultReferenceManager>.Instance.zapImpactParticle, MonoSingleton<NewMovement>.Instance.transform.position, Quaternion.identity);
		}

		foreach (EnemyIdentifier enemyIdentifier in MonoSingleton<EnemyTracker>.Instance.GetCurrentEnemies())
		{
			if (alreadyHitObjects == null || !alreadyHitObjects.Contains(enemyIdentifier.gameObject))
			{
				bool flag2 = false;
				if (
					!enemyIdentifier.flying
					|| ((sourceWater || sourceEid)
					&& enemyIdentifier.touchingWaters.Count != 0)
				)
				{
					if (enemyIdentifier.touchingWaters.Count > 0)
					{
						if (sourceWater != null)
						{
							flag2 = enemyIdentifier.touchingWaters.Contains(sourceWater);
						}
						if (!flag2 && sourceEid != null && sourceEid.touchingWaters.Count > 0)
						{
							foreach (Water water2 in enemyIdentifier.touchingWaters)
							{
								if (!(water2 == null) && sourceEid.touchingWaters.Contains(water2))
								{
									flag2 = true;
									break;
								}
							}
						}
						if (enemyIdentifier.flying && !flag2)
						{
							continue;
						}
					}
					Vector3 vector = enemyIdentifier.overrideCenter ? enemyIdentifier.overrideCenter.position : enemyIdentifier.transform.position;
					if ((flag2 && (!waterOnly || enemyIdentifier.lastZapped > 1f)) || (!waterOnly && (Vector3.Distance(position, vector) < 30f || (position.y > vector.y && position.y - vector.y < 60f && Vector3.Distance(position, new Vector3(vector.x, position.y, vector.z)) < 30f)) && !Physics.Raycast(position, vector - position, Vector3.Distance(position, vector), LayerMaskDefaults.Get(LMD.Environment))))
					{
						enemyIdentifier.hitter = "zap";
						enemyIdentifier.hitterAttributes.Add(HitterAttribute.Electricity);
						enemyIdentifier.DeliverDamage(enemyIdentifier.gameObject, Vector3.zero, vector, Mathf.Max((enemyIdentifier.lastZapped < 5f) ? 0.5f : 2f, damage), true, 0f, sourceWeapon, false, false);
						enemyIdentifier.lastZapped = 0f;
						LineRenderer lineRenderer2 = Object.Instantiate<LineRenderer>(MonoSingleton<DefaultReferenceManager>.Instance.electricLine, Vector3.Lerp(position, vector, 0.5f), Quaternion.identity);
						lineRenderer2.SetPosition(0, position);
						lineRenderer2.SetPosition(1, vector);
						Object.Instantiate<GameObject>(MonoSingleton<DefaultReferenceManager>.Instance.zapImpactParticle, vector, Quaternion.identity);
					}
				}
			}
		}

		if (waterOnly)
		{
			return;
		}

		foreach (Magnet magnet in MonoSingleton<ObjectTracker>.Instance.magnetList)
		{
			if (
				!(magnet == null)
				&& !alreadyHitObjects.Contains(magnet.gameObject)
				&& (!(magnet.onEnemy != null)
				|| !alreadyHitObjects.Contains(magnet.onEnemy.gameObject))
				&& Vector3.Distance(position, magnet.transform.position) < 30f
				&& !Physics.Raycast(position, magnet.transform.position - position, Vector3.Distance(position, magnet.transform.position), LayerMaskDefaults.Get(LMD.Environment))
			)
			{
				magnet.StartCoroutine(magnet.Zap(alreadyHitObjects, Mathf.Max(0.5f, damage), sourceWeapon));
				LineRenderer lineRenderer3 = Object.Instantiate<LineRenderer>(MonoSingleton<DefaultReferenceManager>.Instance.electricLine, Vector3.Lerp(position, magnet.transform.position, 0.5f), Quaternion.identity);
				lineRenderer3.SetPosition(0, position);
				lineRenderer3.SetPosition(1, magnet.transform.position);
			}
		}
		
		foreach (Zappable zappable in MonoSingleton<ObjectTracker>.Instance.zappablesList)
		{
			RaycastHit raycastHit;
			if (
				!(zappable == null)
				&& !alreadyHitObjects.Contains(zappable.gameObject)
				&& Vector3.Distance(position, zappable.transform.position) < 30f
				&& (!Physics.Raycast(position, zappable.transform.position - position, out raycastHit, Vector3.Distance(position, zappable.transform.position), LayerMaskDefaults.Get(LMD.Environment)) || raycastHit.transform.gameObject == zappable.gameObject)
			)
			{
				zappable.StartCoroutine(zappable.Zap(alreadyHitObjects, Mathf.Max(0.5f, damage), sourceWeapon));
				LineRenderer lineRenderer4 = Object.Instantiate<LineRenderer>(MonoSingleton<DefaultReferenceManager>.Instance.electricLine, Vector3.Lerp(position, zappable.transform.position, 0.5f), Quaternion.identity);
				lineRenderer4.SetPosition(0, position);
				lineRenderer4.SetPosition(1, zappable.transform.position);
			}
		}
	}
}*/

/*[HarmonyPatch(typeof(StyleHUD), "UpdateMeter")]  // For D rank only
=======
/*[HarmonyPatch(typeof(StyleHUD), "UpdateMeter")]  // For D rank only, left for review silly code, why do you want to patch a Update() function? 
>>>>>>> 145108309970536fe7b8cd4ad469d6e00f945bb7
public static class StyleHUDUpdateMeter_Patch
{
    private static readonly AccessTools.FieldRef<StyleHUD, float> currentMeterRef = AccessTools.FieldRefAccess<StyleHUD, float>("currentMeter");
    private static          bool                                  previousWasZero = true;
    static void Postfix(StyleHUD __instance)
    {
        float currentMeter    = GetCurrentMeter(__instance);
        bool currentIsNonZero = __instance.rankIndex == 0 && currentMeter > 0;

        if (previousWasZero && currentIsNonZero)
        {
            RankAnnouncer.PlaySound(0);
        }

        previousWasZero = __instance.rankIndex == 0 && currentMeter <= 0;
    }

    

    private static float GetCurrentMeter(StyleHUD instance)
    {
        return currentMeterRef(instance);
    }

    private static float GetCurrentMeter(StyleHUD instance)
    {
        return Traverse.Create(instance).Field("currentMeter").GetValue<float>();
    }
}*/    

