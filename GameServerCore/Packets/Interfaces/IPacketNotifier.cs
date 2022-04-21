using System;
using System.Collections.Generic;
using System.Numerics;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using GameServerCore.NetInfo;
using GameServerCore.Enums;
using GameServerCore.Packets.Enums;
using GameServerCore.Packets.PacketDefinitions.Requests;
using LeaguePackets;
using LeaguePackets.Game.Common;
using LeaguePackets.Game.Events;

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
        /// Sends a packet to the specified team that a part of the map has changed. Known to be used in League for initializing turret vision and collision.
        /// </summary>
        /// <param name="unitNetId">NetID of the unit owning the region.</param>
        /// <param name="bubbleNetId">NetID of the unit which owns the vision for this region. Functionality unknown.</param>
        /// <param name="team">Team to send the packet to.</param>
        /// <param name="position">2D top-down position of the region.</param>
        /// <param name="time">Amount of time the region lasts.</param>
        /// <param name="radius">Radius of the region.</param>
        /// <param name="regionType">Type of region, possible values unknown.</param>
        /// <param name="clientInfo">Info about a client that might own (or be the target of) the region.</param>
        /// <param name="obj">GameObject that might own (or be the target of) the region.</param>
        /// <param name="collisionRadius">Collision radius for the region (only if it should have collision).</param>
        /// <param name="grassRadius">Radius of the region's grass.</param>
        /// <param name="sizemult">Multiplier that is applied to the radius of the region.</param>
        /// <param name="addsize">Number of units to add to the region's radius.</param>
        /// <param name="grantVis">Whether or not the region should give the region's team vision of enemy units.</param>
        /// <param name="stealthVis">Whether or not invisible units should be visible in the region.</param>
        /// TODO: Implement a Region class so we can easily grab these parameters instead of listing them all in the function.
        void NotifyAddRegion(uint unitNetId, uint bubbleNetId, TeamId team, Vector2 position, float time, float radius = 0, int regionType = 0, ClientInfo clientInfo = null, IGameObject obj = null, float collisionRadius = 0, float grassRadius = 0, float sizemult = 1.0f, float addsize = 0, bool grantVis = true, bool stealthVis = false);
        /// <summary>
        /// Sends a packet to the specified team that a part of the map has changed. Known to be used in League for initializing turret vision and collision.
        /// </summary>
        /// <param name="region">Region to add.</param>
        void NotifyAddRegion(IRegion region);
        /// <summary>
        /// Sends a packet to all players with vision of the specified attacker detailing that they have targeted the specified target.
        /// </summary>
        /// <param name="attacker">AI that is targeting an AttackableUnit.</param>
        /// <param name="target">AttackableUnit that is being targeted by the attacker.</param>
        void NotifyAI_TargetS2C(IObjAiBase attacker, IAttackableUnit target);
        /// <summary>
        /// Sends a packet to all players with vision of the specified attacker detailing that they have targeted the specified champion.
        /// </summary>
        /// <param name="attacker">AI that is targeting a champion.</param>
        /// <param name="target">Champion that is being targeted by the attacker.</param>
        void NotifyAI_TargetHeroS2C(IObjAiBase attacker, IChampion target);
        /// <summary>
        /// Sends a packet to the specified user or all users informing them of the given client's summoner data such as runes, summoner spells, masteries (or talents as named internally), etc.
        /// </summary>
        /// <param name="client">Info about the player's summoner data.</param>
        /// <param name="userId">User to send the packet to. Set to -1 to broadcast.</param>
        void NotifyAvatarInfo(ClientInfo client, int userId = -1);
        /// <summary>
        /// Sends a packet to all players detailing that the specified  unit is starting their next auto attack.
        /// </summary>
        /// <param name="attacker">Unit that is attacking.</param>
        /// <param name="target">AttackableUnit being attacked.</param>
        /// <param name="futureProjNetId">NetId of the auto attack projectile.</param>
        /// <param name="isCrit">Whether or not the auto attack will crit.</param>
        /// <param name="nextAttackFlag">Whether or this basic attack is not the first time this basic attack has been performed on the given target.</param>
        /// TODO: Verify the differences between normal Basic_Attack and Basic_Attack_Pos.
        void NotifyBasic_Attack(IObjAiBase attacker, IAttackableUnit target, uint futureProjNetId, bool isCrit, bool nextAttackFlag);
        /// <summary>
        /// Sends a packet to all players that the specified attacker is starting their first auto attack.
        /// </summary>
        /// <param name="attacker">AI that is starting an auto attack.</param>
        /// <param name="target">AttackableUnit being attacked.</param>
        /// <param name="futureProjNetId">NetID of the projectile that will be created for the auto attack.</param>
        /// <param name="isCrit">Whether or not the auto attack is a critical.</param>
        /// TODO: Verify the differences between BasicAttackPos and normal BasicAttack.
        void NotifyBasic_Attack_Pos(IObjAiBase attacker, IAttackableUnit target, uint futureProjNetId, bool isCrit);
        /// <summary>
        /// Notifies a building, such as towers, inhibs or nexus has died
        /// </summary>
        /// <param name="deathData"></param>
        void NotifyBuilding_Die(IDeathData deathData);
        /// <summary>
        /// Sends a packet to the player attempting to buy an item that their purchase was successful.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="gameObject">GameObject of type ObjAiBase that can buy items.</param>
        /// <param name="itemInstance">Item instance housing all information about the item that has been bought.</param>
        void NotifyBuyItem(int userId, IObjAiBase gameObject, IItem itemInstance);
        /// <summary>
        /// Sends a packet to the specified user detailing that the specified owner unit's spell in the specified slot has been changed.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="owner">Unit that owns the spell being changed.</param>
        /// <param name="slot">Slot of the spell being changed.</param>
        /// <param name="changeType">Type of change being made.</param>
        /// <param name="isSummonerSpell">Whether or not the spell being changed is a summoner spell.</param>
        /// <param name="targetingType">New targeting type to set.</param>
        /// <param name="newName">New internal name of a spell to set.</param>
        /// <param name="newRange">New cast range for the spell to set.</param>
        /// <param name="newMaxCastRange">New max cast range for the spell to set.</param>
        /// <param name="newDisplayRange">New max display range for the spell to set.</param>
        /// <param name="newIconIndex">New index of an icon for the spell to set.</param>
        /// <param name="offsetTargets">New target netids for the spell to set.</param>
        void NotifyChangeSlotSpellData(int userId, IObjAiBase owner, byte slot, GameServerCore.Enums.ChangeSlotSpellDataType changeType, bool isSummonerSpell = false, TargetingType targetingType = TargetingType.Invalid, string newName = "", float newRange = 0, float newMaxCastRange = 0, float newDisplayRange = 0, byte newIconIndex = 0x0, List<uint> offsetTargets = null);
        /// <summary>
        /// Sends a packet to all players with vision of a specified ObjAiBase explaining that their specified spell's cooldown has been set.
        /// </summary>
        /// <param name="u">ObjAiBase who owns the spell going on cooldown.</param>
        /// <param name="slotId">Slot of the spell.</param>
        /// <param name="currentCd">Amount of time the spell has already been on cooldown (if applicable).</param>
        /// <param name="totalCd">Amount of time that the spell should have be in cooldown before going off cooldown.</param>
        /// <param name="userId">UserId to send the packet to. If not specified or zero, the packet is broadcasted to all players that have vision of the specified unit.</param>
        void NotifyCHAR_SetCooldown(IObjAiBase c, byte slotId, float currentCd, float totalCd, int userId = 0);
        /// <summary>
        /// Sends a packet to the specified user that highlights the specified GameObject.
        /// </summary>
        /// <param name="userId">ID of the user to send the packet to.</param>
        /// <param name="unit">GameObject to highlght.</param>
        void NotifyCreateUnitHighlight(int userId, IGameObject unit);
        void NotifyDampenerSwitchStates(IInhibitor inhibitor);
        void NotifyDeath(IDeathData deathData);
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
        void NotifyDestroyClientMissile(ISpellMissile p);
        /// <summary>
        /// Sends a packet to the specified team detailing the destruction of (usually) an auto attack missile.
        /// </summary>
        /// <param name="p">Projectile that is being destroyed.</param>
        /// <param name="team">TeamId to send the packet to.</param>
        void NotifyDestroyClientMissile(ISpellMissile p, TeamId team);
        /// <summary>
        /// Sends a packet to either all players with vision of a target, or the specified player.
        /// The packet displays the specified message of the specified type as floating text over a target.
        /// </summary>
        /// <param name="floatTextData">Contains all the data from a floating text.</param>
        /// <param name="userId">User to send to. 0 = sends to all in vision.</param>
        void NotifyDisplayFloatingText(IFloatingTextData floatTextData, TeamId team = 0, int userId = 0);
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
        /// <param name="userId">UserId to send the packet to. If not specified or zero, the packet is broadcasted to all players that have vision of the specified unit.</param>
        /// <param name="ignoreVision">Optionally ignore vision checks when sending this packet and broadcast it to all players instead.</param>
        void NotifyEnterLocalVisibilityClient(IGameObject o, int userId = 0, bool ignoreVision = false);
        /// <summary>
        /// Sends a packet to either all players with vision of the specified object or the specified user. The packet details the data surrounding the specified GameObject that is required by players when a GameObject enters vision such as items, shields, skin, and movements.
        /// </summary>
        /// <param name="o">GameObject entering vision.</param>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="isChampion">Whether or not the GameObject entering vision is a Champion.</param>
        /// <param name="ignoreVision">Optionally ignore vision checks when sending this packet.</param>
        /// <param name="packets">Takes in a list of packets to send alongside this vision packet.</param>
        /// TODO: Incomplete implementation.
        void NotifyEnterVisibilityClient(IGameObject o, int userId = 0, bool isChampion = false, bool ignoreVision = false, List<GamePacket> packets = null);
        /// <summary>
        /// Sends a packet to all players with vision of the specified unit detailing that the unit has begun facing the specified direction.
        /// </summary>
        /// <param name="obj">GameObject that is changing their orientation.</param>
        /// <param name="direction">3D direction the unit will face.</param>
        /// <param name="isInstant">Whether or not the unit should instantly turn to the direction.</param>
        /// <param name="turnTime">The amount of time (seconds) the turn should take.</param>
        void NotifyFaceDirection(IGameObject obj, Vector3 direction, bool isInstant = true, float turnTime = 0.0833f);
        /// <summary>
        /// Sends a packet to all players that (usually) an auto attack missile has been created.
        /// </summary>
        /// <param name="p">Projectile that was created.</param>
        void NotifyForceCreateMissile(ISpellMissile p);
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
        /// Sends a packet to all players detailing that the game has started. Sent when all players have finished loading.
        /// </summary>
        /// <param name="userId">UserId to send the packet to. If not specified or zero, the packet is broadcasted to all players.</param>
        void NotifyGameStart(int userId = 0);
        /// <summary>
        /// Sends a packet to all players detailing the state (DEAD/ALIVE) of the specified inhibitor.
        /// </summary>
        /// <param name="inhibitor">Inhibitor to check.</param>
        /// <param name="killer">Killer of the inhibitor (if applicable).</param>
        /// <param name="assists">Assists of the killer (if applicable).</param>
        void NotifyInhibitorState(IInhibitor inhibitor, IDeathData deathData = null, List<IChampion> assists = null);
        /// Sends a basic heartbeat packet to either the given player or all players.
        /// </summary>
        void NotifyKeyCheck(int clientID, long playerId, uint version, ulong checkSum = 0, byte action = 0, bool broadcast = false);
        /// <summary>
        /// Sends a packet to all players detailing that the specified LaneMinion has spawned.
        /// </summary>
        /// <param name="m">LaneMinion that spawned.</param>
        /// TODO: Implement wave counter.
        void NotifyLaneMinionSpawned(ILaneMinion m);
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
        /// Sends a packet to either all players or the specified player detailing that the specified GameObject of type LevelProp has spawned.
        /// </summary>
        /// <param name="levelProp">LevelProp that has spawned.</param>
        /// <param name="userId">User to send the packet to.</param>
        void NotifySpawnLevelPropS2C(ILevelProp levelProp, int userId = 0);
        /// <summary>
        /// Sends a packet to the specified player detailing the load screen information.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="players">Client info of all players in the loading screen.</param>
        void NotifyLoadScreenInfo(int userId, List<Tuple<uint, ClientInfo>> players);
        /// <summary>
        /// Optionally sends a packet to all players who have vision of the specified Minion detailing that it has spawned.
        /// </summary>
        /// <param name="minion">Minion that is spawning.</param>
        void NotifyMinionSpawned(IMinion minion);
        /// <summary>
        /// Sends a packet to either all players with vision (given the projectile is networked to the client) of the projectile, or all players. The packet contains all details regarding the specified projectile's creation.
        /// </summary>
        /// <param name="p">Projectile that was created.</param>
        void NotifyMissileReplication(ISpellMissile p);
        void NotifyS2C_CameraBehavior(IChampion target, Vector3 position);
        /// <summary>
        /// Sends a packet to all players that updates the specified unit's model.
        /// </summary>
        /// <param name="obj">AttackableUnit to update.</param>
        /// <param name="userId">UserId to send the packet to. If not specified or zero, the packet is broadcasted to all players that have vision of the specified unit.</param>
        /// <param name="skinID">Unit's skin ID after changing model.</param>
        /// <param name="modelOnly">Wether or not it's only the model that it's being changed(?). I don't really know what's this for</param>
        /// <param name="overrideSpells">Wether or not the user's spells should be overriden, i assume it would be used for things like Nidalee or Elise.</param>
        /// <param name="replaceCharacterPackage">Unknown.</param>
        void NotifyS2C_ChangeCharacterData(IAttackableUnit obj, int userId = 0, uint skinID = 0, bool modelOnly = true, bool overrideSpells = false, bool replaceCharacterPackage = false);
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
        /// Sends a packet to all players detailing the movement driver homing data for the given unit.
        /// Used to sync homing (target-based) dashes between client and server.
        /// </summary>
        /// <param name="unit">Unit to sync.</param>
        void NotifyMovementDriverReplication(IObjAiBase unit);
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
        void NotifyNPC_BuffAddGroup(IAttackableUnit target, List<IBuff> buffs, BuffType buffType, string buffName, float runningTime, float duration);
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
        void NotifyNPC_BuffRemoveGroup(IAttackableUnit target, List<IBuff> buffs, string buffName);
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
        void NotifyNPC_BuffReplaceGroup(IAttackableUnit target, List<IBuff> buffs, float runningtime, float duration);
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
        /// <param name="target">Attackable who's buffs will be updated.</param>
        /// <param name="buffs">Group of buffs to update.</param>
        /// <param name="duration">Total time the buff should last.</param>
        /// <param name="runningTime">Time since the buff's creation.</param>
        void NotifyNPC_BuffUpdateCountGroup(IAttackableUnit target, List<IBuff> buffs, float duration, float runningTime);
        /// <summary>
        /// Sends a packet to all players with vision of the target of the specified buff detailing an update to the stack counter of the specified buff.
        /// </summary>
        /// <param name="b">Buff who's stacks will be updated.</param>
        void NotifyNPC_BuffUpdateNumCounter(IBuff b);
        /// <summary>
        /// Sends a packet to all players with vision of the owner of the specified spell detailing that a spell has been cast.
        /// </summary>
        /// <param name="s">Spell being cast.</param>
        void NotifyNPC_CastSpellAns(ISpell s);
        /// <summary>
        /// Sends a packet to all players detailing that the specified unit has been killed by the specified killer.
        /// </summary>
        /// <param name="data">Data of the death.</param>
        /// TODO: Use this. Seems to be often used when the killer = the attacker.
        void NotifyNPC_Die_Broadcast(IDeathData data);
        /// <summary>
        /// Sends a packet to all players that a champion has died and calls a death timer update packet.
        /// </summary>
        /// <param name="champion">Champion that died.</param>
        /// <param name="killer">Unit that killed the Champion.</param>
        /// <param name="goldFromKill">Amount of gold the killer received.</param>
        void NotifyNPC_Hero_Die(IDeathData deathData);
        /// <summary>
        /// Sends a packet to all players with vision of the specified AttackableUnit detailing that the attacker has abrubtly stopped their attack (can be a spell or auto attack, although internally AAs are also spells).
        /// </summary>
        /// <param name="attacker">AttackableUnit that stopped their auto attack.</param>
        /// <param name="isSummonerSpell">Whether or not the spell is a summoner spell.</param>
        /// <param name="keepAnimating">Whether or not to continue the auto attack animation after the abrupt stop.</param>
        /// <param name="destroyMissile">Whether or not to destroy the missile which may have been created before stopping (client-side removal).</param>
        /// <param name="overrideVisibility">Whether or not stopping this auto attack overrides visibility checks.</param>
        /// <param name="forceClient">Whether or not this packet should be forcibly applied, regardless of if an auto attack is being performed client-side.</param>
        /// <param name="missileNetID">NetId of the missile that may have been spawned by the spell.</param>
        void NotifyNPC_InstantStop_Attack(IAttackableUnit attacker, bool isSummonerSpell, bool keepAnimating = false, bool destroyMissile = true, bool overrideVisibility = true, bool forceClient = false, uint missileNetID = 0);
        /// <summary>
        /// Sends a packet to all players detailing that the specified Champion has leveled up.
        /// </summary>
        /// <param name="c">Champion which leveled up.</param>
        /// <param name="userId">UserId to send the packet to. If not specified or zero, the packet is broadcasted to all players that have vision of the specified unit.</param>
        void NotifyNPC_LevelUp(IObjAiBase c);
        /// <summary>
        /// Sends a packet to the specified user that the spell in the specified slot has been upgraded (skill point added).
        /// </summary>
        /// <param name="userId">ID of the user to send the packet to.</param>
        /// <param name="netId">NetID of the unit that owns the spell being upgraded.</param>
        /// <param name="slot">Slot of the spell being upgraded.</param>
        /// <param name="level">New level of the spell after being upgraded.</param>
        /// <param name="points">New number of points after the upgrade.</param>
        void NotifyNPC_UpgradeSpellAns(int userId, uint netId, byte slot, byte level, byte points);
        /// <summary>
        /// Sends a packet to all users with vision of the given caster detailing that the given spell has been set to auto cast (as well as the spell in the critSlot) for the given caster.
        /// </summary>
        /// <param name="caster">Unit responsible for the autocasting.</param>
        /// <param name="spell">Spell to auto cast.</param>
        /// // TODO: Verify critSlot functionality
        /// <param name="critSlot">Optional spell slot to cast when a crit is going to occur.</param>
        void NotifyNPC_SetAutocast(IObjAiBase caster, ISpell spell, byte critSlot = 0);
        /// <summary>
        /// Sends a packet to the given user detailing that the given spell has been set to auto cast (as well as the spell in the critSlot) for the given caster.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="caster">Unit responsible for the autocasting.</param>
        /// <param name="spell">Spell to auto cast.</param>
        /// // TODO: Verify critSlot functionality
        /// <param name="critSlot">Optional spell slot to cast when a crit is going to occur.</param>
        void NotifyNPC_SetAutocast(int userId, IObjAiBase caster, ISpell spell, byte critSlot = 0);
        /// <summary>
        /// Sends a packet to all players with vision of the specified unit detailing that the specified unit's stats have been updated.
        /// </summary>
        /// <param name="u">Unit who's stats have been updated.</param>
        /// <param name="userId">UserId to send the packet to. If not specified or zero, the packet is broadcasted to all players that have vision of the specified unit.</param>
        /// <param name="partial">Whether or not the packet should be counted as a partial update (whether the stats have actually changed or not). *NOTE*: Use case for this parameter is unknown.</param>
        /// TODO: Replace with LeaguePackets and preferably move all uses of this function to a central EventHandler class (if one is fully implemented).
        void NotifyOnReplication(IAttackableUnit u, int userId = 0, bool partial = true);
        /// <summary>
        /// Sends a packet to all players detailing that the game has paused.
        /// </summary>
        /// <param name="seconds">Amount of time till the pause ends.</param>
        /// <param name="showWindow">Whether or not to show a pause window.</param>
        void NotifyPausePacket(ClientInfo player, int seconds, bool isTournament);
        /// <summary>
        /// Sends a packet to all players detailing the specified client's loading screen progress.
        /// </summary>
        /// <param name="request">Info of the target client given via the client who requested loading screen progress.</param>
        /// <param name="clientInfo">Client info of the client who's progress is being requested.</param>
        void NotifyPingLoadInfo(ClientInfo client, PingLoadInfoRequest request);
        /// <summary>
        /// Sends a packet to all players that a champion has respawned.
        /// </summary>
        /// <param name="c">Champion that respawned.</param>
        void NotifyHeroReincarnateAlive(IChampion c, float parToRestore);
        /// <summary>
        /// Sends a packet to the specified player detailing that the specified Debug Object has been removed.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="sender">NetId of the GameObject that is responsible for this packet being sent.</param>
        /// <param name="objID">Debug Object being removed.</param>
        void NotifyRemoveDebugObject(int userId, uint sender, int objID);
        /// <summary>
        /// Sends a packet to all players with vision of the specified AI detailing that item in the specified slot was removed (or the number of stacks of the item in that slot changed).
        /// </summary>
        /// <param name="ai">AI with the items.</param>
        /// <param name="slot">Slot of the item that was removed.</param>
        /// <param name="remaining">Number of stacks of the item left (0 if not applicable).</param>
        void NotifyRemoveItem(IObjAiBase ai, byte slot, byte remaining);
        /// <summary>
        /// Sends a packet to all players detailing that the specified region was removed.
        /// </summary>
        /// <param name="region">Region to remove.</param>
        void NotifyRemoveRegion(IRegion region);
        /// <summary>
        /// Sends a packet to the specified player detailing that the highlight of the specified GameObject was removed.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="unit">GameObject that had the highlight.</param>
        void NotifyRemoveUnitHighlight(int userId, IGameObject unit);
        /// <summary>
        /// Sends a packet to the specified player detailing skin and player name information of all specified players on the loading screen.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="player">Player information to send.</param>
        void NotifyRequestRename(int userId, Tuple<uint, ClientInfo> player);
        /// <summary>
        /// Sends a packet to the specified player detailing skin information of all specified players on the loading screen.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="player">Player information to send.</param>
        void NotifyRequestReskin(int userId, Tuple<uint, ClientInfo> player);
        /// <summary>
        /// Sends a packet to all players detailing that the game has been unpaused.
        /// </summary>
        /// <param name="unpauser">Unit that unpaused the game.</param>
        /// <param name="showWindow">Whether or not to show a window before unpausing (delay).</param>
        void NotifyResumePacket(IChampion unpauser, ClientInfo player, bool isDelayed);
        void NotifyS2C_ActivateMinionCamp(IMonsterCamp monsterCamp);
        void NotifyS2C_AmmoUpdate(ISpell spell);
        /// <summary>
        /// Sends a packet to all players with vision of the given chain missile that it has updated (unit/position).
        /// </summary>
        /// <param name="p">Missile that should be synced.</param>
        void NotifyS2C_ChainMissileSync(ISpellMissile m);
        /// <summary>
        /// Sends a packet to all players with vision of the given projectile that it has changed targets (unit/position).
        /// </summary>
        /// <param name="p">Projectile that has changed target.</param>
        void NotifyS2C_ChangeMissileTarget(ISpellMissile p);
        /// <summary>
        /// Sends a packet to the specified user or all users detailing that the hero designated to the given clientInfo has been created.
        /// </summary>
        /// <param name="clientInfo">Information about the client which had their hero created.</param>
        /// <param name="userId">User to send the packet to. Set to -1 to broadcast.</param>
        void NotifyS2C_CreateHero(ClientInfo clientInfo, int userId = -1);
        void NotifyS2C_CreateMinionCamp(IMonsterCamp monsterCamp);
        void NotifyS2C_CreateNeutral(IMonster monster, float time);
        /// <summary>
        /// Sends a packet to either all players or the specified player detailing that the specified LaneTurret has spawned.
        /// </summary>
        /// <param name="turret">LaneTurret that spawned.</param>
        /// <param name="userId">User to send the packet to.</param>
        void NotifyS2C_CreateTurret(ILaneTurret turret, int userId = 0);
        /// <summary>
        /// Disables the UI for ther end of the game
        /// </summary>
        /// <param name="player">Player for the UI to be disabled</param>
        void NotifyS2C_DisableHUDForEndOfGame(Tuple<uint, ClientInfo> player);
        /// <summary>
        /// Sends packets to all players notifying the result of a match (Victory or defeat)
        /// </summary>
        /// <param name="losingTeam">The Team that lost the match</param>
        /// <param name="time">The offset for the result to actually be displayed</param>
        void NotifyS2C_EndGame(TeamId losingTeam);
        void NotifyS2C_HandleCapturePointUpdate(byte capturePointIndex, uint otherNetId, byte PARType, byte attackTeam, CapturePointUpdateCommand capturePointUpdateCommand);
        /// <summary>
        /// Notifies the game about a map score
        /// </summary>
        /// <param name="team"></param>
        /// <param name="score"></param>
        void NotifyS2C_HandleGameScore(TeamId team, int score);
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
        void NotifyS2C_HandleTipUpdatep(int userId, string title, string text, string imagePath, byte tipCommand, uint playerNetId, uint targetNetId);
        /// <summary>
        /// Sends a packet to all players detailing the stats (CS, kills, deaths, etc) of the player who owns the specified Champion.
        /// </summary>
        /// <param name="champion">Champion owned by the player.</param>
        void NotifyS2C_HeroStats(IChampion champion);
        void NotifyS2C_IncrementPlayerScore(IScoreData scoreData);
        /// <summary>
        /// Sends a packet to the specified client's team detailing a map ping.
        /// </summary>
        /// <param name="client">Info of the client that initiated the ping.</param>
        /// <param name="pos">2D top-down position of the ping.</param>
        /// <param name="targetNetId">Target of the ping (if applicable).</param>
        /// <param name="type">Type of ping; COMMAND/ATTACK/DANGER/MISSING/ONMYWAY/FALLBACK/REQUESTHELP. *NOTE*: Not all ping types are supported yet.</param>
        void NotifyS2C_MapPing(Vector2 pos, Pings type, uint targetNetId = 0, ClientInfo client = null);
        /// <summary>
        /// Notifies the camera of a given player to move
        /// </summary>
        /// <param name="player">Player who'll it's camera moved</param>
        /// <param name="startPosition">The starting position of the camera (Not yet known how to get it's values)</param>
        /// <param name="endPosition">End point to where the camera will move</param>
        /// <param name="travelTime">The time the camera will have to travel the given distance</param>
        /// <param name="startFromCurretPosition">Wheter or not it starts from current position</param>
        /// <param name="unlockCamera">Whether or not the camera is unlocked</param>
        void NotifyS2C_MoveCameraToPoint(Tuple<uint, ClientInfo> player, Vector3 startPosition, Vector3 endPosition, float travelTime = 0, bool startFromCurretPosition = true, bool unlockCamera = false);
        void NotifyS2C_Neutral_Camp_Empty(IMonsterCamp monsterCamp, IDeathData deathData = null);
        /// <summary>
        /// Sends a packet to all players detailing that the specified unit has been killed by the specified killer.
        /// </summary>
        /// <param name="data">Data of the death.</param>
        void NotifyS2C_NPC_Die_MapView(IDeathData data);
        /// <summary>
        /// Sends a packet to either all players with vision of the specified GameObject or a specified user.
        /// The packet contains details of which team gained visibility of the GameObject and is meant for after it is first initialized into vision.
        /// </summary>
        /// <param name="o">GameObject coming into vision.</param>
        /// <param name="userId">User to send the packet to.</param>
        void NotifyS2C_OnEnterTeamVisibility(IGameObject o, TeamId team, int userId = 0);
        /// <summary>
        /// Sends a packet to all players that announces a specified message (ex: "Minions have spawned.")
        /// </summary>
        /// <param name="eventId">Id of the event to happen.</param>
        /// <param name="sourceNetID">Not yet know it's use.</param>
        void NotifyS2C_OnEventWorld(IEvent mapEvent, uint sourceNetId = 0);
        /// <summary>
        /// Sends a packet to either all players with vision of the specified GameObject or a specified user.
        /// The packet contains details of which team lost visibility of the GameObject and should only be used after it is first initialized into vision (NotifyEnterVisibility).
        /// </summary>
        /// <param name="o">GameObject going out of vision.</param>
        /// <param name="userId">User to send the packet to.</param>
        void NotifyS2C_OnLeaveTeamVisibility(IGameObject o, TeamId team, int userId = 0);
        /// <summary>
        /// Sends a packet to all players detailing that the specified object's current animations have been paused/unpaused.
        /// </summary>
        /// <param name="obj">GameObject that is playing the animation.</param>
        /// <param name="pause">Whether or not to pause/unpause animations.</param>
        void NotifyS2C_PauseAnimation(IGameObject obj, bool pause);
        /// <summary>
        /// Sends a packet to all players with vision of the specified object detailing that it is playing the specified animation.
        /// </summary>
        /// <param name="obj">GameObject that is playing the animation.</param>
        /// <param name="animation">Internal name of the animation to play.</param>
        /// TODO: Implement AnimationFlags enum for this and fill it in.
        /// <param name="flags">Animation flags. Refer to AnimationFlags enum.</param>
        /// <param name="timeScale">How fast the animation should play. Default 1x speed.</param>
        /// <param name="startTime">Time in the animation to start at.</param>
        /// TODO: Verify if this description is correct, if not, correct it.
        /// <param name="speedScale">How much the speed of the GameObject should affect the animation.</param>
        void NotifyS2C_PlayAnimation(IGameObject obj, string animation, AnimationFlags flags = 0, float timeScale = 1.0f, float startTime = 0.0f, float speedScale = 1.0f);
        /// <summary>
        /// Sends a packet to all players detailing an emotion that is being performed by the unit that owns the specified netId.
        /// </summary>
        /// <param name="type">Type of emotion being performed; DANCE/TAUNT/LAUGH/JOKE/UNK.</param>
        /// <param name="netId">NetID of the unit performing the emotion.</param>
        void NotifyS2C_PlayEmote(Emotions type, uint netId);
        void NotifyS2C_PlaySound(string soundName, IAttackableUnit soundOwner);
        /// <summary>
        /// Sends a packet to the specified player which is meant as a response to the players query about the status of the game.
        /// </summary>
        /// <param name="userId">User to send the packet to; player that sent the query.</param>
        void NotifyS2C_QueryStatusAns(int userId);
        /// <summary>
        /// Sends a packet to all players with vision of the specified unit detailing that its animation states have changed to the specified animation pairs.
        /// Replaces the unit's normal animation behaviors with the given animation pairs. Structure of the animationPairs is expected to follow the same structure from before the replacement.
        /// </summary>
        /// <param name="u">AttackableUnit to change.</param>
        /// <param name="animationPairs">Dictionary of animations to set.</param>
        void NotifyS2C_SetAnimStates(IAttackableUnit u, Dictionary<string, string> animationPairs);
        /// <summary>
        /// Sets if your screen will be grey or not when dead (used in the end of a game)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="enabled"></param>
        void NotifyS2C_SetGreyscaleEnabledWhenDead(bool enabled, IAttackableUnit sender = null);
        /// <summary>
        /// Sends a packet to the specified user detailing that the spell in the given slot has had its spelldata changed to the spelldata of the given spell name.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="netId">NetId of the unit that owns the spell being changed.</param>
        /// <param name="spellName">Internal name of the spell to grab spell data from (to set).</param>
        /// <param name="slot">Slot of the spell being changed.</param>
        void NotifyS2C_SetSpellData(int userId, uint netId, string spellName, byte slot);
        /// <summary>
        /// Sends a packet to the specified player detailing that the level of the spell in the given slot has changed.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="netId">NetId of the unit that owns the spell being changed.</param>
        /// <param name="slot">Slot of the spell being changed.</param>
        /// <param name="level">New level of the spell to set.</param>
        void NotifyS2C_SetSpellLevel(int userId, uint netId, int slot, int level);
        /// <summary>
        /// Sends a packet to the specified player detailing that the game has started the spawning GameObjects that occurs at the start of the game.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        void NotifyS2C_StartSpawn(int userId);
        /// <summary>
        /// Sends a packet to all players detailing that the specified object has stopped playing an animation.
        /// </summary>
        /// <param name="obj">GameObject that is playing the animation.</param>
        /// <param name="animation">Internal name of the animation to stop.</param>
        /// <param name="stopAll">Whether or not to stop all animations. Only works if animation is empty/null.</param>
        /// <param name="fade">Whether or not the animation should fade before stopping.</param>
        /// <param name="ignoreLock">Whether or not locked animations should still be stopped.</param>
        void NotifyS2C_StopAnimation(IGameObject obj, string animation, bool stopAll = false, bool fade = false, bool ignoreLock = true);
        /// <summary>
        /// Sends a packet to all players detailing a debug message.
        /// </summary>
        /// <param name="htmlDebugMessage">Debug message to send.</param>
        void NotifyS2C_SystemMessage(string htmlDebugMessage);
        /// <summary>
        /// Sends a packet to the specified user detailing a debug message.
        /// </summary>
        /// <param name="userId">ID of the user to send the packet to.</param>
        /// <param name="message">Debug message to send.</param>
        void NotifyS2C_SystemMessage(int userId, string message);
        /// <summary>
        /// Sends a packet to the specified team detailing a debug message.
        /// </summary>
        /// <param name="team">TeamId to send the packet to; BLUE/PURPLE/NEUTRAL.</param>
        /// <param name="message">Debug message to send.</param>
        void NotifyS2C_SystemMessage(TeamId team, string message);
        /// <summary>
        /// Sends a packet to the given user detailing that the specified input locking flags have been toggled.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="flags">InputLockFlags to toggle.</param>
        void NotifyS2C_ToggleInputLockFlag(int userId, InputLockFlags flags);
        /// <summary>
        /// Sends a packet to all players detailing spell tooltip parameters that the game does not inform automatically.
        /// </summary>
        /// <param name="data">The list of changed tool tip values.</param>
        void NotifyS2C_ToolTipVars(List<IToolTipData> data);
        /// <summary>
        /// Sends a packet to all players with vision of the specified attacker that it it looking at the specified attacked unit with the given AttackType.
        /// </summary>
        /// <param name="attacker">Unit that is attacking.</param>
        /// <param name="attacked">Unit that is being attacked.</param>
        /// <param name="attackType">AttackType that the attacker is using to attack.</param>
        void NotifyS2C_UnitSetLookAt(IAttackableUnit attacker, IAttackableUnit attacked, AttackType attackType);
        void NotifyS2C_UnitSetMinimapIcon(IAttackableUnit unit, string iconCategory = "", bool changeIcon = false, string borderCategory = "", bool changeBorder = false, string borderScriptName = "");
        void NotifyS2C_UpdateAscended(IObjAiBase ascendant = null);
        /// <summary>
        /// Sends a packet to all players detailing the attack speed cap overrides for this game.
        /// </summary>
        /// <param name="overrideMax">Whether or not to override the maximum attack speed cap.</param>
        /// <param name="maxAttackSpeedOverride">Value to override the maximum attack speed cap.</param>
        /// <param name="overrideMin">Whether or not to override the minimum attack speed cap.</param>
        /// <param name="minAttackSpeedOverride">Value to override the minimum attack speed cap.</param>
        void NotifyS2C_UpdateAttackSpeedCapOverrides(bool overrideMax, float maxAttackSpeedOverride, bool overrideMin, float minAttackSpeedOverride, IAttackableUnit unit = null);
        /// <summary>
        /// Sends a packet to all players with vision of the given bounce missile that it has updated (unit/position).
        /// </summary>
        /// <param name="p">Missile that has been updated.</param>
        void NotifyS2C_UpdateBounceMissile(ISpellMissile p);
        /// <summary>
        /// Sends a packet to all players updating a player's death timer.
        /// </summary>
        /// <param name="champion">Champion that died.</param>
        void NotifyS2C_UpdateDeathTimer(IChampion champion);
        /// <summary>
        /// Sends a packet to the specified user detailing that the specified spell's toggle state has been updated.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="s">Spell being updated.</param>
        void NotifyS2C_UpdateSpellToggle(int userId, ISpell s);
        /// <summary>
        /// Sends a packet to all players detailing that the server has ticked within the specified time delta.
        /// </summary>
        /// <param name="delta">Time it took to tick.</param>
        void NotifyServerTick(float delta);
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
        /// Sends a packet to all players detailing that the specified unit's team has been set.
        /// </summary>
        /// <param name="unit">AttackableUnit who's team has been set.</param>
        void NotifySetTeam(IAttackableUnit unit);
        /// <summary>
        /// Calls for the appropriate spawn packet to be sent given the specified GameObject's type and calls for a vision packet to be sent for the specified GameObject.
        /// </summary>
        /// <param name="o">GameObject that has spawned.</param>
        /// <param name="team">The team the user belongs to.</param>
        /// <param name="userId">UserId to send the packet to.</param>
        /// <param name="gameTime">Time elapsed since the start of the game</param>
        /// <param name="doVision">Whether or not to package the packets into a vision packet.</param>
        void NotifySpawn(IGameObject obj, TeamId team, int userId, float gameTime, bool doVision = true);
        /// <summary>
        /// Sends a packet to the specified player detailing that the spawning (of champions & buildings) that occurs at the start of the game has ended.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        void NotifySpawnEnd(int userId);
        /// <summary>
        /// Sends a packet to all players with vision of the specified Champion detailing that the Champion's items have been swapped.
        /// </summary>
        /// <param name="c">Champion who swapped their items.</param>
        /// <param name="fromSlot">Slot the item was previously in.</param>
        /// <param name="toSlot">Slot the item was swapped to.</param>
        void NotifySwapItemAns(IChampion c, byte fromSlot, byte toSlot);
        /// <summary>
        /// Sends a packet to the specified player detailing the amount of time since the game started (in seconds). Used to initialize the user's in-game timer.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="time">Time since the game started (in milliseconds).</param>
        void NotifySyncMissionStartTimeS2C(int userId, float time);
        /// <summary>
        /// Sends a packet to all players detailing the amount of time since the game started (in seconds).
        /// </summary>
        /// <param name="gameTime">Time since the game started (in milliseconds).</param>
        void NotifySynchSimTimeS2C(float gameTime);
        /// <summary>
        /// Sends a packet to the specified player detailing the amount of time since the game started (in seconds).
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="time">Time since the game started (in milliseconds).</param>
        void NotifySynchSimTimeS2C(int userId, float time);
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
        /// Sends a packet to all players with vision of the given unit detailing that it has teleported to the given position.
        /// </summary>
        /// <param name="unit">Unit that has teleported.</param>
        /// <param name="position">Position the unit teleported to.</param>
        void NotifyTeleport(IAttackableUnit unit, Vector2 position);
        /// <summary>
        /// Sends a packet to all players detailing that their screen's tint is shifting to the specified color.
        /// </summary>
        /// <param name="team">TeamID to apply the tint to.</param>
        /// <param name="enable">Whether or not to fade in the tint.</param>
        /// <param name="speed">Amount of time that should pass before tint is fully applied.</param>
        /// <param name="color">Color of the tint.</param>
        void NotifyTint(TeamId team, bool enable, float speed, Content.Color color);
        /// <summary>
        /// Sends a packet to all players that the specified Champion has gained the specified amount of experience.
        /// </summary>
        /// <param name="champion">Champion that gained the experience.</param>
        /// <param name="experience">Amount of experience gained.</param>
        void NotifyUnitAddEXP(IChampion champion, float experience);
        /// <summary>
        /// Sends a packet to all players that the specified Champion has killed a specified player and received a specified amount of gold.
        /// </summary>
        /// <param name="c">Champion that killed a unit.</param>
        /// <param name="died">AttackableUnit that died to the Champion.</param>
        /// <param name="gold">Amount of gold the Champion gained for the kill.</param>
        /// TODO: Only use BroadcastPacket when the unit that died is a Champion.
        void NotifyUnitAddGold(IChampion c, IAttackableUnit died, float gold);
        /// <summary>
        /// Sends a packet to optionally all players (given isGlobal), a specified user that is the source of damage, or a specified user that is receiving the damage. The packet details an instance of damage being applied to a unit by another unit.
        /// </summary>
        /// <param name="source">Unit which caused the damage.</param>
        /// <param name="target">Unit which is taking the damage.</param>
        /// <param name="amount">Amount of damage dealt to the target (usually the end result of all damage calculations).</param>
        /// <param name="type">Type of damage being dealt; PHYSICAL/MAGICAL/TRUE</param>
        /// <param name="damagetext">Type of text to show above the target; refer to DamageResultType enum.</param>
        /// <param name="isGlobal">Whether or not the packet should be sent to all players.</param>
        /// <param name="sourceId">ID of the user who dealt the damage that should receive the packet.</param>
        /// <param name="targetId">ID of the user who is taking the damage that should receive the packet.</param>
        void NotifyUnitApplyDamage(IAttackableUnit source, IAttackableUnit target, float amount, DamageType type, DamageResultType damagetext, bool isGlobal = true, int sourceId = 0, int targetId = 0);
        /// <summary>
        /// Sends a packet to the specified player detailing that the specified target GameObject's (debug?) path drawing mode has been set to the specified mode.
        /// </summary>
        /// <param name="userId">User to send the packet to(?).</param>
        /// <param name="unit">Unit that has called for the packet.</param>
        /// <param name="target">GameObject who's (debug?) draw path mode is being set.</param>
        /// <param name="mode">Draw path mode to set. Refer to DrawPathMode enum.</param>
        /// TODO: Verify the functionality of this packet (and its parameters) and create an enum for the mode.
        void NotifyUnitSetDrawPathMode(int userId, IAttackableUnit unit, IGameObject target, DrawPathMode mode);
        void NotifyUpdateLevelPropS2C(UpdateLevelPropData propData);
        /// <summary>
        /// Sends a packet to the player attempting to use an item that the item was used successfully.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="ai">GameObject of type ObjAiBase that can buy items.</param>
        /// <param name="itemInstance">Item instance housing all information about the item that has been used.</param>
        void NotifyUseItemAns(int userId, IObjAiBase ai, IItem itemInstance);
        /// <summary>
        /// Sends a packet to the specified team detailing that an object's visibility has changed.
        /// General function which will send the needed vision packet for the specific object type.
        /// </summary>
        /// <param name="obj">GameObject which had their visibility changed.</param>
        /// <param name="team">Team which is affected by this visibility change.</param>
        /// <param name="becameVisible">Whether or not the change was an entry into vision.</param>
        /// <param name="userId">UserId to send the packet to. If not specified or zero, the packet is broadcasted to the team.</param>
        void NotifyVisibilityChange(IGameObject obj, TeamId team, bool becameVisible, int userId = 0);
        /// <summary>
        /// Sends a packet to all players that have vision of the specified unit that it has made a movement.
        /// </summary>
        /// <param name="u">AttackableUnit that is moving.</param>
        /// <param name="userId">UserId to send the packet to. If not specified or zero, the packet is broadcasted to all players that have vision of the specified unit.</param>
        /// <param name="useTeleportID">Whether or not to teleport the unit to its current position in its path.</param>
        void NotifyWaypointGroup(IAttackableUnit u, int userId = 0, bool useTeleportID = false);
        /// <summary>
        /// Sends a packet to all players that have vision of the specified unit.
        /// The packet details a group of waypoints with speed parameters which determine what kind of movement will be done to reach the waypoints, or optionally a GameObject.
        /// Functionally referred to as a dash in-game.
        /// </summary>
        /// <param name="u">Unit that is dashing.</param>
        void NotifyWaypointGroupWithSpeed(IAttackableUnit u);
        /// <summary>
        /// Sends a packet to all players with vision of the given unit detailing its waypoints.
        /// </summary>
        /// <param name="unit">Unit to send.</param>
        void NotifyWaypointList(IAttackableUnit unit);
        /// <summary>
        /// Sends a packet to all players with vision of the given GameObject detailing its waypoints.
        /// </summary>
        /// <param name="obj">GameObject to send.</param>
        void NotifyWaypointList(IGameObject obj, List<Vector2> waypoints);
        /// <summary>
        /// Sends a packet to all players that have vision of the specified unit.
        /// The packet details a list of waypoints with speed parameters which determine what kind of movement will be done to reach the waypoints, or optionally a GameObject.
        /// Functionally referred to as a dash in-game.
        /// </summary>
        /// <param name="u">Unit that is dashing.</param>
        /// <param name="dashSpeed">Constant speed that the unit will have during the dash.</param>
        /// <param name="leapGravity">Optionally how much gravity the unit will experience when above the ground while dashing.</param>
        /// <param name="keepFacingLastDirection">Optionally whether or not the unit should maintain the direction they were facing before dashing.</param>
        /// <param name="target">Optional GameObject to follow.</param>
        /// <param name="followTargetMaxDistance">Optional maximum distance the unit will follow the Target before stopping the dash or reaching to the Target.</param>
        /// <param name="backDistance">Optional unknown parameter.</param>
        /// <param name="travelTime">Optional total time the dash will follow the GameObject before stopping or reaching the Target.</param>
        /// TODO: Implement ForceMovement class which houses these parameters, then have that as the only parameter to this function (and other Dash-based functions).
        void NotifyWaypointListWithSpeed
        (
            IAttackableUnit u,
            float dashSpeed,
            float leapGravity = 0,
            bool keepFacingLastDirection = false,
            IGameObject target = null,
            float followTargetMaxDistance = 0,
            float backDistance = 0,
            float travelTime = 0
        );
        /// <summary>
        /// Sends a packet to the specified player detailing that their request to view something with their camera has been acknowledged.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="request">ViewRequest housing information about the camera's view.</param>
        /// TODO: Verify if this is the correct implementation.
        /// TODO: Fix LeaguePackets Typos.
        void NotifyWorld_SendCamera_Server_Acknologment(ClientInfo client, ViewRequest request);
    }
}
