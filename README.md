# BUTTLYSS

A [BepInEx](https://github.com/BepInEx/BepInEx) mod for [ATLYSS](http://atlyssgame.com) that adds [buttplug.io](https://buttplug.io/) support

## Features
* Connects game events inside ATLYSS to haptic device vibration through buttplug.io
* Vibrates in response to movement, combat, chat, and various other events
* Vibration and mod behaviour are configurable live through local settings file and chat console commands

[Installation](#installation)\
[Use](#use)\
[Configuration](#configuration)\
&nbsp;&nbsp;- [Chat Commands](#chat-commands)\
&nbsp;&nbsp;- [Default Values](#default-values)


## Installation
1. [Install the latest version of BepInEx 5](https://docs.bepinex.dev/articles/user_guide/installation/index.html)
2. Run game with BepInEx installed, and exit game
3. Download the latest version of this mod from the [release page](https://github.com/mintchipleaf/BUTTLYSS/releases/latest)
3. Go to the games main directory (where the ATLYSS.exe can be found. In steam, right click on *ATLYSS > Manage > Browse Local Files*)
4. Go to the ATLYSS BepInEx plugins folder (**ATLYSS/BepInEx/plugins/**)
5. Put the .DLL files from the release page into the BepInEx plugins folder
6. Install [Intiface Central](https://intiface.com/central/) from buttplug.io.

## Use
1. Run Intiface Central, connect your device to it (not to your computer's bluetooth,) and start the server (click on the large "Play" button)
2. Run ATLYSS
3. The Intiface server should now have the status *"ATLYSS Connected"*
4. Play the game and enjoy! :3 📳

## Configuration
BUTTLYSS can be configured by modifying the local config file (found at **ATLYSS/ATLYSS_DATA/buttlyssSettings.json**) to change the speed and duration of various vibration parameters.\
Its behaviour can also be modified live through various chat console commands.\
To restore BUTTLYSS to default settings, delete the local config file and either run the game again or use the `reload` command.

### Chat Commands
| Key                   | Description                                                                               |
|-----------------------|-------------------------------------------------------------------------------------------|
| `/buttlyss`           | List BUTTLYSS status and available commands                                               |
| `/buttlyss stop`      | Immediately stop all vibrations until the `start` command is used or game is restarted    |
| `/buttlyss start`     | Start vibrations again after using `stop` command                                         |
| `/buttlyss reload`    | Overwrites current preferences with those in the local BUTTLYSS settings file             |
| `/buttlyss reconnect` | Attempt to reconnect to the Intiface server URL specified in BUTLYSS settings             |

### Default Values
| Key                  | Type     | Description                                                             | Default                |
|----------------------|----------|-------------------------------------------------------------------------|------------------------|
| `IntifaceServer`     | `string` | The URL of the Intiface Server                                          | `ws://localhost:12345` |
| `MaxVibeDuration`    | `float`  | Max length of time for individual vibrations. (in seconds)              | `0.2`                  |
| `StrengthMultiplier` | `float`  | Multiplier applied to all vibrations.                                   | `0.8`                  |
| `TapSpeed`           | `float`  | Speed for small, subtle tap vibrations.                                 | `0.2`                  |
| `MinSpeed`           | `float`  | Lowest possible vibration speed.                                        | `0.05`                 |
| `BaseSpeed`          | `float`  | Speed to vibrate when idle, (no vibration events occuring.)             | `0.0`                  |

### Requirements
* [BepInEx 5](https://github.com/BepInEx/BepInEx)
* [Intiface Central](https://github.com/intiface/intiface-central)