<img alt="Static Badge" src="https://img.shields.io/badge/Grey-Announcer-00FFFF" width="150">

It will play a **audio** when you reach to a higher style rank!

You can add the sound by replace the .wav file in the **Audio** folder near the plugin!

## How to install it
1. [r2modman](https://thunderstore.io/c/ultrakill/p/ebkr/r2modman/)

2. [BepInEx](https://thunderstore.io/c/ultrakill/p/BepInEx/BepInExPack/) (Manual Installation)

   1. Install **BepInEx**.
   2. Download the mod.
   3. Extract all contents from the `.zip` file into the **ULTRAKILL\BepInEx\plugin** folder.
      - **Highly recommended**: Create and name a separate folder inside the `plugins` directory to keep mods organized.


### How to add the rank sound
1. Find the directory path of the audio
   - audio folder inside ULTRAKILL_DATA
      1. Right click your ULTRAKILL on steam

      2. **Manage** --> **Browse Local Files**

      3. Go to the **ULTRAKILL_DATA** folder

      4. Go to a **Audio** Folder ( if not, create a new one )

   - Legacy method ( Put the audio near the plugin, it still work )
      1. Find directory path of the plugin/.dll

      2. Find the **Audio** folder near the plugin/.dll.

2. Open the **Audio** folder.

3. Add the sound you wanted to the folder ( The file format is recommanded to using **.mp3** and **.wav** format, Not support **.flac** and **.acc** )

4. The rank sound file's name should rename to the specific format, include: **D C B A S SS SSS U**   

#### Rank Sound File Naming Convention

The rank sound files should be named in the following specific format:

<img align="right" width="300" src="https://github.com/seerge/g-helper/assets/5920850/3e119674-db8d-486b-aa65-2bf9b61f9aa6">

- `D.wav`
- `C.wav`
- `B.wav`
- `A.wav`
- `S.wav`
- `SS.wav`
- `SSS.wav`
- `U.wav`

Ensure that all rank sound files follow this naming convention for proper recognition.

If a required sound file is missing, the system will:
- **Skip playback** for that rank.
- **Not affect other rank sounds** that are correctly configured.

### How to customize the setting
There's two ways to customize the setting

1. Download and Using [PluginConfigurator](https://thunderstore.io/c/ultrakill/p/EternalsTeam/PluginConfigurator/) to edit in-game.

2. Find the **greycsont.ultrakill.GreyAnnouncer.cfg** file in the **ULTRAKILL/Beplnex/config** folder, then open and edit it manually.

### Bugs & Suggestions
if you encountered any bug, complains and suggestions, please upload it on [GreyAnnouncer issue](https://github.com/greycsont/GreyAnnouncer/issues).

#### Credit
Thanks:
- [Announcer Mod](https://www.nexusmods.com/ultrakill/mods/54)
- [PluginConfigurator doc](https://github.com/eternalUnion/UKPluginConfigurator/wiki)
- [Maxwell's puzzling demon](https://store.steampowered.com/app/2770160/)
- [Everything](https://www.voidtools.com/) Saved my mod's thumbnail and manifest by VSCODE's github extention's auto correction.

#### License
- This project is not open-sourced under the MIT License.
- The code/software is licensed under the MIT License.
- The icons and assets are not covered by the MIT License and are subject to separate licensing terms.
- The icons and assets are provided only for use within this project. **Unauthorized commercial use is prohibited**.
<br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br>

###### Future
- json to allow custom audio name instead naming stuff, but idk how do i shown it in the pluginConfigurator
###### Easter egg
The ULTRA_REVAMP update DESTROYed my favorite mod, So ......
(But the mod updated to the ULTRA_REVAMP, please check **Announcer** in the Thanks)
- [greycsont](https://space.bilibili.com/408475448)
- [快乐萨卡兹厨](https://space.bilibili.com/93667339)