[![Build status](https://ci.appveyor.com/api/projects/status/7olahkndcs3r295p/branch/indev?svg=true)](https://ci.appveyor.com/project/MythicManiac/gameserver/branch/indev)
[![Build Status](https://travis-ci.org/LeagueSandbox/GameServer.svg?branch=indev)](https://travis-ci.org/LeagueSandbox/GameServer)
[![codecov.io](https://codecov.io/github/LeagueSandbox/GameServer/coverage.svg?branch=indev)](https://codecov.io/github/LeagueSandbox/GameServer?branch=indev)
# The League Sandbox project's game server
Project website along with more specifications can be fround from: https://leaguesandbox.github.io/  
Project chat on Discord: https://discord.gg/Bz3znAM

# Contributing

If you're interested in contributing, come find us from [Discord](https://discord.gg/0vmmZ6VAwXB05gB6) and let us know. You will need to PM a moderator to get access to development channels.

# Setup guide
* Install Microsoft Visual Studio 2015 or newer (Community Edition is fine)
* Install DotNet 4.6.1 Framework
* Install Editor Guidelines (https://visualstudiogallery.msdn.microsoft.com/da227a0b-0e31-4a11-8f6b-3a149cf2e459)
	* This is strongly encouraged to follow the 120 character limit per line guideline
* Download the 4.20 version of League client (https://mega.nz/#!hpkiQK5A!pFkZJtxCMQktJf4umplAdPC_Fukt0xgMfO7g3bGp1Io)
* Clone the git repository and run ```git submodule update --init --recursive``` to download the necessary contents package 
* Copy `GameServer/Settings/GameInfo.json.template` to `GameServer/Settings/GameInfo.json`
* Modify the file copied in the last step as required
* Build and run

# Running the client

#### Launching from command line
```
start "" "Path/To/Your/League420/RADS/solutions/lol_game_client_sln/releases/0.0.1.68/deploy/League of Legends.exe" "8394" "LoLLauncher.exe" "" "127.0.0.1 5119 17BLOhi6KZsTtldTsizvHg== 1"
```

# Project policies
* Line length should be 120 characters maximum whenever possible (use Editor Guidelines plugin for a ruler)
* Pull requests must be approved before they can be merged
* Pull requests should not be merged before the build has passed
    * If the build fails, ping the pull request creator and tell him to fix it
* Files and folders in `PascalCase`
* JSON dictionary keys in `PascalCase`
* Keep the code as simple and clear to read as possible
* Each separate feature should be developed in their own branch
* Commits should be in logical small pieces
* Pull requests should be kept as small as possible, generally one feature per pull requests
    * Instead of submitting one huge pull request with 3 features, submit each feature individually

# C# guidelines
* Function names in `PascalCase`
* Constants in `ALL_CAPS`
* Private variables in `_camelCaseWithUnderscore`
* Public properties as getters / setters in `PascalCase`
* Acronyms should be consider a single word when capitalizing
	* `configJson` instead of `configJSON`
* All public variable access should happen through getters / setters
* Regions shouldn't be used, instead split code into classes/files when needed
* Dictionaries preferred over switches and long if/else statements
* Boolean variable names should be prefixed with a question (is/can/should)
* Conditional operator should be avoided. `condition ? option1 : option2`
    * This is fine to use in some niche cases where you can't avoid using it
* String inetpolation with embedded logic should not be used
	* It's fine to use string interpolation for variable substitution

# Development flow and how to use git shell
**Using git shell is strongly encouraged**

1. Pull latest version of indev
    * `git fetch -p`
    * `git pull origin indev`
2. Checkout to a new branch
    * `git checkout -b <branch_name>`
3. Make changes, do commits
    * `git status` - List of changed files
    * `git add <filename>` - Stage file for commit
    * `git add -u` - Stage all updated files for commit
    * `git add -A` - Stage all unstaged files for commit
    * `git commit -m "<commit message>"` - Create commit
4. Push to github
    * `git push origin <branch_name>`
5. Create pull request
6. Checkout back to indev
    * `git checkout indev`
7. Repeat

# License

Currently this repository is under the [GPL-3.0](LICENSE/GPL-3.0.txt) license.  
We are in a transition phase to the [AGPL-3.0](LICENSE/AGPL-3.0.txt) license. Once all of the current contributors have consented to the license change, this license will be in effect.

You can follow who has given consent so far in the following table:

| Consent Given      | Contributor                                       |
|--------------------|---------------------------------------------------|
| :x:                | [joaquin95](https://github.com/joaquin95)         |
| :white_check_mark: | [MythicManiac](https://github.com/MythicManiac)   |
| :x:                | [horato](https://github.com/horato)               |
| :x:                | [FurkanS](https://github.com/FurkanS1821)         |
| :x:                | [Neekhaulas](https://github.com/Neekhaulas)       |
| :x:                | [Maufeat](https://github.com/Maufeat)             |
| :white_check_mark: | [Deudly](https://github.com/Deudly)               |
| :white_check_mark: | [moonshadow565](https://github.com/moonshadow565) |
| :x:                | [chutch1122](https://github.com/chutch1122)       |
| :x:                | [NitroXenon](https://github.com/NitroXenon)       |
| :white_check_mark: | [MatthewFrench](https://github.com/MatthewFrench) |
| :x:                | [piorrro33](https://github.com/piorrro33)         |
| :x:                | [TheWebs](https://github.com/TheWebs)             |
| :white_check_mark: | [xDawiss](https://github.com/xDawiss)             |
| :x:                | [Pokemonred200](https://github.com/Pokemonred200) |
| :white_check_mark: | [ChewyBomber](https://github.com/ChewyBomber)     |
| :x:                | [Fighter19](https://github.com/Fighter19)         |
| :x:                | [TornjV](https://github.com/TornjV)               |
| :x:                | [danil179](https://github.com/danil179)           |

Once everyone in this table has a :white_check_mark: icon in the consent column, the AGPL-3 license will be in effect.

### Giving consent

To give consent in changing the license:
* You must submit a **GPG signed commit** changing your icon from :x: to :white_check_mark: in the table from previous section.
	* See: https://help.github.com/articles/signing-commits-with-gpg/
	* You can also use GitHub's web interface when editing, this will sign the created commit
* The commit must come from the **same GitHub account** as the contributions belong to.
* Make sure to only modify your own line as to avoid any merge conflicts.
* Once the submission is done, you have consented to having all of your contributions in this repository so far to be licensed under the [AGPL-3.0](LICENSE/AGPL-3.0.txt) license
