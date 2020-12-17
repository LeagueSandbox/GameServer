﻿using System.Collections.Generic;
using System.Numerics;
using GameServerCore.Content;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.NetInfo;
using GameServerCore.Enums;
using GameServerCore.Packets.Enums;
using GameServerCore.Packets.PacketDefinitions.Requests;
using System;

namespace GameServerCore.Packets.Interfaces
{
    /// <summary>
    /// Interface containing all function related packets (except handshake) which are sent by the server to game clients.
    /// </summary>
    public interface IPacketNotifier
    {
        // TODO: Maybe clean up all the function parameters somehow?

        /// <summary>
        /// Sends a packet to the specified user which is intended to creates a client-side debug object. *NOTE*: Has not been tested, function implementation may be incorrect.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="unit">Unit that </param>
        /// <param name="objNetId">NetID to assign to the debug object.</param>
        /// <param name="lifetime">How long the debug object should exist (in seconds).</param>
        /// <param name="radius">Distance from the center of the debug object to its edge.</param>
        /// <param name="pos1">Position of the first point. Untested.</param>
        /// <param name="pos2">Position of the second point. Untested.</param>
        /// <param name="objID">Index of the debug object. Untested, function unknown.</param>
        /// <param name="type">Type of debug object. Untested, possible types unknown.</param>
        /// <param name="name">Name of the debug object. Untested. Might be displayed as floating text?</param>
        /// <param name="r">Red hex color value.</param>
        /// <param name="g">Green hex color value.</param>
        /// <param name="b">Blue hex color value.</param>
        void NotifyAddDebugObject(int userId, IAttackableUnit unit, uint objNetId, float lifetime, float radius, Vector3 pos1, Vector3 pos2, int objID = 0, byte type = 0x0, string name = "debugobj", byte r = 0xFF, byte g = 0x46, byte b = 0x0);
        /// <summary>
        /// Sends a packet to all players that the specified player has killed a specified player and received a specified amount of gold.
        /// </summary>
        /// <param name="c">Champion that killed a unit.</param>
        /// <param name="died">AttackableUnit that died to the Champion.</param>
        /// <param name="gold">Amount of gold the Champion gained for the kill.</param>
        void NotifyAddGold(IChampion c, IAttackableUnit died, float gold);
        /// <summary>
        /// Sends a packet to all players that have vision of the specified Azir turret that it has spawned.
        /// </summary>
        /// <param name="azirTurret">AzirTurret instance.</param>
        void NotifyAzirTurretSpawned(IAzirTurret azirTurret);
        /// <summary>
        /// Sends a packet to all players that the specified Champion has gained the specified amount of experience.
        /// </summary>
        /// <param name="champion">Champion that gained the experience.</param>
        /// <param name="experience">Amount of experience gained.</param>
        void NotifyAddXp(IChampion champion, float experience);
        /// <summary>
        /// Sends a packet to all players that announces a specified message (ex: "Minions have spawned.")
        /// </summary>
        /// <param name="mapId">Current map ID.</param>
        /// <param name="messageId">Message ID to announce.</param>
        /// <param name="isMapSpecific">Whether the announce is specific to the map ID.</param>
        void NotifyAnnounceEvent(int mapId, Announces messageId, bool isMapSpecific);
        /// <summary>
        /// Sends a packet to the specified user that informs them of their summoner data such as runes, summoner spells, masteries (or talents as named internally), etc.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="client">Info about the player's summoner data.</param>
        void NotifyAvatarInfo(int userId, ClientInfo client);
        /// <summary>
        /// Sends a packet to all players that the specified attacker is starting their first auto attack.
        /// </summary>
        /// <param name="attacker">AttackableUnit that is starting an auto attack.</param>
        /// <param name="victim">AttackableUnit that is the target of the auto attack.</param>
        /// <param name="futureProjNetId">NetID of the projectile that will be created for the auto attack.</param>
        /// <param name="isCritical">Whether or not the auto attack is a critical.</param>
        void NotifyBeginAutoAttack(IAttackableUnit attacker, IAttackableUnit victim, uint futureProjNetId, bool isCritical);
        /// <summary>
        /// Sends a side bar tip to the specified player (ex: quest tips).
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="title">Title of the tip.</param>
        /// <param name="text">Description text of the tip.</param>
        /// <param name="imagePath">Path to an image that will be embedded in the tip.</param>
        /// <param name="tipCommand">Action suggestion(? unconfirmed).</param>
        /// <param name="playerNetId">NetID to send the packet to.</param>
        /// <param name="targetNetId">NetID of the target referenced by the tip.</param>
        /// TODO: tipCommand should be a lib/core enum that gets translated into a league version specific packet enum as it may change over time.
        void NotifyBlueTip(int userId, string title, string text, string imagePath, byte tipCommand, uint playerNetId, uint targetNetId);
        /// <summary>
        /// Sends a packet to the player attempting to buy an item that their purchase was successful.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="gameObject">GameObject of type ObjAiBase that can buy items.</param>
        /// <param name="itemInstance">Item instance housing all information about the item that has been bought.</param>
        void NotifyBuyItem(int userId, IObjAiBase gameObject, IItem itemInstance);
        /// <summary>
        /// Sends a packet to all players updating a player's death timer.
        /// </summary>
        /// <param name="champion">Champion that died.</param>
        void NotifyChampionDeathTimer(IChampion champion);
        /// <summary>
        /// Sends a packet to all players that a champion has died and calls a death timer update packet.
        /// </summary>
        /// <param name="champion">Champion that died.</param>
        /// <param name="killer">Unit that killed the Champion.</param>
        /// <param name="goldFromKill">Amount of gold the killer received.</param>
        void NotifyChampionDie(IChampion champion, IAttackableUnit killer, int goldFromKill);
        /// <summary>
        /// Sends a packet to all players that a champion has respawned.
        /// </summary>
        /// <param name="c">Champion that respawned.</param>
        void NotifyChampionRespawn(IChampion c);
        /// <summary>
        /// Sends a packet to all players of the specified team that a champion has spawned (for the first time).
        /// </summary>
        /// <param name="c">Champion that has spawned.</param>
        /// <param name="team">Team to send the packet to.</param>
        void NotifyChampionSpawned(IChampion c, TeamId team);
        /// <summary>
        /// Sends a packet to all players with vision of a specified ObjAiBase explaining that their specified spell's cooldown has been set.
        /// </summary>
        /// <param name="u">ObjAiBase who owns the spell going on cooldown.</param>
        /// <param name="slotId">Slot of the spell.</param>
        /// <param name="currentCd">Amount of time the spell has already been on cooldown (if applicable).</param>
        /// <param name="totalCd">Amount of time that the spell should have be in cooldown before going off cooldown.</param>
        void NotifyCHAR_SetCooldown(IObjAiBase c, byte slotId, float currentCd, float totalCd);
        /// <summary>
        /// Sends a packet to the specified user that highlights the specified GameObject.
        /// </summary>
        /// <param name="userId">ID of the user to send the packet to.</param>
        /// <param name="unit">GameObject to highlght.</param>
        void NotifyCreateUnitHighlight(int userId, IGameObject unit);
        /// <summary>
        /// Sends a packet to optionally all players (given isGlobal), a specified user that is the source of damage, or a specified user that is receiving the damage. The packet details an instance of damage being applied to a unit by another unit.
        /// </summary>
        /// <param name="source">Unit which caused the damage.</param>
        /// <param name="target">Unit which is taking the damage.</param>
        /// <param name="amount">Amount of damage dealt to the target (usually the end result of all damage calculations).</param>
        /// <param name="type">Type of damage being dealt; PHYSICAL/MAGICAL/TRUE</param>
        /// <param name="damagetext">Type of text to show above the target; INVULNERABLE/DODGE/CRIT/NORMAL/MISS</param>
        /// <param name="isGlobal">Whether or not the packet should be sent to all players.</param>
        /// <param name="sourceId">ID of the user who dealt the damage that should receive the packet.</param>
        /// <param name="targetId">ID of the user who is taking the damage that should receive the packet.</param>
        void NotifyDamageDone(IAttackableUnit source, IAttackableUnit target, float amount, DamageType type, DamageText damagetext, bool isGlobal = true, int sourceId = 0, int targetId = 0);
        /// <summary>
        /// Sends a packet to all players that have vision of the specified unit. The packet details a group of waypoints which the unit will move to given the specified parameters.
        /// </summary>
        /// <param name="u">Unit that is dashing.</param>
        /// <param name="t">Target the unit is dashing to (single-point or unit).</param>
        /// <param name="dashSpeed">Constant speed that the unit will have during the dash.</param>
        /// <param name="keepFacingLastDirection">Whether or not the unit should maintain the direction they were facing before dashing.</param>
        /// <param name="leapHeight">How high the unit will reach at the peak of the dash.</param>
        /// <param name="followTargetMaxDistance">Maximum distance the unit will follow the Target before stopping the dash or reaching to the Target.</param>
        /// <param name="backDistance">Unknown parameter.</param>
        /// <param name="travelTime">Total time the dash will attempt to travel to the Target before stopping or reaching the Target.</param>
        /// TODO: Replace current implementation with LeaguePackets as this is incomplete.
        void NotifyDash(IAttackableUnit u, ITarget t, float dashSpeed, bool keepFacingLastDirection, float leapHeight, float followTargetMaxDistance, float backDistance, float travelTime);
        /// <summary>
        /// Sends a packet to all players detailing a debug message.
        /// </summary>
        /// <param name="htmlDebugMessage">Debug message to send.</param>
        void NotifyDebugMessage(string htmlDebugMessage);
        /// <summary>
        /// Sends a packet to the specified user detailing a debug message.
        /// </summary>
        /// <param name="userId">ID of the user to send the packet to.</param>
        /// <param name="message">Debug message to send.</param>
        void NotifyDebugMessage(int userId, string message);
        /// <summary>
        /// Sends a packet to the specified team detailing a debug message.
        /// </summary>
        /// <param name="team">TeamId to send the packet to; BLUE/PURPLE/NEUTRAL.</param>
        /// <param name="message">Debug message to send.</param>
        void NotifyDebugMessage(TeamId team, string message);
        /// <summary>
        /// Sends a packet to the specified user which is intended for debugging.
        /// </summary>
        /// <param name="userId">ID of the user to send the packet to.</param>
        /// <param name="data">Array of bytes representing the packet's data.</param>
        /// TODO: only in debug mode
        void NotifyDebugPacket(int userId, byte[] data);
        /// <summary>
        /// Sends a packet to all players detailing the destruction of (usually) an auto attack missile.
        /// </summary>
        /// <param name="p">Projectile that is being destroyed.</param>
        void NotifyDestroyClientMissile(IProjectile p);
        /// <summary>
        /// Sends a packet to the specified team detailing the destruction of (usually) an auto attack missile.
        /// </summary>
        /// <param name="p">Projectile that is being destroyed.</param>
        /// <param name="team">TeamId to send the packet to.</param>
        void NotifyDestroyClientMissile(IProjectile p, TeamId team);
        /// <summary>
        /// Sends a packet to all players detailing an emotion that is being performed by the unit that owns the specified netId.
        /// </summary>
        /// <param name="type">Type of emotion being performed; DANCE/TAUNT/LAUGH/JOKE/UNK.</param>
        /// <param name="netId">NetID of the unit performing the emotion.</param>
        void NotifyEmotions(Emotions type, uint netId);
        /// <summary>
        /// Sends a packet to the specified user detailing that the GameObject that owns the specified netId has finished being initialized into vision.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="netId">NetID of the GameObject coming into vision.</param>
        void NotifyEnterLocalVisibilityClient(int userId, uint netId);
        /// <summary>
        /// Sends a packet to either all players with vision of the specified GameObject or a specified user. The packet contains details of the GameObject's health (given it is of the type AttackableUnit) and is meant for after the GameObject is first initialized into vision.
        /// </summary>
        /// <param name="o">GameObject coming into vision.</param>
        /// <param name="userId">User to send the packet to.</param>
        void NotifyEnterLocalVisibilityClient(IGameObject o, int userId = 0);
        /// <summary>
        /// Sends a packet to either all players of the specified team or the specified user. The packet details the data surrounding the specified GameObject that is required by players when a GameObject enters vision such as items, shields, skin, and movements.
        /// </summary>
        /// <param name="o">GameObject entering vision.</param>
        /// <param name="team">TeamId to send the packet to.</param>
        /// <param name="userId">User to send the packet to.</param>
        void NotifyEnterVisibilityClient(IGameObject o, TeamId team, int userId = 0);
        /// <summary>
        /// Sends a packet to all players with vision of the specified unit detailing that the unit is facing the specified direction.
        /// </summary>
        /// <param name="u">Unit that is changing their facing direction.</param>
        /// <param name="direction">2D, top-down direction the unit will face.</param>
        /// <param name="isInstant">Whether or not the unit should instantly turn to the direction.</param>
        /// <param name="turnTime">The amount of time (seconds) the turn should take.</param>
        void NotifyFaceDirection(IAttackableUnit u, Vector2 direction, bool isInstant = true, float turnTime = 0.0833F);
        /// <summary>
        /// Sends a packet to the specified team that a part of the map has changed. Known to be used in League for initializing turret vision and collision.
        /// TODO: Replace with LeaguePackets' NotifyAddRegion
        /// </summary>
        /// <param name="u">Unit to attach the region to.</param>
        /// <param name="newFogId">NetID of the region.</param>
        void NotifyFogUpdate2(IAttackableUnit u, uint newFogId);
        /// <summary>
        /// Sends a packet to all players that (usually) an auto attack missile has been created.
        /// </summary>
        /// <param name="p">Projectile that was created.</param>
        void NotifyForceCreateMissile(IProjectile p);
        /// <summary>
        /// Sends a packet to, optionally, a specified player, all players with vision of the particle, or all players (given the particle is set as globally visible).
        /// </summary>
        /// <param name="particle">Particle to network.</param>
        /// <param name="playerId">User to send the packet to.</param>
        void NotifyFXCreateGroup(IParticle particle, int playerId = 0);
        /// <summary>
        /// Sends a packet to the specified team detailing that the specified Particle has become visible.
        /// </summary>
        /// <param name="particle">Particle that came into vision.</param>
        /// <param name="team">TeamId to send the packet to; BLUE/PURPLE/NEUTRAL</param>
        void NotifyFXEnterTeamVisibility(IParticle particle, TeamId team);
        /// <summary>
        /// Sends a packet to all players detailing that the specified particle has been destroyed.
        /// </summary>
        /// <param name="particle">Particle that is being destroyed.</param>
        /// TODO: Change to only broadcast to players who have vision of the particle (maybe?).
        void NotifyFXKill(IParticle particle);
        /// <summary>
        /// Sends a packet to the specified team detailing that the specified Particle has left vision.
        /// </summary>
        /// <param name="particle">Particle that left vision.</param>
        /// <param name="team">TeamId to send the packet to; BLUE/PURPLE/NEUTRAL.</param>
        void NotifyFXLeaveTeamVisibility(IParticle particle, TeamId team);
        /// <summary>
        /// Sends packets to all players which force the players' cameras to the nexus being destroyed, hides their UI, and ends the game.
        /// </summary>
        /// <param name="cameraPosition">Position of the nexus being destroyed.</param>
        /// <param name="nexus">Nexus being destroyed.</param>
        /// <param name="players">All players that can receive packets.</param>
        void NotifyGameEnd(Vector3 cameraPosition, INexus nexus, List<Tuple<uint, ClientInfo>> players);
        /// <summary>
        /// Sends a packet to all players detailing that the game has started. Sent when all players have finished loading.
        /// </summary>
        void NotifyGameStart();
        /// <summary>
        /// Sends a packet to all players detailing the amount of time since the game started (in seconds).
        /// </summary>
        /// <param name="gameTime">Time since the game started (in milliseconds).</param>
        void NotifyGameTimer(float gameTime);
        /// <summary>
        /// Sends a packet to the specified player detailing the amount of time since the game started (in seconds).
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="time">Time since the game started (in milliseconds).</param>
        void NotifyGameTimer(int userId, float time);
        /// <summary>
        /// Sends a packet to the specified player detailing the amount of time since the game started (in seconds). Used to initialize the user's in-game timer.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="time">Time since the game started (in milliseconds).</param>
        void NotifyGameTimerUpdate(int userId, float time);
        /// <summary>
        /// Sends a packet to the specified player detailing that (usually) their hero has spawned.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="client">Info of the client which the user owns.</param>
        void NotifyHeroSpawn(int userId, ClientInfo client);
        /// <summary>
        /// Sends a packet to the specified player which spawns (usually) their Champion.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="champion">Champion which the user owns.</param>
        void NotifyHeroSpawn2(int userId, IChampion champion);
        /// <summary>
        /// Sends a packet to all players which announces that the team which owns the specified inhibitor has an inhibitor which is respawning soon.
        /// </summary>
        /// <param name="inhibitor">Inhibitor that is respawning soon.</param>
        void NotifyInhibitorSpawningSoon(IInhibitor inhibitor);
        /// <summary>
        /// Sends a packet to all players detailing the state (DEAD/ALIVE) of the specified inhibitor.
        /// </summary>
        /// <param name="inhibitor">Inhibitor to check.</param>
        /// <param name="killer">Killer of the inhibitor (if applicable).</param>
        /// <param name="assists">Assists of the killer (if applicable).</param>
        void NotifyInhibitorState(IInhibitor inhibitor, IGameObject killer = null, List<IChampion> assists = null);
        /// <summary>
        /// Sends a packet to all players with vision of the specified Champion detailing that the Champion's items have been swapped.
        /// </summary>
        /// <param name="c">Champion who swapped their items.</param>
        /// <param name="fromSlot">Slot the item was previously in.</param>
        /// <param name="toSlot">Slot the item was swapped to.</param>
        void NotifyItemsSwapped(IChampion c, byte fromSlot, byte toSlot);
        /// <summary>
        /// Sends a packet to the specified team detailing that the specified LaneMinion has spawned.
        /// </summary>
        /// <param name="m">LaneMinion that spawned.</param>
        /// <param name="team">TeamId to send the packet to; BLUE/PURPLE/NEUTRAL.</param>
        void NotifyLaneMinionSpawned(ILaneMinion m, TeamId team);
        /// <summary>
        /// Sends a packet to the specified player detailing that the GameObject which has the specified netId has left vision.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="netId">NetId of the GameObject that left vision.</param>
        void NotifyLeaveLocalVisibilityClient(int userId, uint netId);
        /// <summary>
        /// Sends a packet to either the specified player or team detailing that the specified GameObject has left vision.
        /// </summary>
        /// <param name="o">GameObject that left vision.</param>
        /// <param name="team">TeamId to send the packet to; BLUE/PURPLE/NEUTRAL.</param>
        /// <param name="userId">User to send the packet to.</param>
        void NotifyLeaveLocalVisibilityClient(IGameObject o, TeamId team, int userId = 0);
        /// <summary>
        /// Sends a packet to either the specified user or team detailing that the specified GameObject has left vision.
        /// </summary>
        /// <param name="o">GameObject that left vision.</param>
        /// <param name="team">TeamId to send the packet to; BLUE/PURPLE/NEUTRAL.</param>
        /// <param name="userId">User to send the packet to (if applicable).</param>
        void NotifyLeaveVisibilityClient(IGameObject o, TeamId team, int userId = 0);
        /// <summary>
        /// Sends a packet to the specified player detailing that the specified GameObject of type LevelProp has spawned.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="levelProp">LevelProp that has spawned.</param>
        void NotifyLevelPropSpawn(int userId, ILevelProp levelProp);
        /// <summary>
        /// Sends a packet to all players detailing that the specified Champion has leveled up.
        /// </summary>
        /// <param name="c">Champion which leveled up.</param>
        void NotifyLevelUp(IChampion c);
        /// <summary>
        /// Sends a packet to the specified player detailing the load screen information.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="players">Client info of all players in the loading screen.</param>
        void NotifyLoadScreenInfo(int userId, List<Tuple<uint, ClientInfo>> players);
        /// <summary>
        /// Sends a packet to the specified player detailing skin information of all specified players on the loading screen.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="player">Player information to send.</param>
        void NotifyLoadScreenPlayerChampion(int userId, Tuple<uint, ClientInfo> player);
        /// <summary>
        /// Sends a packet to the specified player detailing skin and player name information of all soecified players on the loading screen.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="player">Player information to send.</param>
        void NotifyLoadScreenPlayerName(int userId, Tuple<uint, ClientInfo> player);
        /// <summary>
        /// Sends a packet to all players who have vision of the specified Minion detailing that it has spawned.
        /// </summary>
        /// <param name="minion">Minion that is spawning.</param>
        /// <param name="team">Unused, to be removed.</param>
        void NotifyMinionSpawned(IMinion m, TeamId team);
        /// <summary>
        /// Sends a packet to either all players with vision (given the projectile is networked to the client) of the projectile, or all players. The packet contains all details regarding the specified projectile's creation.
        /// </summary>
        /// <param name="p">Projectile that was created.</param>
        void NotifyMissileReplication(IProjectile p);
        /// <summary>
        /// Sends a packet to all players that updates the specified unit's model.
        /// </summary>
        /// <param name="obj">AttackableUnit to update.</param>
        void NotifyModelUpdate(IAttackableUnit obj);
        /// <summary>
        /// Sends a packet to the specified player detailing that the specified debug object's radius has changed.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="sender">NetId of the GameObject that is responsible for this packet being sent.</param>
        /// <param name="objID">ID of the Debug Object.</param>
        /// <param name="newRadius">New radius of the Debug Object.</param>
        void NotifyModifyDebugCircleRadius(int userId, uint sender, int objID, float newRadius);
        /// <summary>
        /// Sends a packet to the specified player detailing that the specified Debug Object's color has changed.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="sender">NetId of the GameObject responsible for this packet being sent.</param>
        /// <param name="objID">ID of the Debug Object.</param>
        /// <param name="r">Red hex value of the Debug Object.</param>
        /// <param name="g">Green hex value of the Debug Object.</param>
        /// <param name="b">Blue hex value of the Debug Object.</param>
        void NotifyModifyDebugObjectColor(int userId, uint sender, int objID, byte r, byte g, byte b);
        /// <summary>
        /// Sends a packet to all players detailing that the specified unit has had their shield values modified.
        /// </summary>
        /// <param name="unit">Unit who's shield is being modified.</param>
        /// <param name="amount">Shield amount.</param>
        /// <param name="IsPhysical">Whether or not the shield being modified is of the Physical type.</param>
        /// <param name="IsMagical">Whether or not the shield being modified is of the Magical type.</param>
        /// <param name="StopShieldFade">Whether the shield should stay static or fade.</param>
        void NotifyModifyShield(IAttackableUnit unit, float amount, bool IsPhysical, bool IsMagical, bool StopShieldFade);
        /// <summary>
        /// Sends a packet to all players with vision of the specified Monster that it has spawned.
        /// </summary>
        /// <param name="m">GameObject of type Monster that spawned.</param>
        void NotifyMonsterSpawned(IMonster m);
        /// <summary>
        /// Sends a packet to all players that have vision of the specified GameObject that it has made a movement.
        /// </summary>
        /// <param name="o">GameObject moving.</param>
        /// TODO: Make moving only applicable to ObjAiBase.
        void NotifyMovement(IGameObject o);
        /// <summary>
        /// Sends a packet to all players detailing that the specified attacker unit is starting their next auto attack.
        /// </summary>
        /// <param name="attacker">AttackableUnit auto attacking.</param>
        /// <param name="target">AttackableUnit that is the target of the auto attack.</param>
        /// <param name="futureProjNetId">NetId of the auto attack projectile.</param>
        /// <param name="isCritical">Whether or not the auto attack will crit.</param>
        /// <param name="nextAttackFlag">Whether or not to increase the time to auto attack due to being the next auto attack.</param>
        void NotifyNextAutoAttack(IAttackableUnit attacker, IAttackableUnit target, uint futureProjNetId, bool isCritical, bool nextAttackFlag);
        /// <summary>
        /// Sends a packet to all players who have vision of the specified buff's target detailing that the buff has been added to the target.
        /// </summary>
        /// <param name="b">Buff being added.</param>
        void NotifyNPC_BuffAdd2(IBuff b);
        /// <summary>
        /// Sends a packet to all players with vision of the specified ObjAiBase detailing that the specified group of buffs has been added to the ObjAiBase.
        /// </summary>
        /// <param name="target">ObjAiBase who is receiving the group of buffs.</param>
        /// <param name="buffs">Group of buffs being added to the target.</param>
        /// <param name="buffType">Type of buff that applies to the entire group of buffs.</param>
        /// <param name="buffName">Internal name of the buff that applies to the group of buffs.</param>
        /// <param name="runningTime">Time that has passed since the group of buffs was created.</param>
        /// <param name="duration">Total amount of time the group of buffs should be active.</param>
        void NotifyNPC_BuffAddGroup(IObjAiBase target, List<IBuff> buffs, BuffType buffType, string buffName, float runningTime, float duration);
        /// <summary>
        /// Sends a packet to all players who have vision of the target of the specified buff detailing that the buff was removed from its target.
        /// </summary>
        /// <param name="b">Buff that was removed.</param>
        void NotifyNPC_BuffRemove2(IBuff b);
        /// <summary>
        /// Sends a packet to all players with vision of the specified ObjAiBase detailing that the specified group of buffs is being removed from the ObjAiBase.
        /// </summary>
        /// <param name="target">ObjAiBase getting their group of buffs removed.</param>
        /// <param name="buffs">Group of buffs getting removed.</param>
        /// <param name="buffName">Internal name of the buff that is applicable to the entire group of buffs.</param>
        void NotifyNPC_BuffRemoveGroup(IObjAiBase target, List<IBuff> buffs, string buffName);
        /// <summary>
        /// Sends a packet to all players with vision of the target of the specified buff detailing that the buff previously in the same slot was replaced by the newly specified buff.
        /// </summary>
        /// <param name="b">Buff that will replace the old buff in the same slot.</param>
        void NotifyNPC_BuffReplace(IBuff b);
        /// <summary>
        /// Sends a packet to all players with vision of the specified ObjAiBase detailing that the buffs already occupying the slots of the group of buffs were replaced by the newly specified group of buffs.
        /// </summary>
        /// <param name="target">ObjAiBase getting their group of buffs replaced.</param>
        /// <param name="buffs">Group of buffs replacing buffs in the same slots.</param>
        /// <param name="runningtime">Time since the group of buffs was created.</param>
        /// <param name="duration">Total time the group of buffs should be active.</param>
        void NotifyNPC_BuffReplaceGroup(IObjAiBase target, List<IBuff> buffs, float runningtime, float duration);
        /// <summary>
        /// Sends a packet to all players with vision of the target of the specified buff detailing an update to the number of buffs in the specified buff's slot
        /// </summary>
        /// <param name="b">Buff who's count is being updated.</param>
        /// <param name="duration">Total time the buff should last.</param>
        /// <param name="runningTime">Time since the buff's creation.</param>
        void NotifyNPC_BuffUpdateCount(IBuff b, float duration, float runningTime);
        /// <summary>
        /// Sends a packet to all players with vision of the specified target detailing an update to the number of buffs in each of the buff slots occupied by the specified group of buffs.
        /// </summary>
        /// <param name="target">ObjAiBase who's buffs will be updated.</param>
        /// <param name="buffs">Group of buffs to update.</param>
        /// <param name="duration">Total time the buff should last.</param>
        /// <param name="runningTime">Time since the buff's creation.</param>
        void NotifyNPC_BuffUpdateCountGroup(IObjAiBase target, List<IBuff> buffs, float duration, float runningtime);
        /// <summary>
        /// Sends a packet to all players with vision of the target of the specified buff detailing an update to the stack counter of the specified buff.
        /// </summary>
        /// <param name="b">Buff who's stacks will be updated.</param>
        void NotifyNPC_BuffUpdateNumCounter(IBuff b);
        /// <summary>
        /// Sends a packet to all players with vision of the owner of the specified spell detailing that a spell has been cast.
        /// </summary>
        /// <param name="navGrid">NavigationGrid instance used for networking positional data.</param>
        /// <param name="s">Spell being cast.</param>
        /// <param name="start">Starting position of the spell.</param>
        /// <param name="end">End position of the spell.</param>
        /// <param name="futureProjNetId">NetId of the projectile that may be spawned by the spell.</param>
        void NotifyNPC_CastSpellAns(INavigationGrid navGrid, ISpell s, Vector2 start, Vector2 end, uint futureProjNetId);
        /// <summary>
        /// Sends a packet to all players with vision of the specified AttackableUnit detailing that the attacker has abrubtly stopped their attack (can be a spell or auto attack, although internally AAs are also spells).
        /// </summary>
        /// <param name="attacker">AttackableUnit that stopped their auto attack.</param>
        /// <param name="isSummonerSpell">Whether or not the spell is a summoner spell.</param>
        /// <param name="missileNetID">NetId of the missile that may have been spawned by the spell.</param>
        void NotifyNPC_InstantStopAttack(IAttackableUnit attacker, bool isSummonerSpell, uint missileNetID = 0);
        /// <summary>
        /// Sends a packet to all players detailing that the specified AttackableUnit die has died to the specified AttackableUnit killer.
        /// </summary>
        /// <param name="unit">AttackableUnit that was killed.</param>
        /// <param name="killer">AttackableUnit that killed the unit.</param>
        void NotifyNpcDie(IAttackableUnit unit, IAttackableUnit killer);
        /// <summary>
        /// Sends a packet to all players detailing that the specified attacker has begun trying to attack (has targeted) the specified target. Functionally makes the attacker face the target.
        /// </summary>
        /// <param name="attacker">AttackableUnit that is targeting another unit.</param>
        /// <param name="target">AttackableUnit being targetted by the attacker.</param>
        /// <param name="attackType">Type of attack; RADIAL/MELEE/TARGETED</param>
        void NotifyOnAttack(IAttackableUnit attacker, IAttackableUnit target, AttackType attackType);
        /// <summary>
        /// Sends a packet to all players detailing that the game has paused.
        /// </summary>
        /// <param name="seconds">Amount of time till the pause ends.</param>
        /// <param name="showWindow">Whether or not to show a pause window.</param>
        void NotifyPauseGame(int seconds, bool showWindow);
        /// <summary>
        /// Sends a packet to the specified client's team detailing a map ping.
        /// </summary>
        /// <param name="client">Info of the client that initiated the ping.</param>
        /// <param name="pos">2D top-down position of the ping.</param>
        /// <param name="targetNetId">Target of the ping (if applicable).</param>
        /// <param name="type">Type of ping; COMMAND/ATTACK/DANGER/MISSING/ONMYWAY/FALLBACK/REQUESTHELP. *NOTE*: Not all ping types are supported yet.</param>
        void NotifyPing(ClientInfo client, Vector2 position, int targetNetId, Pings type);
        /// <summary>
        /// Sends a packet to all players detailing the specified client's loading screen progress.
        /// </summary>
        /// <param name="request">Info of the target client given via the client who requested loading screen progress.</param>
        /// <param name="clientInfo">Client info of the client who's progress is being requested.</param>
        void NotifyPingLoadInfo(PingLoadInfoRequest request, ClientInfo clientInfo);
        /// <summary>
        /// Sends a packet to all players detailing the stats (CS, kills, deaths, etc) of the player who owns the specified Champion.
        /// </summary>
        /// <param name="champion">Champion owned by the player.</param>
        void NotifyPlayerStats(IChampion champion);
        /// <summary>
        /// Sends a packet to the specified player which is meant as a response to the players query about the status of the game.
        /// </summary>
        /// <param name="userId">User to send the packet to; player that sent the query.</param>
        void NotifyQueryStatus(int userId);
        /// <summary>
        /// Sends a packet to the specified player detailing that the specified Debug Object has been removed.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="sender">NetId of the GameObject that is responsible for this packet being sent.</param>
        /// <param name="objID">Debug Object being removed.</param>
        void NotifyRemoveDebugObject(int userId, uint sender, int objID);
        /// <summary>
        /// Sends a packet to all players with vision of the specified Champion detailing that item in the specified slot was removed (or the number of stacks of the item in that slot changed).
        /// </summary>
        /// <param name="c">Champion with the items.</param>
        /// <param name="slot">Slot of the item that was removed.</param>
        /// <param name="remaining">Number of stacks of the item left (0 if not applicable).</param>
        void NotifyRemoveItem(IChampion c, byte slot, byte remaining);
        /// <summary>
        /// Sends a packet to the specified player detailing that the highlight of the specified GameObject was removed.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="unit">GameObject that had the highlight.</param>
        void NotifyRemoveUnitHighlight(int userId, IGameObject unit);
        /// <summary>
        /// Sends a packet to all players detailing that the game has been unpaused.
        /// </summary>
        /// <param name="unpauser">Unit that unpaused the game.</param>
        /// <param name="showWindow">Whether or not to show a window before unpausing (delay).</param>
        void NotifyResumeGame(IAttackableUnit unpauser, bool showWindow);
        /// <summary>
        /// Sends a packet to all players detailing that the server has ticked within the specified time delta.
        /// </summary>
        /// <param name="delta">Time it took to tick.</param>
        void NotifyServerTick(float delta);
        /// <summary>
        /// Sends a packet to all players with vision of the specified unit detailing that the unit's animations have been set.
        /// </summary>
        /// <param name="u">AttackableUnit to set the animations of.</param>
        /// <param name="animationPairs">Animations to apply.</param>
        void NotifySetAnimation(IAttackableUnit u, List<string> animationPairs);
        /// <summary>
        /// Sends a packet to all players on the specified team detailing whether the team has become able to surrender.
        /// </summary>
        /// <param name="can">Whether or not the team should be able to surrender.</param>
        /// <param name="team">Team to send the packet to.</param>
        void NotifySetCanSurrender(bool can, TeamId team);
        /// <summary>
        /// Sends a packet to the specified player detailing that their range of movement has changed. This packet forces the game client to only send movement request packets when the distance from the specified center is less than the specified radius.
        /// </summary>
        /// <param name="ai">ObjAiBase that the restriction is being applied to.</param>
        /// <param name="center">Center of the restriction circle.</param>
        /// <param name="radius">Radius of the restriction circle; minimum distance from center required to move.</param>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="restrictCam">Whether or not the player's camera is also restricted to the same area.</param>
        void NotifySetCircularMovementRestriction(IObjAiBase ai, Vector2 center, float radius, int userId, bool restrictCam = false);
        /// <summary>
        /// Sends a packet to the specified player detailing that the specified Debug Object has become hidden.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="sender">NetId that is sending this packet.</param>
        /// <param name="objID">ID of the Debug Object that will be hidden.</param>
        /// <param name="bitfield">Unknown variable.</param>
        void NotifySetDebugHidden(int userId, uint sender, int objID, byte bitfield = 0x0);
        /// <summary>
        /// Sends a packet to all players with vision of the specified attacker detailing that they have targeted the specified target.
        /// </summary>
        /// <param name="attacker">AttackableUnit that is targeting an AttackableUnit.</param>
        /// <param name="target">AttackableUnit that is being targeted by the attacker.</param>
        void NotifySetTarget(IAttackableUnit attacker, IAttackableUnit target);
        /// <summary>
        /// Sends a packet to all players detailing that the specified unit's team has been set.
        /// </summary>
        /// <param name="unit">AttackableUnit who's team has been set.</param>
        void NotifySetTeam(IAttackableUnit unit);
        /// <summary>
        /// Sends a packet to the specified player detailing that they have leveled up the specified skill.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="netId">NetId of the GameObject whos skill is being leveled up.</param>
        /// <param name="skill">Slot of the skill being leveled up.</param>
        /// <param name="level">Current level of the skill.</param>
        /// <param name="pointsLeft">Number of skill points available after the skill has been leveled up.</param>
        void NotifySkillUp(int userId, uint netId, byte skill, byte level, byte pointsLeft);
        /// <summary>
        /// Calls for the appropriate spawn packet to be sent given the specified GameObject's type and calls for a vision packet to be sent for the specified GameObject.
        /// </summary>
        /// <param name="o">GameObject that has spawned.</param>
        /// <param name="team">TeamId to send the packet to; BLUE/PURPLE/NEUTRAL.</param>
        void NotifySpawn(IGameObject o, TeamId team);
        /// <summary>
        /// Sends a packet to the specified player detailing that the spawning (of champions & buildings) that occurs at the start of the game has ended.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        void NotifySpawnEnd(int userId);
        /// <summary>
        /// Sends a packet to the specified player detailing that the game has started the spawning GameObjects that occurs at the start of the game.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        void NotifySpawnStart(int userId);
        /// <summary>
        /// Sends a packet to all players with vision of the specified unit detailing that the unit is playing an animation.
        /// </summary>
        /// <param name="u">Unit that will do the animation.</param>
        /// <param name="animation">Animation that the unit will do.</param>
        void NotifySpellAnimation(IAttackableUnit u, string animation);
        /// <summary>
        /// Sends a packet to the specified player detailing that the GameObject associated with the specified NetID has spawned.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="netId">NetID of the GameObject that has spawned.</param>
        /// TODO: Remove this and replace all usages with NotifyEnterVisibilityClient, refer to the MinionSpawn2 packet as it uses the same packet command.
        void NotifyStaticObjectSpawn(int userId, uint netId);
        /// <summary>
        /// Sends a packet to the specified player detailing the results of server's the version and game info check for the specified player.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="players">List of ClientInfo of all players set to connect to the game.</param>
        /// <param name="version">Version of the player being checked.</param>
        /// <param name="gameMode">String of the internal name of the gamemode being played.</param>
        /// <param name="mapId">ID of the map being played.</param>
        void NotifySynchVersion(int userId, List<Tuple<uint, ClientInfo>> players, string version, string gameMode, int mapId);
        /// <summary>
        /// Sends a packet to the specified player detailing the status (results) of a surrender vote that was called for and ended.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="team">TeamId that called for the surrender vote; BLUE/PURPLE/NEUTRAL.</param>
        /// <param name="reason">SurrenderReason of why the vote ended.</param>
        /// <param name="yesVotes">Number of votes for the surrender.</param>
        /// <param name="noVotes">Number of votes against the surrender.</param>
        void NotifyTeamSurrenderStatus(int userId, TeamId team, SurrenderReason reason, byte yesVotes, byte noVotes);
        /// <summary>
        /// Sends a packet to all players on the same team as the Champion that made the surrender vote detailing what vote was made.
        /// </summary>
        /// <param name="starter">Champion that made the surrender vote.</param>
        /// <param name="open">Whether or not to automatically open the surrender voting menu.</param>
        /// <param name="votedYes">Whether or not voting for the surrender is still available.</param>
        /// <param name="yesVotes">Number of players currently for the surrender.</param>
        /// <param name="noVotes">Number of players currently against the surrender.</param>
        /// <param name="maxVotes">Maximum number of votes possible.</param>
        /// <param name="timeOut">Time until voting becomes unavailable.</param>
        void NotifyTeamSurrenderVote(IChampion starter, bool open, bool votedYes, byte yesVotes, byte noVotes, byte maxVotes, float timeOut);
        /// <summary>
        /// Sends a packet to all players with vision of the specified unit detailing that the unit has teleported to the specified position.
        /// </summary>
        /// <param name="u">AttackableUnit that teleported.</param>
        /// <param name="pos">2D top-down position that the unit teleported to.</param>
        /// TODO: Take into account any movements (waypoints) that should carry over after the teleport.
        void NotifyTeleport(IAttackableUnit u, Vector2 pos);
        /// <summary>
        /// Sends a packet to all players detailing that their screen's tint is shifting to the specified color.
        /// </summary>
        /// <param name="team">TeamID to apply the tint to.</param>
        /// <param name="enable">Whether or not to fade in the tint.</param>
        /// <param name="speed">Amount of time that should pass before tint is fully applied.</param>
        /// <param name="color">Color of the tint.</param>
        void NotifyTint(TeamId team, bool enable, float speed, Color color);
        /// <summary>
        /// Sends a packet to the specified player detailing that the specified LaneTurret has spawned.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="turret">LaneTurret that spawned.</param>
        void NotifyTurretSpawn(int userId, ILaneTurret turret);
        /// <summary>
        /// Sends a packet to all players detailing that the specified event has occurred.
        /// </summary>
        /// <param name="messageId">ID of the event that has occurred. *NOTE*: This enum is incomplete and will be renamed to EventID</param>
        /// <param name="target">Unit that caused the event to occur.</param>
        /// <param name="killer">Optional killer of the unit that caused the event to occur.</param>
        /// <param name="assists">Optional number of assists of the killer.</param>
        /// TODO: Replace this with LeaguePackets, rename UnitAnnounces to EventID, and complete its enum (refer to LeaguePackets.Game.Events.EventID).
        void NotifyUnitAnnounceEvent(UnitAnnounces messageId, IAttackableUnit target, IGameObject killer = null, List<IChampion> assists = null);
        /// <summary>
        /// Sends a packet to the specified player detailing that the specified target GameObject's (debug?) path drawing mode has been set to the specified mode.
        /// </summary>
        /// <param name="userId">User to send the packet to(?).</param>
        /// <param name="unit">Unit that has called for the packet.</param>
        /// <param name="target">GameObject who's (debug?) draw path mode is being set.</param>
        /// <param name="mode">Draw path mode to set.</param>
        /// TODO: Verify the functionality of this packet (and its parameters) and create an enum for the mode.
        void NotifyUnitSetDrawPathMode(int userId, IAttackableUnit unit, IGameObject target, byte mode);
        /// <summary>
        /// Unfinished(?) function which intends to resume the game automatically (without client requests). This is usually called after the pause time has ended in Game.GameLoop.
        /// </summary>
        /// TODO: Verify if this works and if not, then finish it.
        void NotifyUnpauseGame();
        /// <summary>
        /// Sends a packet to all players with vision of the specified unit detailing that the specified unit's stats have been updated.
        /// </summary>
        /// <param name="u">Unit who's stats have been updated.</param>
        /// <param name="partial">Whether or not the packet should be counted as a partial update (whether the stats have actually changed or not). *NOTE*: Use case for this parameter is unknown.</param>
        /// TODO: Replace with LeaguePackets and preferably move all uses of this function to a central EventHandler class (if one is fully implemented).
        void NotifyUpdatedStats(IAttackableUnit u, bool partial = true);
        /// <summary>
        /// Sends a packet to the specified player detailing that their request to view something with their camera has been acknowledged.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="request">ViewRequest housing information about the camera's view.</param>
        /// TODO: Verify if this is the correct implementation.
        void NotifyViewResponse(int userId, ViewRequest request);
    }
}