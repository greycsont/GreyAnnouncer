
#### Mod description
It will plays a **audio** when you reach to a higher style rank!


#### How to install it
1. [r2modman](https://thunderstore.io/c/ultrakill/p/ebkr/r2modman/)

2. [BepInEx](https://thunderstore.io/c/ultrakill/p/BepInEx/BepInExPack/) (Manual Installation)

   1. Install **BepInEx**.
   2. Download the mod.
   3. Extract all contents from the `.zip` file into the **ULTRAKILL\BepInEx\plugin** folder.


#### How to add the rank sound
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

3. The rank sound file's name should rename to the specific format, include: **D C B A S SS SSS U**  

#### Rank Sound File Naming

You can customize the audio name in the **greyannouncer.json** which is **near the plugin**. the default audio name given in the below:

<img align="right" width="180" src="https://github.com/greycsont/GreyAnnouncer/raw/main/docs/image/customAudioName.png">

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


#### Supported and Unsupported Audio Formats

| Format      | Supported |                 
|-------------|-----------|
| `.mp3`      | ✅        |       
| `.wav`      | ✅        |        
| `.flac`     | ❌        |             
| `.aac`      | ❌        |           
| `.ogg`      | ?        |                  
| `.aiff/.aif`| ?        |                 

#### How to customize the setting


- Download and Using [PluginConfigurator](https://thunderstore.io/c/ultrakill/p/EternalsTeam/PluginConfigurator/) to edit in-game.

#### Bugs & Suggestions
if you encountered any bug, complains and suggestions, please upload it on [GreyAnnouncer issue](https://github.com/greycsont/GreyAnnouncer/issues).

#### Credit
Thanks:
- [Announcer Mod](https://www.nexusmods.com/ultrakill/mods/54)
- [PluginConfigurator doc](https://github.com/eternalUnion/UKPluginConfigurator/wiki)
- [Maxwell's puzzling demon](https://store.steampowered.com/app/2770160/)
- [Everything](https://www.voidtools.com/) Saved my mod's thumbnail and manifest ****** up by vscode's github extention's auto correction.

#### License
- This project is not open-sourced under the MIT License.
- The code/software is licensed under the MIT License.
- The icons and assets are not covered by the MIT License and are subject to separate licensing terms.
- The icons and assets are provided only for use within this project. **Unauthorized commercial use is prohibited**.
<br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br>


###### Easter egg
The ULTRA_REVAMP update DESTROYed my favorite mod, So ......
(But the mod updated to the ULTRA_REVAMP, please check **Announcer** in the Thanks)
- [greycsont](https://space.bilibili.com/408475448)
- [快乐萨卡兹厨](https://space.bilibili.com/93667339)