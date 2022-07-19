using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.SpellNS;

namespace Buffs
{
    internal class AscRelicBombBuff : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.INTERNAL,
            BuffAddType = BuffAddType.REPLACE_EXISTING,
            IsHidden = false
        };

        public StatsModifier StatsModifier { get; private set; }

        Particle p1;
        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            AddUnitPerceptionBubble(unit, 800.0f, 25000.0f, TeamId.TEAM_BLUE, false, null, 38.08f);
            AddUnitPerceptionBubble(unit, 800.0f, 25000.0f, TeamId.TEAM_PURPLE, false, null, 38.08f);
            p1 = AddParticleTarget(unit, unit, "Asc_RelicPrism_Sand", unit, -1.0f, 1.0f ,direction: new Vector3(0.0f, 0.0f, -1.0f), flags: (FXFlags)304);
            AddParticleTarget(unit, unit, "Asc_relic_Sand_buf", unit, -1.0f, flags: (FXFlags)32);
            unit.IconInfo.ChangeIcon("Relic");
        }
    }
}