using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects;
using System.Collections.Generic;
using static LeagueSandbox.GameServer.Content.TalentContentCollection;

namespace LeagueSandbox.GameServer.Inventory
{
    public class TalentInventory : ITalentInventory
    {
        Game _game;
        public List<ITalent> Talents { get; } = new List<ITalent>();

        public TalentInventory(Game game)
        {
            _game = game;
        }

        public void Add(string talentId, byte level)
        {
            if (TalentIsValid(talentId))
            {
                Talents.Add(new Talent(_game, talentId, level));
            }
        }

        public void Initialize(IObjAiBase owner)
        {
            foreach (var mastery in Talents)
            {
                mastery.Script.OnActivate(owner, mastery.Rank);
            }
        }
    }
}