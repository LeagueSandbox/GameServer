using System.Collections.Generic;
using System.Numerics;
using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits.Buildings.AnimatedBuildings;
using LeagueSandbox.GameServer.Logic.GameObjects.Missiles;
using LeagueSandbox.GameServer.Logic.GameObjects.Other;
using LeagueSandbox.GameServer.Logic.GameObjects.Spells;

namespace LeagueSandbox.GameServer.Logic.Packets
{
    public interface IPacketNotifier
    {
        void NotifyAddBuff(Buff b);
        void NotifyAddGold(Champion c, AttackableUnit died, float gold);
        void NotifyAddXp(Champion champion, float experience);
        void NotifyAnnounceEvent(int mapId, Announces messageId, bool isMapSpecific);
        void NotifyBeginAutoAttack(AttackableUnit attacker, AttackableUnit victim, uint futureProjNetId, bool isCritical);
        void NotifyChampionDeathTimer(Champion die);
        void NotifyChampionDie(Champion die, AttackableUnit killer, int goldFromKill);
        void NotifyChampionRespawn(Champion c);
        void NotifyChampionSpawned(Champion c, TeamId team);
        void NotifyDamageDone(AttackableUnit source, AttackableUnit target, float amount, DamageType type, DamageText damagetext);
        void NotifyDash(AttackableUnit u, Target t, float dashSpeed, bool keepFacingLastDirection, float leapHeight, float followTargetMaxDistance, float backDistance, float travelTime);
        void NotifyDebugMessage(string htmlDebugMessage);
        void NotifyEditBuff(Buff b, int stacks);
        void NotifyEnterVision(GameObject o, TeamId team);
        void NotifyFaceDirection(AttackableUnit u, Vector2 direction, bool isInstant = true, float turnTime = 0.0833F);
        void NotifyFogUpdate2(AttackableUnit u);
        void NotifyGameEnd(Vector3 cameraPosition, Nexus nexus);
        void NotifyGameTimer(float gameTime);
        void NotifyInhibitorSpawningSoon(Inhibitor inhibitor);
        void NotifyInhibitorState(Inhibitor inhibitor, GameObject killer = null, List<Champion> assists = null);
        void NotifyItemBought(AttackableUnit u, Item i);
        void NotifyItemsSwapped(Champion c, byte fromSlot, byte toSlot);
        void NotifyLeaveVision(GameObject o, TeamId team);
        void NotifyLevelUp(Champion c);
        void NotifyMinionSpawned(Minion m, TeamId team);
        void NotifyModelUpdate(AttackableUnit obj);
        void NotifyModifyShield(AttackableUnit unit, float amount, ShieldType type);
        void NotifyMovement(GameObject o);
        void NotifyNextAutoAttack(AttackableUnit attacker, AttackableUnit target, uint futureProjNetId, bool isCritical, bool nextAttackFlag);
        void NotifyNpcDie(AttackableUnit die, AttackableUnit killer);
        void NotifyOnAttack(AttackableUnit attacker, AttackableUnit attacked, AttackType attackType);
        void NotifyParticleDestroy(Particle particle);
        void NotifyParticleSpawn(Particle particle);
        void NotifyPauseGame(int seconds, bool showWindow);
        void NotifyProjectileDestroy(Projectile p);
        void NotifyProjectileSpawn(Projectile p);
        void NotifyRemoveBuff(AttackableUnit u, string buffName, byte slot = 1);
        void NotifyRemoveItem(Champion c, byte slot, byte remaining);
        void NotifyResumeGame(AttackableUnit unpauser, bool showWindow);
        void NotifySetAnimation(AttackableUnit u, List<string> animationPairs);
        void NotifySetCooldown(Champion c, byte slotId, float currentCd, float totalCd);
        void NotifySetHealth(AttackableUnit u);
        void NotifySetTarget(AttackableUnit attacker, AttackableUnit target);
        void NotifyShowProjectile(Projectile p);
        void NotifySpawn(AttackableUnit u);
        void NotifySpellAnimation(AttackableUnit u, string animation);
        void NotifyStopAutoAttack(AttackableUnit attacker);
        void NotifyTeleport(AttackableUnit u, float x, float y);
        void NotifyUnitAnnounceEvent(UnitAnnounces messageId, AttackableUnit target, GameObject killer = null, List<Champion> assists = null);
        void NotifyUpdatedStats(AttackableUnit u, bool partial = true);
    }
}