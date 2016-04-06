# IntWarsSharp
DotNet port of IntWars: https://github.com/MythicManiac/IntWars/:
Project chat on Discord: https://discord.gg/0vmmZ6VAwXB05gB6

# Setup guide
* Install Microsoft Visual Studio 2015 (Community Edition is fine)
* Install DotNet 4.6.1 Framework
* Install NuGet package installer (https://visualstudiogallery.msdn.microsoft.com/5d345edc-2e2d-4a9c-b73b-d53956dc458d)
* Install StyleCop (https://stylecop.codeplex.com/)
* Install Editor Guidelines (https://visualstudiogallery.msdn.microsoft.com/da227a0b-0e31-4a11-8f6b-3a149cf2e459)
* Download the 4.20 version of League client
* Clone git repository
* Copy `IntWarsSharp/lua/settings.json.template` to `IntWarsSharp/lua/settings.json`
* Modify the just copied settings file as required
* Build and run

# Project policies
* Line length should be 120 characters maximum whenever possible (use Editor Guidelines plugin for a ruler)
* No StyleCop warnings should be present in new code
* No committing to master—all changes must go through pull requests
* No self merging pull requests

# Planned data structure

```
Data
    Base
        Champions
        Items
        Etc...
    SomeOtherPackage
        Champions
        ...
Gamemodes
    Somemode
        Data
            <data that's local to this gamemode only>
        <config files determining what data the mode uses>
```

# Credits
|            |                         |                               |
|------------|-------------------------|-------------------------------|
| Intline9   | Original creator        | https://github.com/Intline9   |
| Ltsstar    | Contributor             | https://github.com/ltsstar    |
| Elyotna    | Contributor             | https://github.com/Elyotna    |
| Eddy5641   | C# port original author | https://github.com/eddy5641   |
| Chutch1122 | Contributor             | https://github.com/chutch1122 |
| Spudgy     | Contributor             | https://github.com/spudgy     |
