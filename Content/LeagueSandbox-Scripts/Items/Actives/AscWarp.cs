using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;

namespace ItemSpells
{
    public class AscWarp : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            // TODO
            TriggersSpellCasts = true
        };

        public Vector2 teleportTo;

        public void OnSpellPreCast(ObjAIBase owner, Spell spell, AttackableUnit target, Vector2 start, Vector2 end)
        {
            var units = (GetUnitsInRange(new Vector2(spell.CastInfo.TargetPosition.X, spell.CastInfo.TargetPosition.Z), 500.0f, true)).FindAll(x => x is Minion);

            if(units != null)
            {
                foreach (var unit in units)
                {
                    if ((unit as Minion).Name == "AscWarpIcon" && unit.Team == owner.Team)
                    {
                        AddBuff("AscWarp", 3.5f, 1, spell, spell.CastInfo.Owner, spell.CastInfo.Owner);
                        var minion = AddMinion(owner, "TestCubeRender10Vision", "k", start, team: owner.Team, targetable: false, isVisible: false, skinId: owner.SkinID, isWard: true);
                        //NotifySpawnBroadcast(minion);
                        AddBuff("AscWarpTarget", 3.5f, 1, spell, minion, owner);
                    }
                }
            }
        }
    }
}
