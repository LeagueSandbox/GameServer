using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.StatsNS;

namespace Buffs
{
    internal class OdinChannelVision : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.INTERNAL,
            BuffAddType = BuffAddType.REPLACE_EXISTING,
        };

        public StatsModifier StatsModifier { get; private set; }

        Region r1;
        Region r2;
        Region r3;
        Region r4;
        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            r1 = AddUnitPerceptionBubble(unit, 400.0f, buff.Duration, TeamId.TEAM_BLUE, false, collisionArea: 35.0f, regionType: RegionType.Unknown2);
            r2 = AddUnitPerceptionBubble(unit, 50.0f, buff.Duration, TeamId.TEAM_BLUE, true, collisionArea: 35.0f, regionType: RegionType.Unknown2);

            r3 = AddUnitPerceptionBubble(unit, 400.0f, buff.Duration, TeamId.TEAM_PURPLE, false, collisionArea: 35.0f, regionType: RegionType.Unknown2);
            r4 = AddUnitPerceptionBubble(unit, 50.0f, buff.Duration, TeamId.TEAM_PURPLE, true, collisionArea: 35.0f, regionType: RegionType.Unknown2);
        }

        public void OnDeactivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            r1.SetToRemove();
            r2.SetToRemove();
            r3.SetToRemove();
            r4.SetToRemove();
        }
    }
}

