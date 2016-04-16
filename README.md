[![Build status](https://ci.appveyor.com/api/projects/status/7olahkndcs3r295p?svg=true)](https://ci.appveyor.com/project/MythicManiac/gameserver)
# LeagueSandbox Game Server
DotNet port and continuation of IntWars

Active fork of original IntWars: https://github.com/MythicManiac/IntWars/  
Project chat on Discord: https://discord.gg/0vmmZ6VAwXB05gB6

# Setup guide
* Install Microsoft Visual Studio 2015 (Community Edition is fine)
* Install DotNet 4.6.1 Framework
* Install NuGet package installer (https://visualstudiogallery.msdn.microsoft.com/5d345edc-2e2d-4a9c-b73b-d53956dc458d)
* Install StyleCop (https://stylecop.codeplex.com/)
* Install Editor Guidelines (https://visualstudiogallery.msdn.microsoft.com/da227a0b-0e31-4a11-8f6b-3a149cf2e459)
* Download the 4.20 version of League client (https://mega.nz/#!pFRVxBJQ!AMbsJnS9kqhvQ-tfP8QxoBikbrjlGQ4MdzNYGo0fIKM)
* Clone the git repository
* Copy `GameServer/Settings/Settings.json.template` to `GameServer/Settings/Settings.json`
* Copy `GameServer/Settings/GameInfo.json.template` to `GameServer/Settings/GameInfo.json`
* Modify the just copied settings files as required
* Build and run

# Project policies
* Line length should be 120 characters maximum whenever possible (use Editor Guidelines plugin for a ruler)
* No StyleCop warnings should be present in new code
* No pushing/committing to master—all changes must go through pull requests
* Don't merge your own pull requests—get someone else to review/merge it
* Pull requests should not be merged before the build has passed
    * If the build fails, ping the pull request creator and tell him to fix it
* Files and folders in `CamelCase`
* JSON dictionary keys in `CamelCase`

# C# guidelines
* Function names in `CamelCase`
* Constants in `ALL_CAPS`
* Private variables in `_pascalCaseWithUnderscore`
* Public properties as getters / setters in `CamelCase`
* All public variable access should happen through getters / setters

# Development flow
1. Pull latest version of master
    * `git fetch -p`
    * `git pull origin master`
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
6. Checkout back to master
    * `git checkout master`
7. Repeat

# Content data structure
```
Data
    LeagueSandbox-Default // A content package by "LeagueSandbox" called "Default"
        Champions
        Items
        Buffs
        ...
    SomeAuthor-SomePackage // A content package by "SomeAuthor" called "SomePackage"
        Champions
        ...
GameMode
    LeagueSandbox-Default // Game mode by "LeagueSandbox" called "Default"
        Data // Data that's local to this gamemode only
            Champions
            Items
            Buffs
            ...
        Scripts // Game mode related logic
            SomeScript.lua
            SomeOtherScript.lua
            ...
        GameMode.json // Game mode configuration file
```
* Two kinds of packages
    * Data packages
        * Contain data and logic for things such as champions, items, buffs, etc...
    * Game mode packages
        * Contain configuration and logic for the gamemode
        * Configuration that determines what data this mode depends on
* Data should be stored in JSON files
	* So for example, damage, range, health, etc...
* Logic should be stored in lua files
	* Lua is capable of using the data stored in the mentioned JSON files
* These two should be always separate, so no mixing data into lua
	* All predetermined values in JSON
* Packages are to be named using a <authorname>-<packagename> format
    * This to avoid collisions with similarly named packages
* Content will be identified in a <Author>-<Package>-<Type>-<Name> format, thus why namespacing is required
    
# Planned overall configuration/infrastructure
5 different components:
* League of Legends 4.20 Client
    * By Riot
    * Will be modified only by the client patcher
* Content serializer
    * Patches custom content into the game client's files
    * Serializes custom content into a format the game can understand
    * Deserializes the game's content into our custom content format
    * Builds single file packages out of custom content packages
* Lobby client
    * Client that can connect to lobby servers
    * Game mode selection
    * User settings (champion, summoner spells, etc)
    * Game mode settings (if a gamemode has configurable settings, say, kill limit for a deathmatch victory)
        * Host only
    * Receives custom content from the lobby server before the game is started
* Lobby server
    * Lobby clients can create lobbies
        * Lobby creator gets host privileges
    * Lobby clients can connect to existing lobbies
    * Sends lobby clients the gamemode specific custom content
    * Takes care of launching the game server when the game is started
* Game server
    * Receives game settings from the lobby server
    * Runs the game according to the received settings

# LeagueSandbox Credits
|              |                         |                                 |
|--------------|-------------------------|---------------------------------|
| Intline9     | Original creator        | https://github.com/Intline9     |
| Ltsstar      | Contributor             | https://github.com/ltsstar      |
| Elyotna      | Contributor             | https://github.com/Elyotna      |
| Horato       | C# port original author | https://github.com/horato       |
| Eddy5641     | Contributor             | https://github.com/eddy5641     |
| Chutch1122   | Contributor             | https://github.com/chutch1122   |
| Spudgy       | Contributor             | https://github.com/spudgy       |
| Mythic       | Current lead developer  | https://github.com/MythicManiac |
| Coquicox     | Contributor             | https://github.com/coquicox     |
| NitroXenon   | Contributor             | https://github.com/NitroXenon   |
| Fighter19    | Contributor             | https://github.com/Fighter19    |
| Molenzwiebel | Contributor             | https://github.com/molenzwiebel |
