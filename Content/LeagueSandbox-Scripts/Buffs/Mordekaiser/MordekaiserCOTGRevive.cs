using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace Buffs
{
    internal class MordekaiserCOTGRevive : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.INTERNAL,
            BuffAddType = BuffAddType.RENEW_EXISTING
        };

        public StatsModifier StatsModifier { get; private set; } = new StatsModifier();

        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            //It should check if the unit is a zombie (Sion passive / Yorick R) and wait until the unit isn't anymore for then spawn the ghost and then deactivate itself.
            buff.DeactivateBuff();
        }

        public void OnDeactivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            if (buff.SourceUnit is Champion ch && unit is ObjAIBase obj)
            {
                var pet = CreateClonePet(
                    owner: ch,
                    spell: ownerSpell,
                    cloned: obj,
                    position: Vector2.Zero,
                    buffName: "MordekaiserCotgPetSlow",
                    lifeTime: 0.0f,
                    cloneInventory: true,
                    showMinimapIfClone: true,
                    disallowPlayerControl: false,
                    doFade: false
                    );

                AddBuff("MordekaiserCOTGPetBuff2", 30.0f, 1, ownerSpell, pet, ch);
            }
        }
    }
}
