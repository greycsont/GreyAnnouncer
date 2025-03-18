### v0.1.5
- Fixed the bug that will crash the mod if without all the audio required.

- Sorry for the 94 person who downloaded the mod.

### v0.1.6
- Discarded the config.ini reader, instead using the BepInEx's official config.bind() method. ( I've always seen the BepInEx as BeplnEx (lowercase L)). I used to using config.ini as the configuration file because I've seen this in a other mod called NGS2Black create by Fiend_Busa. But BepInEx have a official API, so why not?

- Added MIT Lisence.

### v1.0.0
- supports PluginConfigurator 
- seperate the shared rank cooldown and individual rank cooldown
- using InstanceConfig to store configEntry
- using delegate to set cooldowncorotine
- sets of decouping code
- if you want to add collab to other mods and wants to make sure mod is running normally when other mods are not loaded, using reflection to load collab module!

