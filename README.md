You need to download place this (https://github.com/lilstav/GameServer) into [GameServer] to be able to use it.
Project chat on Discord: https://discord.gg/Bz3znAM

# Contributing
We're looking for people interested in contributing to the project.  
Currently the technologies we use include:
* C#
* Lua
* Electron
* Node.js
* Angular
* Socket.io

# Setup guide
* Install Microsoft Visual Studio 2015 (Community Edition is fine)
* Install DotNet 4.6.1 Framework
* Install NuGet package installer (https://visualstudiogallery.msdn.microsoft.com/5d345edc-2e2d-4a9c-b73b-d53956dc458d)
* Install Editor Guidelines (https://visualstudiogallery.msdn.microsoft.com/da227a0b-0e31-4a11-8f6b-3a149cf2e459)
* Download the 4.20 version of League client (https://mega.nz/#!hpkiQK5A!pFkZJtxCMQktJf4umplAdPC_Fukt0xgMfO7g3bGp1Io)
* Launch GameServer.sln and hit Run
* Creat a .bat file and add this string then run the client :
    * `start "" "Path/To/Your/League420/RADS/solutions/lol_game_client_sln/releases/0.0.1.68/deploy/League of Legends.exe" "8394" "LoLLauncher.exe" "" "127.0.0.1 5119 17BLOhi6KZsTtldTsizvHg== 1"`

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
* All public variable access should happen through getters / setters
* Regions shouldn't be used, instead split code into classes/files when needed
* Dictionaries preferred over switches and long if/else statements
* Boolean variable names should be prefixed with a question (is/can/should)
* Conditional operator should be avoided. `condition ? option1 : option2`
    * This is fine to use in some niche cases where you can't avoid using it
* Interpolated strings with embedded logic should not be used

# Development flow and how to use git shell
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
