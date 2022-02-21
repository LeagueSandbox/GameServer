using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using System;
using System.Collections.Generic;
using System.Text;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.API;
using GameServerCore.Domain;
using System.Numerics;

namespace CharScripts
{
    internal class CharScriptSru_Crab : ICharScript
    {
        bool isScuttleWard = false;
        bool hasPathEnded = false;
        IMinion ScuttleCrab;
        public void OnActivate(IObjAiBase owner, ISpell spell = null)
        {
            ScuttleCrab = owner as IMinion;
            OverrideAnimation(owner, "IDLE1", "CRAB_SPAWN");
            if (ScuttleCrab.Name != "FakeCrab")
            {
                ApiEventManager.OnDeath.AddListener(this, owner, OnDeath, true);
            }
            else
            {
                isScuttleWard = true;
                PlayAnimation(ScuttleCrab, "crab_burrow", 0.0f, 0.0f, 1.0f, (AnimationFlags)168);
                OverrideAnimation(owner, "RUN", "WARD_RUN (UNCOMPRESSED)");
                OverrideAnimation(owner, "IDLE1", "ward_run (Uncompressed)");
            }
        }

        public void OnDeath(IDeathData deathData)
        {
            IMonster monster = deathData.Unit as IMonster;
            SetStatus(monster, StatusFlags.NoRender, true);

            var minion = AddMinion(null, "Sru_Crab", "FakeCrab", deathData.Unit.Position, deathData.Killer.Team, ignoreCollision: true, targetable: true);
            minion.SetWaypoints(GetPath(ScuttleCrab.Position, new Vector2(monster.Camp.Position.X, monster.Camp.Position.Z)));
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell = null)
        {
        }

        public void OnUpdate(float diff)
        {
            /*if (isScuttleWard && ScuttleCrab != null)
            {
                if (!hasPathEnded && ScuttleCrab.IsPathEnded())
                {
                    PlayAnimation(ScuttleCrab, "ward_run_toground", 0.0f, 0.0f, 1.0f, (AnimationFlags)168);
                    SetAnimStates(ScuttleCrab, new Dictionary<string, string> { { "RUN", "WARD_RUN (UNCOMPRESSED)" }, { "IDLE1", "WARD_HIDE" } });
                    SetAnimStates(ScuttleCrab, new Dictionary<string, string> { { "RUN", "ward_hide" }, { "IDLE1", "WARD_HIDE" } });

                    AddUnitPerceptionBubble(ScuttleCrab, 525.0f, 75.0f, ScuttleCrab.Team);
                }
            }*/
        }
    }
}
