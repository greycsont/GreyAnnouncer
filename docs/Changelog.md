### Known issue/TODO
- folder adjustment (.json goes to stm setting folder, ffmpeg goes to like lib\ffmpeg folder etc.)
- .NET upgrade (fuck you unity)

### v1.2.0
- Added Pitch setting in .json
- Added VolumeMultiplier in .json
- Added FFmpeg support (require libs and put them in ./libs/ffmpeg folder)
- *Fixed modify .json through via editor will not synchronous the new setting to PluginConfigurator's interface, and cause setting overwrite*.

### v1.1.1
- Removed PluginConfigurator Hard Dependencies.
- Fixed typo in Bepinex's config, may have trouble but it should be fix ASAP.

### v1.1.0
- Fixed when you quit the mission in the water, the muffle effect still exists when playing audio.
- Changed the mod's icon, the old icon directly uses the Arknights's mini greythroat asset.
- Some changes of license (fixed/added the Lib\ folder that does not include in License, This folder includes assembly-sharp of ULTRAKILL, PluginConfigurator and some unity assets).

- Added parallel audio playing option for future idea.
- Added Load-then-Play loading options for RAM performance.
- Added loading log in PluginConfigurator to see the loading status
- Added Audio randomization
- Added Custom Audio directory
- Added multiple audio in a single category

- switch to using BepInex's config to customize audio loading path (Default path is the legacy method, you can edit it easily via PluginConfigurator).
- RECONSTRUCT the code architecture.
- removed patch to update() in StyleHUD.cs

### v1.0.1
- Added LowPassFilter(Muffle) when under water (Thanks Clover for DM in discord).
- Added more option (Volume of all rank etc.).
- Added support to .mp3, .ogg, .aiff/.aif
- Moved audio folder to ULTRAKILL\ULTRAKILL_DATA audio, But it still will seach the legacy folder (Thanks mightbeusual for [Github_issue](https://github.com/greycsont/GreyAnnouncer/issues/1)).
- Altered some validation.
- Encapsulated and Decoupled some code.
- Reversed the structure of Changelog.md.

### v1.0.0
- supports PluginConfigurator.
- seperate the shared rank cooldown and individual rank cooldown.
- using InstanceConfig to store configEntry.
- using delegate to set cooldowncorotine.
- sets of decouping code.
- if you want to add collab to other mods and wants to make sure mod is running normally when other mods are not loaded, using reflection to load collab module!

### v0.1.6
- Discarded the config.ini reader, instead using the BepInEx's official config.bind() method. ( I've always seen the BepInEx as BeplnEx (lowercase L)). I used to using config.ini as the configuration file because I've seen this in a other mod called NGS2Black create by Fiend_Busa. But BepInEx have a official API, so why not?

- Added MIT Lisence.

### v0.1.5
- Fixed the bug that will crash the mod if without all the audio required.

- Sorry for the 94 person who downloaded the mod.






