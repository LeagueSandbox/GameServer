﻿using System.Collections.Generic;
using System.Numerics;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;

namespace LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI
{
    public class LaneTurret : BaseTurret, ILaneTurret
    {
        public TurretType Type { get; }

        public LaneTurret(
            Game game,
            string name,
            string model,
            Vector2 position,
            TeamId team = TeamId.TEAM_BLUE,
            TurretType type = TurretType.OUTER_TURRET,
            int[] items = null,
            uint netId = 0,
            LaneID lane = LaneID.NONE,
            IMapObject mapObject = null,
            string aiScript = ""
        ) : base(game, name, model, position, team, netId, lane, mapObject, aiScript: aiScript)
        {
            Type = type;

            if (items != null)
            {
                foreach (var item in items)
                {
                    var itemTemplate = _itemManager.SafeGetItemType(item);
                    if (itemTemplate == null)
                    {
                        continue;
                    }
                    Inventory.AddItem(itemTemplate, this);
                }
            }

            if (type == TurretType.FOUNTAIN_TURRET)
            {
                SetIsTargetableToTeam(TeamId.TEAM_BLUE, false);
                SetIsTargetableToTeam(TeamId.TEAM_PURPLE, false);
            }
        }

        //TODO: Decide wether we want MapScrits to handle this with Events or leave this here
        public override void Die(IDeathData data)
        {
            float localGold = CharData.LocalGoldGivenOnDeath;
            float globalGold = CharData.GlobalGoldGivenOnDeath;
            float globalEXP = CharData.GlobalExpGivenOnDeath;

            //TODO: change this to assists
            var championsInRange = _game.ObjectManager.GetChampionsInRange(Position, Stats.Range.Total * 1.5f, true);

            if (localGold > 0 && championsInRange.Count > 0)
            {
                foreach (var champion in championsInRange)
                {
                    if (champion.Team == Team)
                    {
                        continue;
                    }

                    float gold = CharData.LocalGoldGivenOnDeath / championsInRange.Count;
                    champion.AddGold(champion, gold);
                    champion.AddGold(this, globalGold);
                }

                foreach (var player in _game.PlayerManager.GetPlayers())
                {
                    var champion = player.Item2.Champion;
                    if (player.Item2.Team != Team)
                    {
                        if (!championsInRange.Contains(champion))
                        {
                            champion.AddGold(champion, globalGold);
                        }
                        champion.AddExperience(globalEXP);
                    }
                }
            }
            else
            {
                foreach (var player in _game.PlayerManager.GetPlayers().FindAll(x => x.Item2.Team != Team))
                {
                    var champion = player.Item2.Champion;
                    if (player.Item2.Team != Team)
                    {
                        {
                            champion.AddGold(champion, globalGold);
                            champion.AddExperience(globalEXP);
                        }
                    }
                }
            }
            base.Die(data);
        }

        public override void AutoAttackHit(IAttackableUnit target)
        {
            if (Type == TurretType.FOUNTAIN_TURRET)
            {
                target.TakeDamage(this, 1000, DamageType.DAMAGE_TYPE_TRUE, DamageSource.DAMAGE_SOURCE_ATTACK, false);
            }
            else
            {
                base.AutoAttackHit(target);
            }
        }
    }
}
