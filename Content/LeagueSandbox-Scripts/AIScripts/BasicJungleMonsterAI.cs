using System.Numerics;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.GameObjects;
using            GameServerLib.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace AIScripts
{
    public class BasicJungleMonsterAI : IAIScript
    {
        //NOTE: This is a EXTREMELY basic A.I just so the jungle monsters aren't just complete dummies
        public AIScriptMetaData AIScriptMetaData { get; set; } = new AIScriptMetaData();
        Monster monster;
        Vector2 initialPosition;
        Vector3 initialFacingDirection;
        bool isInCombat = false;
        public void OnActivate(ObjAIBase owner)
        {
            monster = owner as Monster;
            initialPosition = monster.Position;
            initialFacingDirection = monster.Direction;
            ApiEventManager.OnTakeDamage.AddListener(this, monster, OnTakeDamage, false);
        }
        public void OnTakeDamage(DamageData damageData)
        {
            foreach (var campMonster in monster.Camp.Monsters)
            {
                campMonster.SetTargetUnit(damageData.Attacker);
                if (campMonster.AIScript is BasicJungleMonsterAI basicJungleScript)
                {
                    basicJungleScript.isInCombat = true;
                }
            }
        }
        public void OnUpdate(float diff)
        {
            if (monster != null)
            {
                if (isInCombat)
                {
                    //Find a better way to do this
                    if (Vector2.DistanceSquared(new Vector2(monster.Camp.Position.X, monster.Camp.Position.Z), monster.Position) > 800f * 800f)
                    {
                        ResetCamp();
                    }
                }
                else if (monster.IsPathEnded() && monster.Direction != initialFacingDirection)
                {
                    monster.FaceDirection(initialFacingDirection);
                }
            }
        }
        public void ResetCamp()
        {
            foreach (var campMonster in monster.Camp.Monsters)
            {
                if (campMonster.AIScript is BasicJungleMonsterAI basicJungleScript && basicJungleScript.isInCombat)
                {
                    campMonster.SetTargetUnit(null);
                    campMonster.SetPathTrueEnd(basicJungleScript.initialPosition);
                    campMonster.Stats.CurrentHealth = campMonster.Stats.HealthPoints.Total;
                    basicJungleScript.isInCombat = false;
                }
            }
        }
    }
}