using System;
using System.Collections.Generic;
using GameServerCore.Domain;
using GameServerCore.Scripting.CSharp;

namespace LeagueSandbox.GameServer.Content
{
    public class MasteryCollection : IMasteryCollection
    {
        Game _game;
        public Dictionary<string, byte> Masteries = new Dictionary<string, byte>();
        public Dictionary<string, IMasteryScript> MasteryScripts = new Dictionary<string, IMasteryScript>();

        public MasteryCollection(Game game)
        {
            _game = game;
        }
        
        public void Add(string name, byte level)
        {
            //Find a better way to limit levels to prevent cheating.
            Masteries.Add(name, Math.Min(level, (byte)3));
            MasteryScripts.Add(name, _game.ScriptEngine.CreateObject<IMasteryScript>("Masteries", $"Mastery_{name}"));
        }
    }
}
