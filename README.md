[![Build status](https://ci.appveyor.com/api/projects/status/7olahkndcs3r295p/branch/master?svg=true)](https://ci.appveyor.com/project/MythicManiac/gameserver/branch/master)
[![codecov.io](https://codecov.io/github/LeagueSandbox/GameServer/coverage.svg?branch=master)](https://codecov.io/github/LeagueSandbox/GameServer?branch=master)
# The League Sandbox project's game server
Project website along with more specifications can be fround from: https://leaguesandbox.github.io/  
Project chat on Discord: https://discord.gg/0vmmZ6VAwXB05gB6  
Project board on Waffle: https://waffle.io/LeagueSandbox/leaguesandbox.github.io

# Contributing
We're looking for people interested in contributing to the project.  
Currently the technologies we use include:
* C#
* Lua
* Electron
* Node.js
* Angular
* Socket.io

For more detailed project specifications head over to https://leaguesandbox.github.io/  
If you're interested in contributing, come find us from [Discord](https://discord.gg/0vmmZ6VAwXB05gB6) and let us know

# Setup guide
* Install Microsoft Visual Studio 2015 (Community Edition is fine)
* Install DotNet 4.6.1 Framework
* Install NuGet package installer (https://visualstudiogallery.msdn.microsoft.com/5d345edc-2e2d-4a9c-b73b-d53956dc458d)
* Install Editor Guidelines (https://visualstudiogallery.msdn.microsoft.com/da227a0b-0e31-4a11-8f6b-3a149cf2e459)
* Download the 4.20 version of League client (https://mega.nz/#!hpkiQK5A!pFkZJtxCMQktJf4umplAdPC_Fukt0xgMfO7g3bGp1Io)
* Clone the git repository
* Copy `GameServer/Settings/Settings.json.template` to `GameServer/Settings/Settings.json`
* Copy `GameServer/Settings/GameInfo.json.template` to `GameServer/Settings/GameInfo.json`
* Modify the just copied settings files as required
* Build and run

# Running the client
Currently there are two options for launching the client.

#### Launching from command line
```
start "" "Path/To/Your/League420/RADS/solutions/lol_game_client_sln/releases/0.0.1.68/deploy/League of Legends.exe" "8394" "LoLLauncher.exe" "" "127.0.0.1 5119 17BLOhi6KZsTtldTsizvHg== 1"
```

#### Using a launcher created by [TheWebs](https://github.com/TheWebs)
1. Clone the launcher's repository from https://github.com/TheWebs/IWLauncher
2. Build it
3. Copy the built launcher and it's dependencies to the game server's build folder `GameServer/bin/Debug/`
4. Run it

# Project policies
* Line length should be 120 characters maximum whenever possible (use Editor Guidelines plugin for a ruler)
* No StyleCop warnings should be present in new code
* No pushing/committing to master—all changes must go through pull requests
* Don't merge your own pull requests—get someone else to review/merge it
* Pull requests should not be merged before the build has passed
    * If the build fails, ping the pull request creator and tell him to fix it
* Files and folders in `PascalCase`
* JSON dictionary keys in `PascalCase`

# C# guidelines
* Function names in `PascalCase`
* Constants in `ALL_CAPS`
* Private variables in `_camelCaseWithUnderscore`
* Public properties as getters / setters in `PascalCase`
* All public variable access should happen through getters / setters
* Regions shouldn't be used, instead split code into classes/files when needed
* Dictionaries preferred over switches and long if/else statements
* Boolean variable names should be prefixed with a question (is/can/should)
* Conditional operator should be avoided. `condition ? option1 : option2`
    * This is fine to use in some niche cases where you can't avoid using it

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
