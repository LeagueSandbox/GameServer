using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects.Spell;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using static LeagueSandbox.GameServer.API.ApiMapFunctionManager;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;

namespace Buffs
{
    internal class AscRelicBombBuff : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.INTERNAL,
            BuffAddType = BuffAddType.REPLACE_EXISTING,
            IsHidden = false
        };

        public IStatsModifier StatsModifier { get; private set; }

        IParticle p1;
        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            AddUnitPerceptionBubble(unit, 800.0f, 25000.0f, TeamId.TEAM_BLUE, false, null, 38.08f, collisionOwner: unit);
            AddUnitPerceptionBubble(unit, 800.0f, 25000.0f, TeamId.TEAM_PURPLE, false, null, 38.08f, collisionOwner: unit);
            p1 = AddParticleTarget(unit, unit, "Asc_RelicPrism_Sand", unit, -1.0f, 1.0f ,direction: new Vector3(0.0f, 0.0f, -1.0f), flags: (FXFlags)304);
            AddParticleTarget(unit, unit, "Asc_relic_Sand_buf", unit, -1.0f, flags: (FXFlags)32);
            SetMinimapIcon(unit, "Relic", true);
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
        }

        public void OnUpdate(float diff)
        {
        }
    }
}