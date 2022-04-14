using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.Logging;
using log4net;
using System.Collections.Generic;
using static LeagueSandbox.GameServer.Content.TalentContentCollection;

namespace LeagueSandbox.GameServer.Inventory
{
    public class TalentInventory : ITalentInventory
    {
        public List<ITalent> Talents { get; } = new List<ITalent>();
        ILog _logger = LoggerProvider.GetLogger();

        public void Add(string talentId, byte level)
        {
            if (TalentIsValid(talentId))
            {
                Talents.Add(new Talent(talentId, level));
            }
            else
            {
                _logger.Warn($"No Talent with ID {talentId} found! Skipping...");
            }
        }

        public void Initialize(IObjAiBase owner)
        {
            foreach (var talent in Talents)
            {
                if(talent.Rank > 0)
                {
                    talent.Script.OnActivate(owner, talent.Rank);
                }
            }
        }
    }
}