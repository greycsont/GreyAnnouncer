### What does it do?
It will play a sound when you reach to a higher style rank!

You can add the sound by replace the .wav file in the **Audio** folder near the plugin.

And you can set the cooldown timer in the **greycsont.ultrakill.GreyAnnouncer.cfg** in the **ULTRAKILL/BepInex/config** folder.

### How to install it
1.Install BepInEx.

2.Get to the directory to the plugin folder in BepInEx.

3.Drag everything in the .zip file to the folder ( highly recommanded to create a individual folder in plugin folder to put mod for easy management ).

### How to add the rank sound
1.Find the **audio** folder near the plugin.

2.Open the **audio** folder.

3.Add the sound you wanted to the rank sound file format only accept the .wav file.

4.The rank sound file's name should rename to the specific format, include: D C B A S SS SSS U

Similar to this:

>D.wav

>C.wav

>B.wav

>A.wav

>S.wav

>SS.wav

>SSS.wav

>U.wav

It is fine to not put all the rank sound in the folder, it will automatically skip the rank without the file.

### How to customize the setting
Currently there's only one settings to configure

1.Find the .cfg file in the ULTRAKILL/BepInEx/config folder.

2.Change the setting you needed.

3.Save it the start the game.

### Silly message:
<details>
<summary>Click to expand</summary>
Because the default rank is D and if we only patch the 

```csharp
AscendRank() // In the StyleHUD class in ULTRAKILL
```

 It can't announce the D-rank so I create a dogshit and patching it to a function running in every single frame to check it

```csharp
[HarmonyPatch(typeof(StyleHUD), "UpdateMeter")]  // For D rank only
    public static class StyleHUDUpdateMeterPatch
    {
        private static bool previousWasZero = true;

        static void Postfix(StyleHUD __instance)
        {
            float currentMeter = GetCurrentMeter(__instance);
            bool currentIsNonZero = __instance.rankIndex == 0 && currentMeter > 0;

            if (previousWasZero && currentIsNonZero)
            {
                Announcer.PlaySound(0);
            }

            previousWasZero = __instance.rankIndex == 0 && currentMeter <= 0;
        }

        private static float GetCurrentMeter(StyleHUD instance)
        {
            return Traverse.Create(instance).Field("currentMeter").GetValue<float>();
        }
    }
```
The style meter can be a negative number, the first frame is 0, then it's -0.16 somewhat when there's no style hud display ( main menu, respawn etc. ), that's why it's

```csharp
bool currentIsNonZero = __instance.rankIndex == 0 && currentMeter > 0
```

instead of

```csharp
bool currentIsNonZero = __instance.rankIndex == 0 && currentMeter != 0
```
</details>

