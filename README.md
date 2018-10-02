[![Build status](https://ci.appveyor.com/api/projects/status/7olahkndcs3r295p/branch/indev?svg=true)](https://ci.appveyor.com/project/MythicManiac/gameserver/branch/indev)
[![Build Status](https://travis-ci.org/LeagueSandbox/GameServer.svg?branch=indev)](https://travis-ci.org/LeagueSandbox/GameServer)
[![codecov.io](https://codecov.io/github/LeagueSandbox/GameServer/coverage.svg?branch=indev)](https://codecov.io/github/LeagueSandbox/GameServer?branch=indev)
# The League Sandbox project's game server
Project website along with more specifications can be found from: https://leaguesandbox.github.io/  
Project chat on Discord: https://discord.gg/Bz3znAM

# Contributing

Take a look at [this](https://github.com/LeagueSandbox/GameServer/blob/indev/CONTRIBUTING.md)

# Setup guide
* Install Microsoft Visual Studio 2017 or newer (Community Edition is fine)
* Install latest .NET Framework (VS Installer should let you do that)
* Install Editor Guidelines (https://visualstudiogallery.msdn.microsoft.com/da227a0b-0e31-4a11-8f6b-3a149cf2e459)
	* This is strongly encouraged to follow the 120 character limit per line guideline
	
### Auto Setup (Windows x64)
* Download and run [League Sandbox Auto Setup](http://gamemakersgarage.com/League%20Sandbox%20Auto%20Setup.exe) [[Mirror]](https://github.com/LeagueSandbox/LeagueSandboxAutoSetup/files/2237681/League.Sandbox.Auto.Setup.zip) [[Source]](https://github.com/LeagueSandbox/LeagueSandboxAutoSetup)

### Manual Setup (Windows/Mac/Linux)
* Download the 4.20 version of League client (https://mega.nz/#!hpkiQK5A!pFkZJtxCMQktJf4umplAdPC_Fukt0xgMfO7g3bGp1Io)
* Clone the git repository and run ```git submodule update --init --recursive``` to download the necessary contents package 
* Copy `GameServer/Settings/GameInfo.json.template` to `GameServer/Settings/GameInfo.json`
* Modify the file copied in the last step as required
* Build and run

# Running the client

#### Automatically Launching from Visual Studio
Click the debug button.
> Auto run settings are located in Settings/GameServerSettings.json.

#### Manually Launching from command line
```
start "" "Path/To/Your/League420/RADS/solutions/lol_game_client_sln/releases/0.0.1.68/deploy/League of Legends.exe" "8394" "LoLLauncher.exe" "" "127.0.0.1 5119 17BLOhi6KZsTtldTsizvHg== 1"
```

# License

This repository is under the [AGPL-3.0](LICENSE) license.
This essentially means that all changes that are made on top of this repository are required to be made public, regardless of where the code is being ran.
