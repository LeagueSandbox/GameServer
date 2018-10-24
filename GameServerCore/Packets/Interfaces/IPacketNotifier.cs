using System.Collections.Generic;
using System.Numerics;
using GameServerCore.Content;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enet;
using GameServerCore.Enums;
using GameServerCore.Packets.Enums;
using GameServerCore.Packets.PacketDefinitions.Requests;

namespace GameServerCore.Packets.Interfaces
{
    public interface IPacketNotifier
    {
        void NotifyAddBuff(IBuff b);
        void NotifyAddGold(IChampion c, IAttackableUnit died, float gold);
        void NotifyAddXp(IChampion champion, float experience);
        void NotifyAnnounceEvent(int mapId, Announces messageId, bool isMapSpecific);
        void NotifyBeginAutoAttack(IAttackableUnit attacker, IAttackableUnit victim, uint futureProjNetId, bool isCritical);
        void NotifyChampionDeathTimer(IChampion die);
        void NotifyChampionDie(IChampion die, IAttackableUnit killer, int goldFromKill);
        void NotifyChampionRespawn(IChampion c);
        void NotifyChampionSpawned(IChampion c, TeamId team);
        void NotifyDamageDone(IAttackableUnit source, IAttackableUnit target, float amount, DamageType type, DamageText damagetext, bool isGlobal = true, int sourceId = 0, int targetId = 0);
        void NotifyDash(IAttackableUnit u, ITarget t, float dashSpeed, bool keepFacingLastDirection, float leapHeight, float followTargetMaxDistance, float backDistance, float travelTime);
        void NotifyDebugMessage(string htmlDebugMessage);
        void NotifyDebugMessage(int userId, string message);
        void NotifyDebugMessage(TeamId team, string message);
        void NotifyEditBuff(IBuff b, int stacks);
        void NotifyEnterVision(IGameObject o, TeamId team);
        void NotifyFaceDirection(IAttackableUnit u, Vector2 direction, bool isInstant = true, float turnTime = 0.0833F);
        void NotifyFogUpdate2(IAttackableUnit u, uint newFogId);
        void NotifyGameEnd(Vector3 cameraPosition, INexus nexus, List<Pair<uint, ClientInfo>> players);
        void NotifyGameTimer(float gameTime);
        void NotifyInhibitorSpawningSoon(IInhibitor inhibitor);
        void NotifyInhibitorState(IInhibitor inhibitor, IGameObject killer = null, List<IChampion> assists = null);
        void NotifyItemBought(IAttackableUnit u, IItem i);
        void NotifyItemsSwapped(IChampion c, byte fromSlot, byte toSlot);
        void NotifyLeaveVision(IGameObject o, TeamId team);
        void NotifyLevelUp(IChampion c);
        void NotifyMinionSpawned(IMinion m, TeamId team);
        void NotifyModelUpdate(IAttackableUnit obj);
        void NotifyModifyShield(IAttackableUnit unit, float amount, ShieldType type);
        void NotifyMovement(IGameObject o);
        void NotifyNextAutoAttack(IAttackableUnit attacker, IAttackableUnit target, uint futureProjNetId, bool isCritical, bool nextAttackFlag);
        void NotifyNpcDie(IAttackableUnit die, IAttackableUnit killer);
        void NotifyOnAttack(IAttackableUnit attacker, IAttackableUnit attacked, AttackType attackType);
        void NotifyParticleDestroy(IParticle particle);
        void NotifyParticleSpawn(IParticle particle);
        void NotifyPauseGame(int seconds, bool showWindow);
        void NotifyProjectileDestroy(IProjectile p);
        void NotifyProjectileSpawn(IProjectile p);
        void NotifyRemoveBuff(IAttackableUnit u, string buffName, byte slot = 1);
        void NotifyRemoveItem(IChampion c, byte slot, byte remaining);
        void NotifyResumeGame(IAttackableUnit unpauser, bool showWindow);
        void NotifySetAnimation(IAttackableUnit u, List<string> animationPairs);
        void NotifySetCooldown(IChampion c, byte slotId, float currentCd, float totalCd);
        void NotifySetHealth(IAttackableUnit u);
        void NotifySetTarget(IAttackableUnit attacker, IAttackableUnit target);
        void NotifyShowProjectile(IProjectile p);
        void NotifySpawn(IAttackableUnit u);
        void NotifySpellAnimation(IAttackableUnit u, string animation);
        void NotifyStopAutoAttack(IAttackableUnit attacker);
        void NotifyTeleport(IAttackableUnit u, float x, float y);
        void NotifyUnitAnnounceEvent(UnitAnnounces messageId, IAttackableUnit target, IGameObject killer = null, List<IChampion> assists = null);
        void NotifyUpdatedStats(IAttackableUnit u, bool partial = true);
        void NotifyPing(ClientInfo client, float x, float y, int targetNetId, Pings type);
        void NotifyTint(TeamId team, bool enable, float speed, byte r, byte g, byte b, float a);
        void NotifySkillUp(int userId, uint netId, byte skill, byte level, byte pointsLeft);
        void NotifySetTeam(IAttackableUnit unit, TeamId team);
        void NotifyCastSpell(INavGrid navGrid, ISpell s, float x, float y, float xDragEnd, float yDragEnd, uint futureProjNetId, uint spellNetId);

        /// <summary> TODO: tipCommand should be an lib/core enum that gets translated into league version specific packet enum as it may change over time </summary>
        void NotifyBlueTip(int userId, string title, string text, string imagePath, byte tipCommand, uint playerNetId, uint targetNetId);
        void NotifyEmotions(Emotions type, uint netId);
        // TODO: move handling to PacketDefinitions
        void NotifyKeyCheck(long userId, int playerNo);
        void NotifyPingLoadInfo(PingLoadInfoRequest request, long userId);
        void NotifyViewResponse(int userId, ViewRequest request);
        void NotifySynchVersion(int userId, List<Pair<uint, ClientInfo>> players, string version, string gameMode, int mapId);
        void NotifyLoadScreenInfo(int userId, List<Pair<uint, ClientInfo>> players);
        void NotifyLoadScreenPlayerName(int userId, Pair<uint, ClientInfo> player);
        void NotifyLoadScreenPlayerChampion(int userId, Pair<uint, ClientInfo> player);
        void NotifyQueryStatus(int userId);
        void NotifyPlayerStats(IChampion champion);
        void NotifySurrender(IChampion starter, byte flag, byte yesVotes, byte noVotes, byte maxVotes, TeamId team, float timeOut);
        void NotifyGameStart();
        void NotifyHeroSpawn2(int userId, IChampion champion);
        void NotifyGameTimer(int userId, float time);
        void NotifyGameTimerUpdate(int userId, float time);
        void NotifySpawnStart(int userId);
        void NotifySpawnEnd(int userId);
        void NotifyHeroSpawn(int userId, ClientInfo client, int playerId);
        void NotifyAvatarInfo(int userId, ClientInfo client);
        void NotifyBuyItem(int userId, IChampion champion, IItem itemInstance);
        void NotifyTurretSpawn(int userId, ILaneTurret turret);
        void NotifySetHealth(int userId, IAttackableUnit unit);
        void NotifyLevelPropSpawn(int userId, ILevelProp levelProp);
        void NotifyEnterVision(int userId, IChampion champion);
        void NotifyStaticObjectSpawn(int userId, uint netId);
        void NotifySetHealth(int userId, uint netId);
        void NotifyProjectileSpawn(int userId, IProjectile projectile);
        void NotifyUnpauseGame();

        // TODO: only in deubg mode
        void NotifyDebugPacket(int userId, byte[] data);
    }
}