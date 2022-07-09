using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.Stats;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace Buffs
{
    internal class MordekaiserCOTGRevive : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.INTERNAL,
            BuffAddType = BuffAddType.RENEW_EXISTING
        };

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            //It should check if the unit is a zombie (Sion passive / Yorick R) and wait until the unit isn't anymore for then spawn the ghost and then deactivate itself.
            buff.DeactivateBuff();
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            if (buff.SourceUnit is IChampion ch && unit is IObjAIBase obj)
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
