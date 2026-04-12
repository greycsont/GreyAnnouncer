## Mod description
A mod plays sound when `StyleHUD.AscendRank` and `StyleHUD.ComboStart` is called

tldr: when your style rank get higher

## Features
- Plays sound when your style rank get higher

- Hot switch between different presets/packs

- Multiple audio format support || use FFmpeg's lib to load unknown format

- 

## Format of config.json
This is an example of config.json:

you can add multiple files in single category's AudioFiles, when that category is called it will select random one between these fils

`Sry I will not switch to .ini and makes a silly broken change again`
```
{
  "RandomizeAudioOnPlay": false,
  "CategorySetting": {
    "Destruction": {
      "Enabled": true,
      "VolumeMultiplier": 1.0,
      "Cooldown": 2.0,
      "AudioFiles": [
        "Destruction.wav"
      ]
    },
    "Chaotic": {
      "Enabled": true,
      "VolumeMultiplier": 1.0,
      "Cooldown": 1.96400023,
      "AudioFiles": [
        "Chaotic.wav"
      ]
    },
    "Brutal": {
      "Enabled": true,
      "VolumeMultiplier": 1.0,
      "Cooldown": 2.0,
      "AudioFiles": [
        "Brutal.wav"
      ]
    },
    "Anarchic": {
      "Enabled": true,
      "VolumeMultiplier": 1.0,
      "Cooldown": 2.0,
      "AudioFiles": [
        "Anarchic.wav"
      ]
    },
    "Supreme": {
      "Enabled": true,
      "VolumeMultiplier": 1.0,
      "Cooldown": 2.0,
      "AudioFiles": [
        "Supreme.wav"
      ]
    },
    "SSadistic": {
      "Enabled": true,
      "VolumeMultiplier": 1.0,
      "Cooldown": 2.0,
      "AudioFiles": [
        "SSadistic.wav"
      ]
    },
    "SSShitstorm": {
      "Enabled": true,
      "VolumeMultiplier": 1.0,
      "Cooldown": 2.0,
      "AudioFiles": [
        "SSShitstorm.wav"
      ]
    },
    "ULTRAKILL": {
      "Enabled": true,
      "VolumeMultiplier": 1.0,
      "Cooldown": 2.0,
      "AudioFiles": [
        "ULTRAKILL.wav"
      ]
    }
  }
}
```

## Note
considering huge performance issue, it's best to set `Logging.Console.Enabled` to `false` in `ULTRAKILL\BepInEx\config\BepInEx.cfg`

but if you mind that, please at least remove the `Debug` in `Logging.Console.LogLevel`

## Bugs & Suggestions
if you have any throuble or suggestion with the mod, feel free to ask in 
- [Github issue](https://github.com/greycsont/GreyAnnouncer/issues)
- Email : 1106230622@qq.com
- Discord : `csont.0721`

## Complants
FUCK r2modman WHO DID THAT YOU NEED TO MAKE THE FOLDER STRUCTURE LIKE `mod's zip/plugins/modName/subfolder` TO MAKE A SUB-FOLDER? FUCK

<br><br><br><br><br><br><br><br><br><br><br><br><br><br><br>


###### Easter egg
你是谁，请支持[魔法少女的魔女审判](https://store.steampowered.com/app/3101040)