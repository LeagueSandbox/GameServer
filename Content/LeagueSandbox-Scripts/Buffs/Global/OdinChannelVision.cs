using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace Buffs
{
    internal class OdinChannelVision : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.INTERNAL,
            BuffAddType = BuffAddType.REPLACE_EXISTING,
        };

        public IStatsModifier StatsModifier { get; private set; }

        IRegion r1;
        IRegion r2;
        IRegion r3;
        IRegion r4;
        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            r1 = AddUnitPerceptionBubble(unit, 400.0f, buff.Duration, TeamId.TEAM_BLUE, false, collisionArea: 35.0f, collisionOwner: unit, regionType: RegionType.Unknown2);
            r2 = AddUnitPerceptionBubble(unit, 50.0f, buff.Duration, TeamId.TEAM_BLUE, true, collisionArea: 35.0f, collisionOwner: unit, regionType: RegionType.Unknown2);

            r3 = AddUnitPerceptionBubble(unit, 400.0f, buff.Duration, TeamId.TEAM_PURPLE, false, collisionArea: 35.0f, collisionOwner: unit, regionType: RegionType.Unknown2);
            r4 = AddUnitPerceptionBubble(unit, 50.0f, buff.Duration, TeamId.TEAM_PURPLE, true, collisionArea: 35.0f, collisionOwner: unit, regionType: RegionType.Unknown2);
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            r1.SetToRemove();
            r2.SetToRemove();
            r3.SetToRemove();
            r4.SetToRemove();
        }

        public void OnUpdate(float diff)
        {
        }
    }
}

