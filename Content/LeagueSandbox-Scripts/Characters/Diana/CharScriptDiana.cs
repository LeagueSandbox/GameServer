using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;

namespace CharScripts
{
    public class CharScriptDiana : ICharScript
    {
        ObjAIBase diana = null;
        float stanceTime = 500;
        float stillTime = 0;
        bool beginStance = false;
        bool stance = false;

        public void OnActivate(ObjAIBase owner, Spell spell = null)
        {
            diana = owner;
        }

        public void OnUpdate(float diff)
        {
            if (diana != null)
            {
                // Not moving
                if (diana.CurrentWaypoint == diana.Position && !beginStance)
                {
                    PlayAnimation(diana, "Attack1", 5f);
                    beginStance = true;
                    if (stillTime >= stanceTime && !stance)
                    {
                        beginStance = false;
                        stance = true;
                        //PlayAnimation(diana, "Attack1", flags: GameServerCore.Enums.AnimationFlags.Lock);
                    }
                    else
                    {
                        stillTime += diff;
                    }
                }
                else
                {
                    stillTime = 0;
                    beginStance = false;
                    stance = false;
                }
            }
        }
    }
}