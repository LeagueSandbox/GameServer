using LeagueSandbox.GameServer.Logic.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Scripting.CSharp
{
    public enum GameScriptTriggerSlot
    {
        None, Q, W, E, R, Item1, Item2, Item3, Item4, Item5, Item6,
        Trinket, SummonerSpell1, SummonerSpell2, Recall
    }
    public class GameScriptInformation
    {
        //Construct in way: new GameScriptInformation {Namespace=..., Name=..., OwnerUnit=..., OwnerSpell=...}

        public String Namespace { get; set; } = "";
        public String Name { get; set; } = "";
        public Unit OwnerUnit { get; set; } = null;
        public Spell OwnerSpell { get; set; } = null;
        public GameScriptTriggerSlot TriggerSlot = GameScriptTriggerSlot.None;
    }
}
