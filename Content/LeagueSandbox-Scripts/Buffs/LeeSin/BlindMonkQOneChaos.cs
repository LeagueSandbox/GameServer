using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.StatsNS;

namespace Buffs
{
    internal class BlindMonkQOneChaos : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.COMBAT_DEHANCER,
            BuffAddType = BuffAddType.REPLACE_EXISTING
        };

        public StatsModifier StatsModifier { get; private set; }

        Spell originSpell;
        Buff thisBuff;
        Region bubble1;
        Region bubble2;
        Particle slow;

        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            originSpell = ownerSpell;
            thisBuff = buff;

            bubble1 = AddUnitPerceptionBubble(unit, 400.0f, 20.0f, buff.SourceUnit.Team);
            bubble2 = AddUnitPerceptionBubble(unit, 50.0f, 20.0f, buff.SourceUnit.Team, true);

            // ApplyAssistMarker(buff.SourceUnit, unit, 10.0f);

            AddParticleTarget(buff.SourceUnit, unit, "blindMonk_Q_resonatingStrike_tar", unit, buff.Duration, bone: "C_BuffBone_Glb_Center_Loc", flags: 0);
            AddParticleTarget(buff.SourceUnit, unit, "blindMonk_Q_resonatingStrike_tar_blood", unit, buff.Duration, bone: "C_BuffBone_Glb_Center_Loc", flags: 0);
            slow = AddParticleTarget(buff.SourceUnit, unit, "blindMonk_Q_tar_indicator", unit, buff.Duration, flags: 0);
            if (buff.SourceUnit.SkinID == 5)
            {
                AddParticleTarget(buff.SourceUnit, unit, "leesin_skin05_q_tar_sound", unit, buff.Duration, bone: "C_BuffBone_Glb_Center_Loc", flags: 0);
            }
        }

        public void OnDeactivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            bubble1.SetToRemove();
            bubble2.SetToRemove();
            slow.SetToRemove();

            var manager = unit.GetBuffWithName("BlindMonkQManager");

            if (manager != null && manager.SourceUnit == buff.SourceUnit)
            {
                RemoveBuff(manager);
            }
        }
    }
}