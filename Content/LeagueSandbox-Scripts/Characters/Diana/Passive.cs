using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using System.Numerics;
using GameServerCore.Scripting.CSharp;

namespace CharScripts
{
    public class CharScriptDiana : ICharScript
    {
        IObjAiBase diana = null;
        float stanceTime = 500;
        float stillTime = 0;
        bool beginStance = false;
        bool stance = false;

        public void OnActivate(IObjAiBase owner, ISpell spell = null)
        {
            diana = owner;
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell = null)
        {
        }

        public void OnUpdate(float diff)
        {
            if (diana != null)
            {
                // Not moving
                if (diana.CurrentWaypoint.Value == diana.Position && !beginStance)
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