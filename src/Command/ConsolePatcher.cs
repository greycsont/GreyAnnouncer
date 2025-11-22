using GameConsole;
using GreyAnnouncer.Commands;
using HarmonyLib;

namespace GreyAnnouncer.Commands;

[HarmonyPatch(typeof(Console))]
public class ConsolePatcher
{
    [HarmonyPrefix]
    [HarmonyPatch("Awake")]
    public static void AddConsoleCommands(Console __instance)
    {
        __instance.RegisterCommand(new GreyAnnouncerCommand(__instance));
    }
}