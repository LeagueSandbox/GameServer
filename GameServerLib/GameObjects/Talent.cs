﻿using System;
using GameServerCore.Scripting.CSharp;
using static LeagueSandbox.GameServer.Content.TalentContentCollection;

namespace LeagueSandbox.GameServer.GameObjects
{
    public class Talent : ITalent
    {
        public string Name { get; }
        public byte Rank { get; }
        public ITalentScript Script { get; }

        public Talent(Game game, string name, byte level)
        {
            Name = name;
            Rank = Math.Min(level, GetMasteryMaxLevel(name));
            Script = game.ScriptEngine.CreateObject<ITalentScript>("Talents", $"Talent_{name}") ?? new EmptyTalentScript();
        }
    }
}