using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.API;
using static LeagueSandbox.GameServer.API.ApiMapFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using GameServerCore.Scripting.CSharp;
using GameServerCore.Enums;

namespace ItemSpells
{
    public class AscWarp : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            // TODO
            TriggersSpellCasts = true
        };

        public Vector2 teleportTo;
        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnSpellPreCast(IObjAiBase owner, ISpell spell, IAttackableUnit target, Vector2 start, Vector2 end)
        {
            var units = (GetUnitsInRange(new Vector2(spell.CastInfo.TargetPosition.X, spell.CastInfo.TargetPosition.Z), 500.0f, true)).FindAll(x => x is IMinion);

            if(units != null)
            {
                foreach (var unit in units)
                {
                    if ((unit as IMinion).Name == "AscWarpIcon" && unit.Team == owner.Team)
                    {
                        AddBuff("AscWarp", 3.5f, 1, spell, spell.CastInfo.Owner, spell.CastInfo.Owner);
                        var minion = AddMinion(owner, "TestCubeRender10Vision", "k", start, team: owner.Team, targetable: false, isVisible: false, skinId: owner.SkinID, isWard: true);
                        //NotifySpawnBroadcast(minion);
                        AddBuff("AscWarpTarget", 3.5f, 1, spell, minion, owner);
                    }
                }
            }
        }

        public void OnSpellCast(ISpell spell)
        {
        }

        public void OnSpellPostCast(ISpell spell)
        {
        }

        public void OnSpellChannel(ISpell spell)
        {
        }

        public void OnSpellChannelCancel(ISpell spell, ChannelingStopSource reason)
        {
        }

        public void OnSpellPostChannel(ISpell spell)
        {
        }

        public void OnUpdate(float diff)
        {
        }
    }
}
