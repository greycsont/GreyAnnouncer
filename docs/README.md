## Grey Announcer
A mod plays sound when `StyleHUD.AscendRank` and `StyleHUD.ComboStart` is called

tl;dr: when your style rank get higher

## Features
- Plays sound when your style rank get higher

- Hot switch between different packs

- Multiple audio format support & Use FFmpeg's lib to load unknown format

- Setting on Volume/SpatialBlend/Load or Play strategies

## How to Config
Go to PluginConfigurator

## Format of config.json
This is an example of config.json:

you can add multiple files in single category's AudioFiles, when that category is called it will select random one between these files

when you turn on the audio randomization, when a category is called it will select all **valid** category's audio file and choose one of them

e.g. ExcludeFromRandom = false

Last, please ensure the extension of audio file are uses the right audio extension
e.g. `.mp3`, `.wav`

`Sry I will not switch to .ini and makes a silly broken change again`
```json
{
  "RandomizeAudioOnPlay": false,
  "CategorySetting": {
    "Destruction": {
      "Enabled": true,
      "ExcludeFromRandom": false,
      "VolumeMultiplier": 1.0,
      "Cooldown": 2.0,
      "AudioFiles": [
        "Destruction.wav"
      ]
    },
    "Chaotic": {
      "Enabled": true,
      "ExcludeFromRandom": false,
      "VolumeMultiplier": 1.0,
      "Cooldown": 1.96400023,
      "AudioFiles": [
        "Chaotic.wav"
      ]
    },
    "Brutal": {
      "Enabled": true,
      "ExcludeFromRandom": false,
      "VolumeMultiplier": 1.0,
      "Cooldown": 2.0,
      "AudioFiles": [
        "Brutal.wav"
      ]
    },
    "Anarchic": {
      "Enabled": true,
      "ExcludeFromRandom": false,
      "VolumeMultiplier": 1.0,
      "Cooldown": 2.0,
      "AudioFiles": [
        "Anarchic.wav"
      ]
    },
    "Supreme": {
      "Enabled": true,
      "ExcludeFromRandom": false,
      "VolumeMultiplier": 1.0,
      "Cooldown": 2.0,
      "AudioFiles": [
        "Supreme.wav"
      ]
    },
    "SSadistic": {
      "Enabled": true,
      "ExcludeFromRandom": false,
      "VolumeMultiplier": 1.0,
      "Cooldown": 2.02559948,
      "AudioFiles": [
        "SSadistic.wav"
      ]
    },
    "SSShitstorm": {
      "Enabled": true,
      "ExcludeFromRandom": false,
      "VolumeMultiplier": 1.0,
      "Cooldown": 2.0,
      "AudioFiles": [
        "SSShitstorm.wav"
      ]
    },
    "ULTRAKILL": {
      "Enabled": true,
      "ExcludeFromRandom": false,
      "VolumeMultiplier": 1.0,
      "Cooldown": 2.0,
      "AudioFiles": [
        "ULTRAKILL.wav"
      ]
    }
  }
}
```
## Default Packs
There's two default pack in this mod: greythroat and irene

one is used to show the default action of config.json, second one is used to show the randomization work on announcer 

## Audio Credits
GreyThroat's Voice Lines (from Arknights):
- Original Author: Hypergryph (鹰角网络) 
- Source Link: https://prts.wiki/w/灰喉

Irene's Voice Lines (from Arknights):
- Original Author: Hypergryph (鹰角网络) 
- Source Link: https://prts.wiki/w/%E8%89%BE%E4%B8%BD%E5%A6%AE

`This mod is a non-commercial project, audio is only for learning use, the copyright of audio is owned by original author - Hypergryph (上海鹰角网络科技有限公司)`

`Some of the audio are renamed but it's for easy to understanding`

## Note
considering huge performance issue, it's best to set `Logging.Console.Enabled` to `false` in `ULTRAKILL\BepInEx\config\BepInEx.cfg` even if you don't use this mod

but if you mind that, please at least remove the `Debug` in `Logging.Console.LogLevel`

## Bugs & Suggestions
if you have any throuble or suggestion with the mod, feel free to ask in 
- [Github issue](https://github.com/greycsont/GreyAnnouncer/issues)
- Email : 1106230622@qq.com
- Discord : `csont.0721`

## Complants
Q: guess which f mod manager need to have a folder structure like this to make a sub folder:

whateverfuckisthis.zip/plugins/modName/subfolder

<br><br><br><br><br><br><br><br><br><br><br><br><br><br><br>


###### Easter egg
你是谁，请支持[茜塔和世界线悖论](https://store.steampowered.com/app/3219580)