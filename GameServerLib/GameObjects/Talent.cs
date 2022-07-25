using System;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.Content.TalentContentCollection;
using static GameServerCore.Content.HashFunctions;

namespace LeagueSandbox.GameServer.GameObjects
{
    public class Talent
    {
        public string Name { get; }
        public byte Rank { get; }
        public ITalentScript Script { get; }
        public uint ScriptNameHash { get; private set; }
        public IEventSource ParentScript => null;

        public Talent(string name, byte level)
        {
            Name = name;
            Rank = Math.Min(level, GetTalentMaxRank(name));
            Script = CSharpScriptEngine.CreateObjectStatic<ITalentScript>("Talents", $"Talent_{name}") ?? new EmptyTalentScript();
            ScriptNameHash = HashString(name);
        }
    }
}
