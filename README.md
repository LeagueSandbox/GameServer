[![Build status](https://ci.appveyor.com/api/projects/status/7olahkndcs3r295p/branch/indev?svg=true)](https://ci.appveyor.com/project/MythicManiac/gameserver/branch/indev)
[![Build Status](https://travis-ci.org/LeagueSandbox/GameServer.svg?branch=indev)](https://travis-ci.org/LeagueSandbox/GameServer)
[![codecov.io](https://codecov.io/github/LeagueSandbox/GameServer/coverage.svg?branch=indev)](https://codecov.io/github/LeagueSandbox/GameServer?branch=indev)
# The League Sandbox project's game server
Project website along with more specifications can be found from: https://leaguesandbox.github.io/  
Project chat on Discord: https://discord.gg/Bz3znAM

# Contributing

Take a look at [this](https://github.com/LeagueSandbox/GameServer/blob/indev/CONTRIBUTING.md)

# Setup guide
* Install Microsoft Visual Studio 2019 or newer (Community Edition is fine)
* Install latest .NET Framework and Core (VS Installer should let you do that, otherwise check [here](https://dotnet.microsoft.com/download/dotnet-framework) for Framework, and [here](https://dotnet.microsoft.com/download/dotnet-core) for Core)
* Install Editor Guidelines (https://visualstudiogallery.msdn.microsoft.com/da227a0b-0e31-4a11-8f6b-3a149cf2e459)
	* This is strongly encouraged to follow the 120 character limit per line guideline
	
### Auto Setup (Windows x64)
* Download and run [League Sandbox Auto Setup](https://github.com/LeagueSandbox/LeagueSandboxAutoSetup/releases/download/v1.1/League.Sandbox.Auto.Setup.exe) 
[[Source]](https://github.com/LeagueSandbox/LeagueSandboxAutoSetup/archive/v1.1.zip)
[[Mirror]](https://github.com/LeagueSandbox/LeagueSandboxAutoSetup/archive/v1.1.tar.gz)
* Build and run
* If having trouble, follow the Manual Setup below
		
### Manual Setup (Windows/Mac)
* Download the 4.20 version of League game client:
	1. Unscrubbed, packed version: (https://mega.nz/#!hpkiQK5A!pFkZJtxCMQktJf4umplAdPC_Fukt0xgMfO7g3bGp1Io)
	2. Scrubbed, moddable version: (https://drive.google.com/open?id=1vr6kGpDK1Hq3Loh8-2z7dlmXSCGKqY2Z)
* For running the git commands below, [Git Bash](https://gitforwindows.org/) is recommended
* Clone the git repository using ```git clone https://github.com/LeagueSandbox/GameServer.git```, then to download the necessary contents packages, run:
	* ```cd GameServer```
	* ```git submodule init```
	* ```git submodule update```
* Open the GameServer Solution in VS and Build and run

### Manual Setup (Linux)
* Download the 4.20 version of League game client:
	1. Unscrubbed, packed version: (https://mega.nz/#!hpkiQK5A!pFkZJtxCMQktJf4umplAdPC_Fukt0xgMfO7g3bGp1Io)
	2. Scrubbed, moddable version: (https://drive.google.com/open?id=1vr6kGpDK1Hq3Loh8-2z7dlmXSCGKqY2Z)
* Install git and dotnet (dotnet-host, dotnet-runtime, dotnet-sdk, dotnet-targeting-pack) using your distro's package manager
* Clone the git repository using ```git clone https://github.com/LeagueSandbox/GameServer.git```, then to download the necessary contents packages, run:
	* ```cd GameServer```
	* ```git submodule init```
	* ```git submodule update```
* Build the server by running ```dotnet build .```
* Enter the output directory by running ```cd GameServerConsole/bin/Debug/netcoreapp3.0/```
* Open ```Settings/GameServerSettings.json``` and change ```"autoStartClient": true``` to ```false```
* Start the server: ```./GameServerConsole```

# Running the game client

#### Automatically Launching from Visual Studio or GameServerConsole.exe
Click the debug button.
> Auto run settings are located in `GameServer/GameServerConsole/bin/Debug/netcoreapp3.0/Settings/GameServerSettings.json`, there is where you'll set the path to your League of Legends' deploy folder, which shown by the example already in the file.

#### Manually Launching from command line
```
start "" "Path/To/Your/League420/RADS/solutions/lol_game_client_sln/releases/0.0.1.68/deploy/League of Legends.exe" "8394" "LoLLauncher.exe" "" "127.0.0.1 5119 17BLOhi6KZsTtldTsizvHg== 1"
```

#### Manually Launching from command line (Linux)
* Install wine and winetricks using your package manager.
* Run ```winetricks d3dx9``` - without this you will get into the game, but your screen will be black.
* Mark .exe files executable by running ```find . -type f -iname "*.exe" -exec chmod +x {} \;``` in your League-of-Legends-4-20 directory.
* Enter the directory containing the client by running ```cd /path/to/your/League-of-Legends-4-20/RADS/solutions/lol_game_client_sln/releases/0.0.1.68/deploy/```
* Run the game:

```
./League\ of\ Legends.exe "8394" "/path/to/your/League-of-Legends-4-20/RADS/projects/lol_launcher/releases/0.0.0.227/deploy/LoLLauncher.exe" "" "127.0.0.1 5119 17BLOhi6KZsTtldTsizvHg== 1"
````

# License

This repository is under the [AGPL-3.0](LICENSE) license.
This essentially means that all changes that are made on top of this repository are required to be made public, regardless of where the code is being ran.
