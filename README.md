[![Build status](https://ci.appveyor.com/api/projects/status/7olahkndcs3r295p/branch/indev?svg=true)](https://ci.appveyor.com/project/MythicManiac/gameserver/branch/indev)
[![Build Status](https://travis-ci.org/LeagueSandbox/GameServer.svg?branch=indev)](https://travis-ci.org/LeagueSandbox/GameServer)
[![codecov.io](https://codecov.io/github/LeagueSandbox/GameServer/coverage.svg?branch=indev)](https://codecov.io/github/LeagueSandbox/GameServer?branch=indev)
# The League Sandbox project's game server
Project website along with more specifications can be fround from: https://leaguesandbox.github.io/  
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

Currently this repository is under the [GPL-3.0](LICENSE/GPL-3.0.txt) license.  
We are in a transition phase to the [AGPL-3.0](LICENSE/AGPL-3.0.txt) license. Once all of the current contributors have consented to the license change, this license will be in effect.

You can follow who has given consent so far in the following table:

| Consent Given      | Contributor                                       |
|--------------------|---------------------------------------------------|
| :white_check_mark: | [joaquin95](https://github.com/joaquin95)         |
| :white_check_mark: | [MythicManiac](https://github.com/MythicManiac)   |
| :white_check_mark: | [horato](https://github.com/horato)               |
| :white_check_mark: | [FurkanS](https://github.com/FurkanS1821)         |
| :white_check_mark: | [Neekhaulas](https://github.com/Neekhaulas)       |
| :white_check_mark: | [Maufeat](https://github.com/Maufeat)             |
| :white_check_mark: | [Deudly](https://github.com/Deudly)               |
| :white_check_mark: | [moonshadow565](https://github.com/moonshadow565) |
| :white_check_mark: | [chutch1122](https://github.com/chutch1122)       |
| :x:                | [NitroXenon](https://github.com/NitroXenon)       |
| :white_check_mark: | [MatthewFrench](https://github.com/MatthewFrench) |
| :white_check_mark: | [piorrro33](https://github.com/piorrro33)         |
| :white_check_mark: | [TheWebs](https://github.com/TheWebs)             |
| :white_check_mark: | [xDawiss](https://github.com/xDawiss)             |
| :white_check_mark: | [Pokemonred200](https://github.com/Pokemonred200) |
| :white_check_mark: | [ChewyBomber](https://github.com/ChewyBomber)     |
| :white_check_mark: | [Fighter19](https://github.com/Fighter19)         |
| :x:                | [TornjV](https://github.com/TornjV)               |
| :white_check_mark: | [danil179](https://github.com/danil179)           |

Once everyone in this table has a :white_check_mark: icon in the consent column, the AGPL-3 license will be in effect.

### Giving consent

To give consent in changing the license:
* You must submit a **GPG signed commit** changing your icon from :x: to :white_check_mark: in the table from previous section.
	* See: https://help.github.com/articles/signing-commits-with-gpg/
	* You can also use GitHub's web interface when editing, this will sign the created commit
* The commit must come from the **same GitHub account** as the contributions belong to.
* Make sure to only modify your own line as to avoid any merge conflicts.
* Once the submission is done, you have consented to having all of your contributions in this repository so far to be licensed under the [AGPL-3.0](LICENSE/AGPL-3.0.txt) license
