using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;


namespace AIScripts
{
    public class BasicJungleMonsterAi : IAIScript
    {
        //NOTE: This is a EXTREMELY basic A.I just so the jungle monsters aren't just complete dummies
        public IAIScriptMetaData AIScriptMetaData { get; set; } = new AIScriptMetaData();
        IMonster monster;
        Vector2 initialPosition;
        Vector3 initialFacingDirection;
        bool isInCombat = false;
        public void OnActivate(IObjAiBase owner)
        {
            monster = owner as IMonster;
            initialPosition = monster.Position;
            initialFacingDirection = monster.Direction;
            ApiEventManager.OnTakeDamage.AddListener(this, monster, OnTakeDamage, false);
        }
        public void OnTakeDamage(IDamageData damageData)
        {
            foreach (var campMonster in monster.Camp.Monsters)
            {
                campMonster.SetTargetUnit(damageData.Attacker);
                if (campMonster.AIScript is BasicJungleMonsterAi basicJungleScript)
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
                else
                {
                    if (monster.Position != initialPosition)
                    {
                        ResetCamp();
                    }
                    else if (monster.Direction != initialFacingDirection)
                    {
                        monster.FaceDirection(initialFacingDirection);
                    }
                }
            }
        }
        public void ResetCamp()
        {
            foreach (var campMonster in monster.Camp.Monsters)
            {
                campMonster.SetTargetUnit(null);
                var waypoints = GetPath(monster.Position, initialPosition);
                if (waypoints != null)
                {
                    monster.SetWaypoints(waypoints);
                }
                else
                {
                    //One of the Red-side wolves in summoners rift actually spawn somewhat inside the wall, so it really can't
                    //Path back to it's spawn position. So this is just to solve that issue.
                    initialPosition = monster.Position;
                }
                monster.Stats.CurrentHealth = monster.Stats.HealthPoints.Total;
                if (campMonster.AIScript is BasicJungleMonsterAi basicJungleScript)
                {
                    basicJungleScript.isInCombat = false;
                }
            }
        }
    }
}