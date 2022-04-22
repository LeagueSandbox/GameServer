using LENet;
using GameServerCore;
using GameServerCore.Content;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using GameServerCore.Enums;
using GameServerCore.NetInfo;
using GameServerCore.Packets.Interfaces;
using LeaguePackets.Game;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Timers;
using PingLoadInfoRequest = GameServerCore.Packets.PacketDefinitions.Requests.PingLoadInfoRequest;
using ViewRequest = GameServerCore.Packets.PacketDefinitions.Requests.ViewRequest;
using LeaguePackets.Game.Common;
using LeaguePackets.Common;
using static GameServerCore.Content.HashFunctions;
using System.Text;
using Force.Crc32;
using System.Linq;
using LeaguePackets;
using LeaguePackets.LoadScreen;
using LeaguePackets.Game.Events;
using Channel = GameServerCore.Packets.Enums.Channel;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420
{
    /// <summary>
    /// Class containing all function related packets (except handshake) which are sent by the server to game clients.
    /// </summary>
    public class PacketNotifier : IPacketNotifier
    {
        private readonly IPacketHandlerManager _packetHandlerManager;
        private readonly INavigationGrid _navGrid;

        /// <summary>
        /// Instantiation which preps PacketNotifier for packet sending.
        /// </summary>
        /// <param name="packetHandlerManager"></param>
        /// <param name="navGrid"></param>
        public PacketNotifier(IPacketHandlerManager packetHandlerManager, INavigationGrid navGrid)
        {
            _packetHandlerManager = packetHandlerManager;
            _navGrid = navGrid;
        }

        AddRegion ConstructAddRegionPacket(IRegion region)
        {
            var regionPacket = new AddRegion
            {
                TeamID = (uint)region.Team,
                // TODO: Find out what values this can be and make an enum for it (so far: -2 & -1 for turrets)
                RegionType = region.Type,
                ClientID = region.OwnerClientID,
                // TODO: Verify (usually 0 for vision only?)
                UnitNetID = 0,
                // TODO: Verify (is usually different from UnitNetID in packets, may also be a remnant or for internal use)
                BubbleNetID = region.VisionNetID,
                VisionTargetNetID = region.VisionBindNetID,
                Position = region.Position,
                // For turrets, usually 25000.0 is used
                TimeToLive = region.Lifetime,
                // 88.4 for turrets
                ColisionRadius = region.PathfindingRadius,
                // 130.0 for turrets
                GrassRadius = region.GrassRadius,
                SizeMultiplier = region.Scale,
                SizeAdditive = region.AdditionalSize,

                HasCollision = region.HasCollision,
                GrantVision = region.GrantVision,
                RevealStealth = region.RevealsStealth,

                BaseRadius = region.VisionRadius // 800.0 for turrets
            };

            if (region.CollisionUnit != null)
            {
                regionPacket.UnitNetID = region.CollisionUnit.NetId;
            }

            return regionPacket;
        }

        S2C_CreateNeutral ConstructCreateNeutralPacket(IMonster monster, float time)
        {
            return new S2C_CreateNeutral
            {
                SenderNetID = monster.NetId,
                UniqueName = monster.Name,
                Name = monster.Name,
                SkinName = monster.Model,
                FaceDirectionPosition = monster.Direction,
                DamageBonus = monster.DamageBonus,
                HealthBonus = monster.HealthBonus,
                InitialLevel = monster.InitialLevel,
                NetID = monster.NetId,
                GroupPosition = monster.Camp.Position,
                BuffSideTeamID = monster.Camp.SideTeamId,
                Position = new Vector3(monster.Position.X, monster.GetHeight(), monster.Position.Y),
                SpawnAnimationName = monster.SpawnAnimation,
                AIscript = "",
                //Seems to be the time it is supposed to spawn, not the time when it spawned, check this later
                SpawnTime = time / 1000,
                BehaviorTree = monster.AIScript.AIScriptMetaData.BehaviorTree,
                RevealEvent = monster.Camp.RevealEvent,
                GroupNumber = monster.Camp.CampIndex,
                MinionRoamState = monster.AIScript.AIScriptMetaData.MinionRoamState,
                SpawnDuration = monster.Camp.SpawnDuration,
                TeamID = (uint)monster.Team,
                NetNodeID = (byte)NetNodeID.Spawned
            };
        }

        S2C_CreateTurret ConstructCreateTurretPacket(ILaneTurret turret)
        {
            var createTurret = new S2C_CreateTurret
            {
                SenderNetID = turret.NetId,
                NetID = turret.NetId,
                // Verify, taken from packets (does not seem to change)
                NetNodeID = 64,
                Name = turret.Name,
                IsTargetable = turret.Stats.IsTargetable,
                IsTargetableToTeamSpellFlags = (uint)turret.Stats.IsTargetableToTeam
            };
            return createTurret;
        }

        OnEnterVisibilityClient ConstructEnterVisibilityClientPacket(IGameObject o, bool isChampion = false, List<GamePacket> packets = null)
        {
            var itemDataList = new List<ItemData>();
            var shields = new ShieldValues(); //TODO: Implement shields so this can be finished

            var charStackDataList = new List<CharacterStackData>();
            var charStackData = new CharacterStackData
            {
                SkinID = 0,
                OverrideSpells = false,
                ModelOnly = false,
                ReplaceCharacterPackage = false,
                ID = 0
            };

            var buffCountList = new List<KeyValuePair<byte, int>>();

            if (o is IAttackableUnit a)
            {
                charStackData.SkinName = a.Model;

                if (a is IObjAiBase obj)
                {
                    charStackData.SkinID = (uint)obj.SkinID;
                    if (obj.Inventory != null)
                    {
                        foreach (var item in obj.Inventory.GetAllItems())
                        {
                            var itemData = item.ItemData;
                            itemDataList.Add(new ItemData
                            {
                                ItemID = (uint)itemData.ItemId,
                                ItemsInSlot = (byte)item.StackCount,
                                Slot = obj.Inventory.GetItemSlot(item),
                                //Unhardcode this when spell ammo gets introduced
                                SpellCharges = 0
                            });
                        }
                    }
                }
                buffCountList = new List<KeyValuePair<byte, int>>();
                var tempBuffs = a.GetParentBuffs();

                for (var i = 0; i < tempBuffs.Count; i++)
                {
                    var buff = tempBuffs.ElementAt(i).Value;
                    buffCountList.Add(new KeyValuePair<byte, int>(buff.Slot, buff.StackCount));
                }

                // TODO: if (a.IsDashing), requires SpeedParams, add it to AttackableUnit so it can be accessed outside of initialization
            }

            charStackDataList.Add(charStackData);

            var type = MovementDataType.Normal;
            SpeedParams speeds = null;

            if (o is IAttackableUnit u)
            {
                if (u.Waypoints.Count <= 1)
                {
                    type = MovementDataType.Stop;
                }

                if (u.MovementParameters != null)
                {
                    type = MovementDataType.WithSpeed;

                    speeds = new SpeedParams
                    {
                        PathSpeedOverride = u.MovementParameters.PathSpeedOverride,
                        ParabolicGravity = u.MovementParameters.ParabolicGravity,
                        // TODO: Implement as parameter (ex: Aatrox Q).
                        ParabolicStartPoint = u.MovementParameters.ParabolicStartPoint,
                        Facing = u.MovementParameters.KeepFacingDirection,
                        FollowNetID = u.MovementParameters.FollowNetID,
                        FollowDistance = u.MovementParameters.FollowDistance,
                        FollowBackDistance = u.MovementParameters.FollowBackDistance,
                        FollowTravelTime = u.MovementParameters.FollowTravelTime
                    };
                }
            }

            var md = PacketExtensions.CreateMovementData(o, _navGrid, type, speeds, useTeleportID: true);

            var enterVis = new OnEnterVisibilityClient
            {
                SenderNetID = o.NetId,
                Items = itemDataList,
                ShieldValues = shields,
                CharacterDataStack = charStackDataList,
                BuffCount = buffCountList,
                LookAtPosition = new Vector3(1, 0, 0),
                // TODO: Verify
                IsHero = isChampion,
                MovementData = md
            };

            if (packets != null)
            {
                enterVis.Packets = packets;
            }

            return enterVis;
        }

        FX_Create_Group ConstructFXCreateGroupPacket(IParticle particle)
        {
            uint bindNetID = 0;
            uint targetNetID = 0;

            if (particle.BindObject != null)
            {
                bindNetID = particle.BindObject.NetId;
            }
            if (particle.TargetObject != null)
            {
                targetNetID = particle.TargetObject.NetId;
            }

            var position = particle.GetPosition3D();

            var ownerPos = position;
            if (particle.Caster != null)
            {
                ownerPos = particle.Caster.GetPosition3D();
            }

            var fxPacket = new FX_Create_Group();
            var fxDataList = new List<FXCreateData>();

            var targetPos = particle.StartPosition;
            if (particle.BindObject == null && particle.TargetObject == null)
            {
                targetPos = particle.EndPosition;
            }

            var targetHeight = _navGrid.GetHeightAtLocation(particle.StartPosition.X, particle.StartPosition.Y);
            var higherValue = Math.Max(targetHeight, particle.GetHeight());

            // TODO: implement option for multiple particles instead of hardcoding one
            var fxData1 = new FXCreateData
            {
                NetAssignedNetID = particle.NetId,
                CasterNetID = 0,
                KeywordNetID = 0, // Not sure what this is

                PositionX = (short)((position.X - _navGrid.MapWidth / 2) / 2),
                PositionY = higherValue,
                PositionZ = (short)((position.Z - _navGrid.MapHeight / 2) / 2),

                TargetPositionX = (short)((targetPos.X - _navGrid.MapWidth / 2) / 2),
                TargetPositionY = higherValue,
                TargetPositionZ = (short)((targetPos.Y - _navGrid.MapHeight / 2) / 2),

                OwnerPositionX = (short)((ownerPos.X - _navGrid.MapWidth / 2) / 2),
                OwnerPositionY = ownerPos.Y,
                OwnerPositionZ = (short)((ownerPos.Z - _navGrid.MapHeight / 2) / 2),

                TimeSpent = particle.GetTimeAlive(),
                ScriptScale = particle.Scale,
                TargetNetID = targetNetID,
                BindNetID = bindNetID
            };

            // TODO: Verify if there is more to this.
            if (particle.FollowsGroundTilt)
            {
                fxData1.TargetPositionY = targetHeight;
            }

            if (particle.Caster != null)
            {
                fxData1.CasterNetID = particle.Caster.NetId;
            }

            if (particle.Direction.Length() <= 0)
            {
                fxData1.OrientationVector = Vector3.Zero;
            }
            else
            {
                fxData1.OrientationVector = particle.Direction;
            }

            fxDataList.Add(fxData1);

            // TODO: implement option for multiple groups of particles instead of hardcoding one
            var fxGroups = new List<FXCreateGroupData>();

            var fxGroupData1 = new FXCreateGroupData
            {
                EffectNameHash = HashString(particle.Name),
                Flags = (ushort)particle.Flags,
                TargetBoneNameHash = HashString(particle.TargetBoneName),
                BoneNameHash = HashString(particle.BoneName),

                FXCreateData = fxDataList
            };

            if (particle.Caster != null && particle.Caster is IObjAiBase o)
            {
                fxGroupData1.PackageHash = o.GetObjHash();
            }
            else
            {
                fxGroupData1.PackageHash = 0; // TODO: Verify
            }

            fxGroups.Add(fxGroupData1);

            fxPacket.FXCreateGroup = fxGroups;

            return fxPacket;
        }

        Barrack_SpawnUnit ConstructLaneMinionSpawnedPacket(ILaneMinion m)
        {
            return new Barrack_SpawnUnit
            {
                SenderNetID = m.NetId,
                ObjectID = m.NetId,
                ObjectNodeID = 0x40, // TODO: check this
                BarracksNetID = 0xFF000000 | Crc32Algorithm.Compute(Encoding.UTF8.GetBytes(m.BarracksName)),
                WaveCount = 1, // TODO: Unhardcode
                MinionType = (byte)m.MinionSpawnType,
                DamageBonus = (short)m.DamageBonus,
                HealthBonus = (short)m.HealthBonus,
                MinionLevel = m.Stats.Level
            };
        }

        SpawnMinionS2C ConstructMinionSpawnedPacket(IMinion minion)
        {
            var spawnPacket = new SpawnMinionS2C
            {
                SenderNetID = minion.NetId,
                NetID = minion.NetId,
                OwnerNetID = minion.NetId,
                NetNodeID = (byte)NetNodeID.Spawned,
                Position = minion.GetPosition3D(),
                SkinID = minion.SkinID,
                TeamID = (ushort)minion.Team,
                IgnoreCollision = minion.IgnoresCollision,
                IsWard = minion.IsWard,
                IsLaneMinion = minion.IsLaneMinion,
                IsBot = minion.IsBot,
                IsTargetable = minion.IsTargetable,

                IsTargetableToTeamSpellFlags = (uint)minion.Stats.IsTargetableToTeam,
                VisibilitySize = minion.VisionRadius,
                Name = minion.Name,
                SkinName = minion.Model,
                InitialLevel = (ushort)minion.InitialLevel,
                OnlyVisibleToNetID = 0
            };

            if (minion.Owner != null)
            {
                spawnPacket.OwnerNetID = minion.Owner.NetId;
            }

            if (minion.VisibilityOwner != null)
            {
                spawnPacket.OnlyVisibleToNetID = minion.VisibilityOwner.NetId;
            }

            return spawnPacket;
        }

        MissileReplication ConstructMissileReplicationPacket(ISpellMissile m)
        {
            var castInfo = new CastInfo
            {
                SpellHash = m.CastInfo.SpellHash,
                SpellNetID = m.CastInfo.SpellNetID,

                SpellLevel = m.CastInfo.SpellLevel,
                AttackSpeedModifier = m.CastInfo.AttackSpeedModifier,
                CasterNetID = m.CastInfo.Owner.NetId,
                // TODO: Implement spell chains?
                SpellChainOwnerNetID = m.CastInfo.Owner.NetId,
                PackageHash = m.CastInfo.PackageHash,
                MissileNetID = m.CastInfo.MissileNetID,
                // Not sure if we want to add height for these, but i did it anyway
                TargetPosition = m.CastInfo.TargetPosition,
                TargetPositionEnd = m.CastInfo.TargetPositionEnd,
                DesignerCastTime = m.CastInfo.DesignerCastTime,
                ExtraCastTime = m.CastInfo.ExtraCastTime,
                DesignerTotalTime = m.CastInfo.DesignerTotalTime,

                Cooldown = m.CastInfo.Cooldown,
                StartCastTime = m.CastInfo.StartCastTime,

                IsAutoAttack = m.CastInfo.IsAutoAttack,
                IsSecondAutoAttack = m.CastInfo.IsSecondAutoAttack,
                IsForceCastingOrChannel = m.CastInfo.IsForceCastingOrChannel,
                IsOverrideCastPosition = m.CastInfo.IsOverrideCastPosition,
                IsClickCasted = m.CastInfo.IsClickCasted,

                SpellSlot = m.CastInfo.SpellSlot,
                ManaCost = m.CastInfo.ManaCost,
                SpellCastLaunchPosition = m.CastInfo.SpellCastLaunchPosition,
                AmmoUsed = m.CastInfo.AmmoUsed,
                AmmoRechargeTime = m.CastInfo.AmmoRechargeTime
            };

            if (m.CastInfo.Targets.Count > 0)
            {
                m.CastInfo.Targets.ForEach(t =>
                {
                    if (t.Unit != null)
                    {
                        castInfo.Targets.Add(new CastInfo.Target() { UnitNetID = t.Unit.NetId, HitResult = (byte)t.HitResult });
                    }
                    else
                    {
                        castInfo.Targets.Add(new CastInfo.Target() { UnitNetID = 0, HitResult = (byte)t.HitResult });
                    }
                });
            }

            var misPacket = new MissileReplication
            {
                SenderNetID = m.CastInfo.Owner.NetId,
                Position = m.GetPosition3D(),
                CasterPosition = m.CastInfo.Owner.GetPosition3D(),
                // Not sure if we want to add height for these, but i did it anyway
                Direction = m.Direction,
                Velocity = m.Direction * m.GetSpeed(),
                StartPoint = m.CastInfo.SpellCastLaunchPosition,
                EndPoint = m.CastInfo.TargetPositionEnd,
                // TODO: Verify
                UnitPosition = m.CastInfo.Owner.GetPosition3D(),
                TimeFromCreation = m.GetTimeSinceCreation(), // TODO: Unhardcode
                Speed = m.GetSpeed(),
                LifePercentage = 0f, // TODO: Unhardcode
                //TODO: Implement time limited projectiles
                TimedSpeedDelta = 0f, // TODO: Implement time limited projectiles for this
                TimedSpeedDeltaTime = 0x7F7FFFFF, // Same as above (this value is from the SpawnProjectile packet, it is a placeholder)

                Bounced = false, //TODO: Implement bouncing projectiles

                CastInfo = castInfo
            };

            if (m is ISpellChainMissile chainMissile && chainMissile.ObjectsHit.Count > 0)
            {
                misPacket.Bounced = true;
            }

            return misPacket;
        }

        SpawnLevelPropS2C ConstructSpawnLevelPropPacket(ILevelProp levelProp, int userId = 0)
        {
            return new SpawnLevelPropS2C
            {
                NetID = levelProp.NetId,
                NetNodeID = levelProp.NetNodeID,
                SkinID = levelProp.SkinID,
                Position = new Vector3(levelProp.Position.X, levelProp.Height, levelProp.Position.Y),
                FacingDirection = levelProp.Direction,
                PositionOffset = levelProp.PositionOffset,
                Scale = levelProp.Scale,
                TeamID = (ushort)levelProp.Team,
                SkillLevel = levelProp.SkillLevel,
                Rank = levelProp.Rank,
                Type = levelProp.Type,
                Name = levelProp.Name,
                PropName = levelProp.Model
            };
        }

        GamePacket ConstructSpawnPacket(IGameObject o, float gameTime = 0)
        {
            switch (o)
            {
                case ISpellMissile missile:
                    return ConstructMissileReplicationPacket(missile);
                case ILevelProp prop:
                    return ConstructSpawnLevelPropPacket(prop);
                case IRegion region:
                    return ConstructAddRegionPacket(region);

                case ILaneTurret turret:
                    return ConstructCreateTurretPacket(turret);

                // Champions spawn a little differently 
                case IChampion champion:
                    return null;
                case IPet pet:
                    return ConstructSpawnPetPacket(pet);
                case IMonster monster:
                    return ConstructCreateNeutralPacket(monster, gameTime);
                case ILaneMinion minion:
                    return ConstructLaneMinionSpawnedPacket(minion);
                case IMinion minion:
                    return ConstructMinionSpawnedPacket(minion);

                case IParticle particle:
                    return ConstructFXCreateGroupPacket(particle);
            }
            // Generic object
            return ConstructEnterVisibilityClientPacket(o);
        }

        public CHAR_SpawnPet ConstructSpawnPetPacket(IPet pet)
        {
            var packet = new CHAR_SpawnPet
            {
                OwnerNetID = pet.Owner.NetId,
                NetNodeID = (byte)NetNodeID.Spawned,
                Position = pet.GetPosition3D(),
                CastSpellLevelPlusOne = pet.SourceSpell.CastInfo.SpellLevel,
                Duration = pet.LifeTime,
                TeamID = (uint)pet.Team,
                DamageBonus = pet.DamageBonus,
                HealthBonus = pet.HealthBonus,
                Name = pet.Name,
                Skin = pet.Model,
                SkinID = pet.SkinID,
                BuffName = pet.CloneBuff.Name,
                CloneInventory = pet.CloneInventory,
                ShowMinimapIconIfClone = pet.ShowMinimapIconIfClone,
                DisallowPlayerControl = pet.DisallowPlayerControl,
                DoFade = pet.DoFade,
                SenderNetID = pet.NetId
            };

            if (pet.IsClone)
            {
                packet.CloneID = pet.Owner.NetId;
            }

            return packet;
        }
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
        public void NotifyAddDebugObject(int userId, IAttackableUnit unit, uint objNetId, float lifetime, float radius, Vector3 pos1, Vector3 pos2, int objID = 0, byte type = 0x0, string name = "debugobj", byte r = 0xFF, byte g = 0x46, byte b = 0x0)
        {
            //TODO: Implement a DebugObject class so this is cleaner
            var color = new LeaguePackets.Game.Common.Color
            {
                Red = r,
                Green = g,
                Blue = b
            };
            var debugObjPacket = new S2C_AddDebugObject
            {
                SenderNetID = unit.NetId,
                DebugID = objID,
                Lifetime = lifetime,
                Type = type,
                NetID1 = unit.NetId,
                NetID2 = objNetId,
                Radius = radius,
                Point1 = pos1,
                Point2 = pos2,
                Color = color,
                MaxSize = 0, // TODO: Verify what this does
                Bitfield = 0x0, // TODO: Verify what this does
                StringBuffer = name
            };
            _packetHandlerManager.SendPacket(userId, debugObjPacket.GetBytes(), Channel.CHL_S2C);
        }

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
        public void NotifyAddRegion(uint unitNetId, uint bubbleNetId, TeamId team, Vector2 position, float time, float radius = 0, int regionType = 0, ClientInfo clientInfo = null, IGameObject obj = null, float collisionRadius = 0, float grassRadius = 0, float sizemult = 1.0f, float addsize = 0, bool grantVis = true, bool stealthVis = false)
        {
            var regionPacket = new AddRegion
            {
                TeamID = (uint)team,
                RegionType = regionType, // TODO: Find out what values this can be and make an enum for it (so far: -2 for turrets)
                UnitNetID = unitNetId, // TODO: Verify (usually 0 for vision only?)
                BubbleNetID = bubbleNetId, // TODO: Verify (is usually different from UnitNetID in packets, may also be a remnant or for internal use)
                VisionTargetNetID = 0,
                Position = position,
                TimeToLive = time, // For turrets, usually 25000.0 is used
                ColisionRadius = collisionRadius, // 88.4 for turrets
                GrassRadius = grassRadius, // 130.0 for turrets
                SizeMultiplier = sizemult,
                SizeAdditive = addsize,

                HasCollision = false,
                GrantVision = grantVis,
                RevealStealth = stealthVis,

                BaseRadius = radius // 800.0 for turrets
            };

            if (clientInfo != null)
            {
                if (clientInfo.Champion != null)
                {
                    regionPacket.VisionTargetNetID = clientInfo.Champion.NetId;
                }
                regionPacket.ClientID = (int)clientInfo.ClientId;
            }

            if (obj != null)
            {
                regionPacket.VisionTargetNetID = obj.NetId;
            }

            // TODO: Verify
            if (collisionRadius > 0.0f)
            {
                regionPacket.HasCollision = true;
            }

            _packetHandlerManager.BroadcastPacketTeam(team, regionPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified team that a part of the map has changed. Known to be used in League for initializing turret vision and collision.
        /// </summary>
        /// <param name="region">Region to add.</param>
        public void NotifyAddRegion(IRegion region)
        {
            var regionPacket = ConstructAddRegionPacket(region);

            // Verify if this should be vision or team regulated.
            _packetHandlerManager.BroadcastPacket(regionPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players with vision of the specified attacker detailing that they have targeted the specified target.
        /// </summary>
        /// <param name="attacker">AI that is targeting an AttackableUnit.</param>
        /// <param name="target">AttackableUnit that is being targeted by the attacker.</param>
        public void NotifyAI_TargetS2C(IObjAiBase attacker, IAttackableUnit target)
        {
            var targetPacket = new AI_TargetS2C
            {
                SenderNetID = attacker.NetId,
                TargetNetID = 0
            };

            if (target != null)
            {
                targetPacket.TargetNetID = target.NetId;
            }

            // TODO: Verify if we need to account for other cases.
            if (attacker is IBaseTurret)
            {
                _packetHandlerManager.BroadcastPacket(targetPacket.GetBytes(), Channel.CHL_S2C);
            }
            else
            {
                _packetHandlerManager.BroadcastPacketVision(attacker, targetPacket.GetBytes(), Channel.CHL_S2C);
            }
        }

        /// <summary>
        /// Sends a packet to all players with vision of the specified attacker detailing that they have targeted the specified champion.
        /// </summary>
        /// <param name="attacker">AI that is targeting a champion.</param>
        /// <param name="target">Champion that is being targeted by the attacker.</param>
        public void NotifyAI_TargetHeroS2C(IObjAiBase attacker, IChampion target)
        {
            var targetPacket = new AI_TargetHeroS2C
            {
                SenderNetID = attacker.NetId,
                TargetNetID = 0
            };

            if (target != null)
            {
                targetPacket.TargetNetID = target.NetId;
            }

            _packetHandlerManager.BroadcastPacketVision(attacker, targetPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified user or all users informing them of the given client's summoner data such as runes, summoner spells, masteries (or talents as named internally), etc.
        /// </summary>
        /// <param name="client">Info about the player's summoner data.</param>
        /// <param name="userId">User to send the packet to. Set to -1 to broadcast.</param>
        public void NotifyAvatarInfo(ClientInfo client, int userId = -1)
        {
            var avatar = new AvatarInfo_Server();
            avatar.SenderNetID = client.Champion.NetId;
            var skills = new uint[] {
                HashFunctions.HashString(client.SummonerSkills[0]),
                HashFunctions.HashString(client.SummonerSkills[1])
            };

            avatar.SummonerIDs[0] = skills[0];
            avatar.SummonerIDs[1] = skills[1];
            for (int i = 0; i < client.Champion.RuneList.Runes.Count; ++i)
            {
                int runeValue = 0;
                client.Champion.RuneList.Runes.TryGetValue(i, out runeValue);
                avatar.ItemIDs[i] = (uint)runeValue;
            }

            for (int i = 0; i < client.Champion.TalentInventory.Talents.Count; i++)
            {
                avatar.Talents[i] = new Talent
                {
                    Hash = HashString(client.Champion.TalentInventory.Talents[i].Name),
                    Level = client.Champion.TalentInventory.Talents[i].Rank
                };
            }

            if (userId < 0)
            {
                _packetHandlerManager.BroadcastPacket(avatar.GetBytes(), Channel.CHL_S2C);
                return;
            }

            _packetHandlerManager.SendPacket(userId, avatar.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players detailing that the specified  unit is starting their next auto attack.
        /// </summary>
        /// <param name="attacker">Unit that is attacking.</param>
        /// <param name="target">AttackableUnit being attacked.</param>
        /// <param name="futureProjNetId">NetId of the auto attack projectile.</param>
        /// <param name="isCrit">Whether or not the auto attack will crit.</param>
        /// <param name="nextAttackFlag">Whether or this basic attack is not the first time this basic attack has been performed on the given target.</param>
        public void NotifyBasic_Attack(IObjAiBase attacker, IAttackableUnit target, uint futureProjNetId, bool isCrit, bool nextAttackFlag)
        {
            var targetPos = MovementVector.ToCenteredScaledCoordinates(target.Position, _navGrid);

            // TODO: Remove attacker, target, and futureProjNetId parameters and replace with CastInfo
            var basicAttackData = new BasicAttackData
            {
                TargetNetID = target.NetId,
                ExtraTime = attacker.AutoAttackSpell.CastInfo.ExtraCastTime, // TODO: Verify, maybe related to CastInfo.ExtraCastTime?
                MissileNextID = futureProjNetId,
                AttackSlot = attacker.AutoAttackSpell.CastInfo.SpellSlot,
                TargetPosition = new Vector3(targetPos.X, _navGrid.GetHeightAtLocation(targetPos.X, targetPos.Y), targetPos.Y)
            };

            // Based on DesignerCastTime. Always negative. Value range from replays: [-0.14, 0].
            // TODO: Find out what should go here.
            basicAttackData.ExtraTime = -attacker.AutoAttackSpell.CurrentDelayTime;

            var basicAttackPacket = new Basic_Attack
            {
                SenderNetID = attacker.NetId,
                Attack = basicAttackData
            };
            _packetHandlerManager.BroadcastPacketVision(attacker, basicAttackPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players that the specified attacker is starting their first auto attack.
        /// </summary>
        /// <param name="attacker">AI that is starting an auto attack.</param>
        /// <param name="target">AttackableUnit being attacked.</param>
        /// <param name="futureProjNetId">NetID of the projectile that will be created for the auto attack.</param>
        /// <param name="isCrit">Whether or not the auto attack is a critical.</param>
        public void NotifyBasic_Attack_Pos(IObjAiBase attacker, IAttackableUnit target, uint futureProjNetId, bool isCrit)
        {
            var targetPos = MovementVector.ToCenteredScaledCoordinates(target.Position, _navGrid);

            // TODO: Remove attacker, target, and futureProjNetId parameters and replace with CastInfo
            var basicAttackData = new BasicAttackData
            {
                TargetNetID = target.NetId,
                ExtraTime = attacker.AutoAttackSpell.CastInfo.ExtraCastTime, // TODO: Verify, maybe related to CastInfo.ExtraCastTime?
                MissileNextID = futureProjNetId,
                AttackSlot = attacker.AutoAttackSpell.CastInfo.SpellSlot,
                TargetPosition = new Vector3(targetPos.X, target.GetHeight(), targetPos.Y)
            };

            // Based on DesignerCastTime. Always negative. Value range from replays: [-0.14, 0].
            // TODO: Find out what should go here.
            basicAttackData.ExtraTime = -attacker.AutoAttackSpell.CurrentDelayTime;

            var basicAttackPacket = new Basic_Attack_Pos
            {
                SenderNetID = attacker.NetId,
                Attack = basicAttackData,
                Position = attacker.Position // TODO: Verify
            };
            _packetHandlerManager.BroadcastPacketVision(attacker, basicAttackPacket.GetBytes(), Channel.CHL_S2C);
        }
        /// <summary>
        /// Sends a packet to all players detailing that the specified building has died.
        /// </summary>
        /// <param name="deathData"></param>
        public void NotifyBuilding_Die(IDeathData deathData)
        {
            var buildingDie = new Building_Die
            {
                SenderNetID = deathData.Unit.NetId,
                AttackerNetID = deathData.Killer.NetId,
                //TODO: Unhardcode this when an assists system gets implemented
                LastHeroNetID = 0
            };
            _packetHandlerManager.BroadcastPacket(buildingDie.GetBytes(), Channel.CHL_S2C);
        }
        /// <summary>
        /// Sends a packet to the player attempting to buy an item that their purchase was successful.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="gameObject">GameObject of type ObjAiBase that can buy items.</param>
        /// <param name="itemInstance">Item instance housing all information about the item that has been bought.</param>
        public void NotifyBuyItem(int userId, IObjAiBase gameObject, IItem itemInstance)
        {
            ItemData itemData = new ItemData
            {
                ItemID = (uint)itemInstance.ItemData.ItemId,
                Slot = gameObject.Inventory.GetItemSlot(itemInstance),
                ItemsInSlot = (byte)itemInstance.StackCount,
                SpellCharges = 0 // TODO: Unhardcode
            };

            //TODO find out what bitfield does, currently unknown
            var buyItemPacket = new BuyItemAns
            {
                SenderNetID = gameObject.NetId,
                Item = itemData,
                Bitfield = 0 //TODO: find out what this does, currently unknown
            };

            _packetHandlerManager.BroadcastPacketVision(gameObject, buyItemPacket.GetBytes(), Channel.CHL_S2C);
        }

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
        public void NotifyChangeSlotSpellData(int userId, IObjAiBase owner, byte slot, GameServerCore.Enums.ChangeSlotSpellDataType changeType, bool isSummonerSpell = false, TargetingType targetingType = TargetingType.Invalid, string newName = "", float newRange = 0, float newMaxCastRange = 0, float newDisplayRange = 0, byte newIconIndex = 0x0, List<uint> offsetTargets = null)
        {
            ChangeSpellData spellData = new ChangeSpellDataUnknown()
            {
                SpellSlot = slot,
                IsSummonerSpell = isSummonerSpell
            };

            switch (changeType)
            {
                case GameServerCore.Enums.ChangeSlotSpellDataType.TargetingType:
                    {
                        if (targetingType != TargetingType.Invalid)
                        {
                            spellData = new ChangeSpellDataTargetingType()
                            {
                                SpellSlot = slot,
                                IsSummonerSpell = isSummonerSpell,
                                TargetingType = (byte)targetingType
                            };
                        }
                        break;
                    }
                case GameServerCore.Enums.ChangeSlotSpellDataType.SpellName:
                    {
                        spellData = new ChangeSpellDataSpellName()
                        {
                            SpellSlot = slot,
                            IsSummonerSpell = isSummonerSpell,
                            SpellName = newName
                        };
                        break;
                    }
                case GameServerCore.Enums.ChangeSlotSpellDataType.Range:
                    {
                        spellData = new ChangeSpellDataRange()
                        {
                            SpellSlot = slot,
                            IsSummonerSpell = isSummonerSpell,
                            CastRange = newRange
                        };
                        break;
                    }
                case GameServerCore.Enums.ChangeSlotSpellDataType.MaxGrowthRange:
                    {
                        spellData = new ChangeSpellDataMaxGrowthRange()
                        {
                            SpellSlot = slot,
                            IsSummonerSpell = isSummonerSpell,
                            OverrideMaxCastRange = newMaxCastRange
                        };
                        break;
                    }
                case GameServerCore.Enums.ChangeSlotSpellDataType.RangeDisplay:
                    {
                        spellData = new ChangeSpellDataRangeDisplay()
                        {
                            SpellSlot = slot,
                            IsSummonerSpell = isSummonerSpell,
                            OverrideCastRangeDisplay = newDisplayRange
                        };
                        break;
                    }
                case GameServerCore.Enums.ChangeSlotSpellDataType.IconIndex:
                    {
                        spellData = new ChangeSpellDataIconIndex()
                        {
                            SpellSlot = slot,
                            IsSummonerSpell = isSummonerSpell,
                            IconIndex = newIconIndex
                        };
                        break;
                    }
                case GameServerCore.Enums.ChangeSlotSpellDataType.OffsetTarget:
                    {
                        if (offsetTargets != null)
                        {
                            spellData = new ChangeSpellDataOffsetTarget()
                            {
                                SpellSlot = slot,
                                IsSummonerSpell = isSummonerSpell,
                                Targets = offsetTargets
                            };
                        }
                        break;
                    }
            }

            var changePacket = new ChangeSlotSpellData()
            {
                SenderNetID = owner.NetId,
                ChangeSpellData = spellData
            };

            _packetHandlerManager.SendPacket(userId, changePacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players with vision of a specified ObjAiBase explaining that their specified spell's cooldown has been set.
        /// </summary>
        /// <param name="u">ObjAiBase who owns the spell going on cooldown.</param>
        /// <param name="slotId">Slot of the spell.</param>
        /// <param name="currentCd">Amount of time the spell has already been on cooldown (if applicable).</param>
        /// <param name="totalCd">Maximum amount of time the spell's cooldown can be.</param>
        /// <param name="userId">UserId to send the packet to. If not specified or zero, the packet is broadcasted to all players that have vision of the specified unit.</param>
        public void NotifyCHAR_SetCooldown(IObjAiBase u, byte slotId, float currentCd, float totalCd, int userId = 0)
        {
            var cdPacket = new CHAR_SetCooldown
            {
                SenderNetID = u.NetId,
                Slot = slotId,
                PlayVOWhenCooldownReady = false, // TODO: Unhardcode
                IsSummonerSpell = false, // TODO: Unhardcode
                Cooldown = currentCd,
                MaxCooldownForDisplay = 0 // TODO: Verify (packet loses functionality otherwise)
            };
            if (u is IChampion && (slotId == 4 || slotId == 5))
            {
                cdPacket.IsSummonerSpell = true; // TODO: Verify functionality
            }
            if (userId == 0)
            {
                _packetHandlerManager.BroadcastPacketVision(u, cdPacket.GetBytes(), Channel.CHL_S2C);
            }
            else
            {
                _packetHandlerManager.SendPacket(userId, cdPacket.GetBytes(), Channel.CHL_S2C);
            }
        }

        /// <summary>
        /// Sends a packet to the specified user that highlights the specified GameObject.
        /// </summary>
        /// <param name="userId">ID of the user to send the packet to.</param>
        /// <param name="unit">GameObject to highlght.</param>
        public void NotifyCreateUnitHighlight(int userId, IGameObject unit)
        {
            var highlightPacket = new S2C_CreateUnitHighlight
            {
                SenderNetID = unit.NetId,
                TargetNetID = unit.NetId
            };

            _packetHandlerManager.SendPacket(userId, highlightPacket.GetBytes(), Channel.CHL_S2C);
        }

        public void NotifyDampenerSwitchStates(IInhibitor inhibitor)
        {
            var inhibState = new DampenerSwitchStates
            {
                SenderNetID = inhibitor.NetId,
                State = (byte)inhibitor.InhibitorState,
                Duration = (ushort)inhibitor.RespawnTime
            };
            _packetHandlerManager.BroadcastPacket(inhibState.GetBytes(), Channel.CHL_S2C);
        }

        public void NotifyDeath(IDeathData deathData)
        {
            switch (deathData.Unit)
            {
                case IChampion ch:
                    NotifyNPC_Hero_Die(deathData);
                    break;
                case IMinion minion:
                    if (minion is IPet || minion is ILaneMinion)
                    {
                        NotifyS2C_NPC_Die_MapView(deathData);
                    }
                    else
                    {
                        goto default;
                    }
                    break;
                case IObjBuilding building:
                    NotifyBuilding_Die(deathData);
                    break;
                default:
                    NotifyNPC_Die_Broadcast(deathData);
                    break;
            }
        }

        /// <summary>
        /// Sends a packet to the specified user which is intended for debugging.
        /// </summary>
        /// <param name="userId">ID of the user to send the packet to.</param>
        /// <param name="data">Array of bytes representing the packet's data.</param>
        public void NotifyDebugPacket(int userId, byte[] data)
        {
            _packetHandlerManager.SendPacket(userId, data, Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players detailing the destruction of (usually) an auto attack missile.
        /// </summary>
        /// <param name="p">Projectile that is being destroyed.</param>
        public void NotifyDestroyClientMissile(ISpellMissile p)
        {
            var misPacket = new S2C_DestroyClientMissile
            {
                SenderNetID = p.NetId
            };
            _packetHandlerManager.BroadcastPacket(misPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified team detailing the destruction of (usually) an auto attack missile.
        /// </summary>
        /// <param name="p">Projectile that is being destroyed.</param>
        /// <param name="team">TeamId to send the packet to.</param>
        public void NotifyDestroyClientMissile(ISpellMissile p, TeamId team)
        {
            var misPacket = new S2C_DestroyClientMissile
            {
                SenderNetID = p.NetId
            };
            _packetHandlerManager.BroadcastPacketTeam(team, misPacket.GetBytes(), Channel.CHL_S2C);
        }
        /// <summary>
        /// Sends a packet to either all players with vision of a target, or the specified player.
        /// The packet displays the specified message of the specified type as floating text over a target.
        /// </summary>
        /// <param name="target">Target to display on.</param>
        /// <param name="message">Message to display.</param>
        /// <param name="textType">Type of text to display. Refer to FloatTextType</param>
        /// <param name="userId">User to send to. 0 = sends to all in vision.</param>
        /// <param name="param">Optional parameters for the text. Untested, function unknown.</param>
        public void NotifyDisplayFloatingText(IFloatingTextData floatTextData, TeamId team = 0, int userId = 0)
        {
            var textPacket = new DisplayFloatingText
            {
                TargetNetID = floatTextData.Target.NetId,
                FloatTextType = (uint)floatTextData.FloatTextType,
                Param = floatTextData.Param,
                Message = floatTextData.Message
            };

            if (userId == 0)
            {
                if (team != 0)
                {
                    _packetHandlerManager.BroadcastPacketTeam(team, textPacket.GetBytes(), Channel.CHL_S2C);
                }
                else
                {
                    _packetHandlerManager.BroadcastPacketVision(floatTextData.Target, textPacket.GetBytes(), Channel.CHL_S2C);
                }
            }
            else
            {
                _packetHandlerManager.SendPacket(userId, textPacket.GetBytes(), Channel.CHL_S2C);
            }
        }

        /// <summary>
        /// Sends a packet to the specified user detailing that the GameObject that owns the specified netId has finished being initialized into vision.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="netId">NetID of the GameObject coming into vision.</param>
        public void NotifyEnterLocalVisibilityClient(int userId, uint netId)
        {
            var enterLocalVis = new OnEnterLocalVisibilityClient
            {
                SenderNetID = netId
            };

            _packetHandlerManager.SendPacket(userId, enterLocalVis.GetBytes(), Channel.CHL_S2C);
        }

        OnEnterLocalVisibilityClient ConstructEnterLocalVisibilityClientPacket(IGameObject o)
        {
            var enterLocalVis = new OnEnterLocalVisibilityClient
            {
                SenderNetID = o.NetId,
            };

            if (o is IAttackableUnit u)
            {
                enterLocalVis.MaxHealth = u.Stats.HealthPoints.Total;
                enterLocalVis.Health = u.Stats.CurrentHealth;
            }

            return enterLocalVis;
        }

        /// <summary>
        /// Sends a packet to either all players with vision of the specified GameObject or a specified user. The packet contains details of the GameObject's health (given it is of the type AttackableUnit) and is meant for after the GameObject is first initialized into vision.
        /// </summary>
        /// <param name="o">GameObject coming into vision.</param>
        /// <param name="userId">UserId to send the packet to. If not specified or zero, the packet is broadcasted to all players that have vision of the specified unit.</param>
        /// <param name="ignoreVision">Optionally ignore vision checks when sending this packet and broadcast it to all players instead.</param>
        public void NotifyEnterLocalVisibilityClient(IGameObject o, int userId = 0, bool ignoreVision = false)
        {
            var enterLocalVis = ConstructEnterLocalVisibilityClientPacket(o);

            if (userId == 0)
            {
                if (ignoreVision)
                {
                    _packetHandlerManager.BroadcastPacket(enterLocalVis.GetBytes(), Channel.CHL_S2C);
                }
                else
                {
                    _packetHandlerManager.BroadcastPacketVision(o, enterLocalVis.GetBytes(), Channel.CHL_S2C);
                }
            }
            else
            {
                _packetHandlerManager.SendPacket(userId, enterLocalVis.GetBytes(), Channel.CHL_S2C);
            }
        }

        /// <summary>
        /// Sends a packet to either all players with vision of the specified object or the specified user. The packet details the data surrounding the specified GameObject that is required by players when a GameObject enters vision such as items, shields, skin, and movements.
        /// </summary>
        /// <param name="o">GameObject entering vision.</param>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="isChampion">Whether or not the GameObject entering vision is a Champion.</param>
        /// <param name="ignoreVision">Optionally ignore vision checks when sending this packet.</param>
        /// <param name="packets">Takes in a list of packets to send alongside this vision packet.</param>
        /// TODO: Incomplete implementation.
        public void NotifyEnterVisibilityClient(IGameObject o, int userId = 0, bool isChampion = false, bool ignoreVision = false, List<GamePacket> packets = null)
        {

            var enterVis = ConstructEnterVisibilityClientPacket(o, isChampion, packets);

            if (userId != 0)
            {
                _packetHandlerManager.SendPacket(userId, enterVis.GetBytes(), Channel.CHL_S2C);
                NotifyEnterLocalVisibilityClient(o, userId, ignoreVision);
            }
            else
            {
                if (ignoreVision)
                {
                    _packetHandlerManager.BroadcastPacket(enterVis.GetBytes(), Channel.CHL_S2C);
                    NotifyEnterLocalVisibilityClient(o, ignoreVision: true);
                }
                else
                {
                    _packetHandlerManager.BroadcastPacketVision(o, enterVis.GetBytes(), Channel.CHL_S2C);
                    NotifyEnterLocalVisibilityClient(o);
                }
            }
        }

        /// <summary>
        /// Sends a packet to all players with vision of the specified unit detailing that the unit has begun facing the specified direction.
        /// </summary>
        /// <param name="obj">GameObject that is changing their orientation.</param>
        /// <param name="direction">3D direction the unit will face.</param>
        /// <param name="isInstant">Whether or not the unit should instantly turn to the direction.</param>
        /// <param name="turnTime">The amount of time (seconds) the turn should take.</param>
        public void NotifyFaceDirection(IGameObject obj, Vector3 direction, bool isInstant = true, float turnTime = 0.0833f)
        {
            var facePacket = new S2C_FaceDirection()
            {
                SenderNetID = obj.NetId,
                Direction = direction,
                DoLerpTime = !isInstant,
                LerpTime = turnTime
            };

            _packetHandlerManager.BroadcastPacket(facePacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players that (usually) an auto attack missile has been created.
        /// </summary>
        /// <param name="p">Projectile that was created.</param>
        public void NotifyForceCreateMissile(ISpellMissile p)
        {
            var misPacket = new S2C_ForceCreateMissile
            {
                SenderNetID = p.CastInfo.Owner.NetId,
                MissileNetID = p.NetId
            };

            _packetHandlerManager.BroadcastPacket(misPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to, optionally, a specified player, all players with vision of the particle, or all players (given the particle is set as globally visible).
        /// </summary>
        /// <param name="particle">Particle to network.</param>
        /// <param name="userId">User to send the packet to.</param>
        public void NotifyFXCreateGroup(IParticle particle, int userId = 0)
        {
            var fxPacket = ConstructFXCreateGroupPacket(particle);

            if (userId == 0)
            {
                // Broadcast only to specific team.
                if (particle.SpecificTeam != TeamId.TEAM_NEUTRAL)
                {
                    _packetHandlerManager.BroadcastPacketTeam(particle.SpecificTeam, fxPacket.GetBytes(), Channel.CHL_S2C);
                }
                // Broadcast to particle team, and only to opposite team if visible.
                else if (particle.Team != TeamId.TEAM_NEUTRAL)
                {
                    _packetHandlerManager.BroadcastPacketTeam(particle.Team, fxPacket.GetBytes(), Channel.CHL_S2C);

                    var oppTeam = particle.Team.GetEnemyTeam();
                    if (particle.IsVisibleByTeam(oppTeam))
                    {
                        _packetHandlerManager.BroadcastPacketTeam(oppTeam, fxPacket.GetBytes(), Channel.CHL_S2C);
                    }
                }
                // Broadcast to all teams.
                else
                {
                    _packetHandlerManager.BroadcastPacket(fxPacket.GetBytes(), Channel.CHL_S2C);
                }
            }
            else
            {
                _packetHandlerManager.SendPacket(userId, fxPacket.GetBytes(), Channel.CHL_S2C);
            }
        }

        S2C_FX_OnEnterTeamVisibility ConstructFXEnterTeamVisibilityPacket(IParticle particle, TeamId team)
        {
            var fxVisPacket = new S2C_FX_OnEnterTeamVisibility
            {
                SenderNetID = particle.NetId,
                NetID = particle.NetId
            };

            fxVisPacket.VisibilityTeam = 0;
            if (team == TeamId.TEAM_PURPLE || team == TeamId.TEAM_NEUTRAL)
            {
                fxVisPacket.VisibilityTeam = 1;
            }

            return fxVisPacket;
        }

        /// <summary>
        /// Sends a packet to the specified team detailing that the specified Particle has become visible.
        /// </summary>
        /// <param name="particle">Particle that came into vision.</param>
        /// <param name="team">TeamId to send the packet to; BLUE/PURPLE/NEUTRAL.</param>
        public void NotifyFXEnterTeamVisibility(IParticle particle, TeamId team)
        {
            var fxVisPacket = ConstructFXEnterTeamVisibilityPacket(particle, team);
            _packetHandlerManager.BroadcastPacketTeam(team, fxVisPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players detailing that the specified particle has been destroyed.
        /// </summary>
        /// <param name="particle">Particle that is being destroyed.</param>
        /// TODO: Change to only broadcast to players who have vision of the particle (maybe?).
        public void NotifyFXKill(IParticle particle)
        {
            var fxKill = new FX_Kill
            {
                SenderNetID = particle.NetId,
                NetID = particle.NetId
            };
            _packetHandlerManager.BroadcastPacket(fxKill.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified team detailing that the specified Particle has left vision.
        /// </summary>
        /// <param name="particle">Particle that left vision.</param>
        /// <param name="team">TeamId to send the packet to; BLUE/PURPLE/NEUTRAL.</param>
        public void NotifyFXLeaveTeamVisibility(IParticle particle, TeamId team)
        {
            NotifyFXLeaveTeamVisibility(particle, team, 0);
        }

        public void NotifyFXLeaveTeamVisibility(IParticle particle, TeamId team, int userId = 0)
        {
            var fxVisPacket = new S2C_FX_OnLeaveTeamVisibility
            {
                SenderNetID = particle.NetId,
                NetID = particle.NetId
            };

            fxVisPacket.VisibilityTeam = 0;
            if (team == TeamId.TEAM_PURPLE || team == TeamId.TEAM_NEUTRAL)
            {
                fxVisPacket.VisibilityTeam = 1;
            }

            if (userId == 0)
            {
                _packetHandlerManager.BroadcastPacketTeam(team, fxVisPacket.GetBytes(), Channel.CHL_S2C);
            }
            else
            {
                _packetHandlerManager.SendPacket(userId, fxVisPacket.GetBytes(), Channel.CHL_S2C);
            }
        }

        /// <summary>
        /// Sends a packet to all players detailing that the game has started. Sent when all players have finished loading.
        /// </summary>
        /// <param name="userId">UserId to send the packet to. If not specified or zero, the packet is broadcasted to all players.</param>
        public void NotifyGameStart(int userId = 0)
        {
            var start = new S2C_StartGame
            {
                EnablePause = true
            };
            if (userId == 0)
            {
                _packetHandlerManager.BroadcastPacket(start.GetBytes(), Channel.CHL_S2C);
            }
            else
            {
                _packetHandlerManager.SendPacket(userId, start.GetBytes(), Channel.CHL_S2C);
            }
        }

        /// <summary>
        /// Sends a packet to all players detailing the state (DEAD/ALIVE) of the specified inhibitor.
        /// </summary>
        /// <param name="inhibitor">Inhibitor to check.</param>
        /// <param name="killer">Killer of the inhibitor (if applicable).</param>
        /// <param name="assists">Assists of the killer (if applicable).</param>
        public void NotifyInhibitorState(IInhibitor inhibitor, IDeathData deathData = null, List<IChampion> assists = null)
        {
            switch (inhibitor.InhibitorState)
            {
                case InhibitorState.DEAD:
                    var annoucementDeath = new OnDampenerDie
                    {
                        //All mentions i found were 0, investigate further if we'd want to unhardcode this
                        GoldGiven = 0.0f,
                        OtherNetID = deathData.Killer.NetId,
                        AssistCount = 0
                        //TODO: Inplement assists when an assist system gets put in place
                    };
                    NotifyS2C_OnEventWorld(annoucementDeath, inhibitor.NetId);

                    NotifyBuilding_Die(deathData);

                    break;
                case InhibitorState.ALIVE:
                    var annoucementRespawn = new OnDampenerRespawn
                    {
                        OtherNetID = inhibitor.NetId
                    };
                    NotifyS2C_OnEventWorld(annoucementRespawn, inhibitor.NetId);
                    break;
            }
            NotifyDampenerSwitchStates(inhibitor);
        }
        /// <summary>
        /// Sends a basic heartbeat packet to either the given player or all players.
        /// </summary>
        public void NotifyKeyCheck(int clientID, long playerId, uint version, ulong checkSum = 0, byte action = 0, bool broadcast = false)
        {
            var keyCheck = new KeyCheckPacket
            {
                Action = action,
                ClientID = clientID,
                PlayerID = playerId,
                VersionNumber = version,
                CheckSum = checkSum,
                // Padding
                ExtraBytes = new byte[4]
            };

            if (broadcast)
            {
                _packetHandlerManager.BroadcastPacket(keyCheck.GetBytes(), Channel.CHL_HANDSHAKE);
            }
            else
            {
                _packetHandlerManager.SendPacket((int)playerId, keyCheck.GetBytes(), Channel.CHL_HANDSHAKE);
            }
        }

        /// <summary>
        /// Sends a packet to all players detailing that the specified LaneMinion has spawned.
        /// </summary>
        /// <param name="m">LaneMinion that spawned.</param>
        /// TODO: Implement wave counter.
        public void NotifyLaneMinionSpawned(ILaneMinion m)
        {
            var p = ConstructLaneMinionSpawnedPacket(m);
            _packetHandlerManager.BroadcastPacket(p.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified player detailing that the GameObject which has the specified netId has left vision.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="netId">NetId of the GameObject that left vision.</param>
        /// TODO: Verify where this should be used.
        public void NotifyLeaveLocalVisibilityClient(int userId, uint netId)
        {
            var leaveLocalVis = new OnLeaveLocalVisibilityClient
            {
                SenderNetID = netId
            };

            _packetHandlerManager.SendPacket(userId, leaveLocalVis.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to either the specified player or team detailing that the specified GameObject has left vision.
        /// </summary>
        /// <param name="o">GameObject that left vision.</param>
        /// <param name="team">TeamId to send the packet to; BLUE/PURPLE/NEUTRAL.</param>
        /// <param name="userId">User to send the packet to.</param>
        /// TODO: Verify where this should be used.
        public void NotifyLeaveLocalVisibilityClient(IGameObject o, TeamId team, int userId = 0)
        {
            var leaveLocalVis = new OnLeaveLocalVisibilityClient
            {
                SenderNetID = o.NetId
            };

            if (userId != 0)
            {
                _packetHandlerManager.SendPacket(userId, leaveLocalVis.GetBytes(), Channel.CHL_S2C);
                return;
            }

            _packetHandlerManager.BroadcastPacketTeam(team, leaveLocalVis.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to either the specified user or team detailing that the specified GameObject has left vision.
        /// </summary>
        /// <param name="o">GameObject that left vision.</param>
        /// <param name="team">TeamId to send the packet to; BLUE/PURPLE/NEUTRAL.</param>
        /// <param name="userId">User to send the packet to (if applicable).</param>
        /// TODO: Verify where this should be used.
        public void NotifyLeaveVisibilityClient(IGameObject o, TeamId team, int userId = 0)
        {
            var leaveVis = new OnLeaveVisibilityClient
            {
                SenderNetID = o.NetId
            };

            if (userId != 0)
            {
                _packetHandlerManager.SendPacket(userId, leaveVis.GetBytes(), Channel.CHL_S2C);
            }
            else
            {
                _packetHandlerManager.BroadcastPacketTeam(team, leaveVis.GetBytes(), Channel.CHL_S2C);
            }

            NotifyLeaveLocalVisibilityClient(o, team, userId);
        }

        /// <summary>
        /// Sends a packet to the specified player detailing the order and size of both teams on the loading screen.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="players">Client info of all players in the loading screen.</param>
        public void NotifyLoadScreenInfo(int userId, List<Tuple<uint, ClientInfo>> players)
        {
            uint orderSizeCurrent = 0;
            uint chaosSizeCurrent = 0;

            var teamRoster = new TeamRosterUpdate
            {
                TeamSizeOrder = 6,
                TeamSizeChaos = 6
            };

            foreach (var player in players)
            {
                if (player.Item2.Team == TeamId.TEAM_BLUE)
                {
                    teamRoster.OrderMembers[orderSizeCurrent] = player.Item2.PlayerId;
                    orderSizeCurrent++;
                }
                // TODO: Verify if it is ok to allow neutral
                else
                {
                    teamRoster.ChaosMembers[chaosSizeCurrent] = player.Item2.PlayerId;
                    chaosSizeCurrent++;
                }
            }

            teamRoster.TeamSizeOrderCurrent = orderSizeCurrent;
            teamRoster.TeamSIzeChaosCurrent = chaosSizeCurrent;

            _packetHandlerManager.SendPacket(userId, teamRoster.GetBytes(), Channel.CHL_LOADING_SCREEN);
        }

        /// <summary>
        /// Sends a packet to all players who have vision of the specified Minion detailing that it has spawned.
        /// </summary>
        /// <returns>A new and fully setup SpawnMinionS2C packet.</returns>
        /// <param name="minion">Minion that is spawning.</param>
        public void NotifyMinionSpawned(IMinion minion)
        {
            var spawnPacket = ConstructMinionSpawnedPacket(minion);
            _packetHandlerManager.BroadcastPacketVision(minion, spawnPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to either all players with vision (given the projectile is networked to the client) of the projectile, or all players. The packet contains all details regarding the specified projectile's creation.
        /// </summary>
        /// <param name="p">Projectile that was created.</param>
        public void NotifyMissileReplication(ISpellMissile m)
        {
            var misPacket = ConstructMissileReplicationPacket(m);

            if (!m.IsServerOnly)
            {
                _packetHandlerManager.BroadcastPacketVision(m, misPacket.GetBytes(), Channel.CHL_S2C);
            }
            else
            {
                _packetHandlerManager.BroadcastPacket(misPacket.GetBytes(), Channel.CHL_S2C);
            }
        }

        public void NotifyS2C_CameraBehavior(IChampion target, Vector3 position)
        {
            var packet = new S2C_CameraBehavior
            {
                SenderNetID = target.NetId,
                Position = position
            };

            _packetHandlerManager.SendPacket((int)target.GetPlayerId(), packet.GetBytes(), Channel.CHL_S2C);
        }
        /// <summary>
        /// Sends a packet to all players that updates the specified unit's model.
        /// </summary>
        /// <param name="obj">AttackableUnit to update.</param>
        /// <param name="userId">UserId to send the packet to. If not specified or zero, the packet is broadcasted to all players that have vision of the specified unit.</param>
        /// <param name="skinID">Unit's skin ID after changing model.</param>
        /// <param name="modelOnly">Wether or not it's only the model that it's being changed(?). I don't really know what's this for</param>
        /// <param name="overrideSpells">Wether or not the user's spells should be overriden, i assume it would be used for things like Nidalee or Elise.</param>
        /// <param name="replaceCharacterPackage">Unknown.</param>
        public void NotifyS2C_ChangeCharacterData(IAttackableUnit obj, int userId = 0, uint skinID = 0, bool modelOnly = true, bool overrideSpells = false, bool replaceCharacterPackage = false)
        {
            var newCharData = new S2C_ChangeCharacterData
            {
                SenderNetID = obj.NetId,
                Data = new CharacterStackData
                {
                    SkinID = skinID,
                    SkinName = obj.Model,
                    OverrideSpells = overrideSpells,
                    ModelOnly = modelOnly,
                    ReplaceCharacterPackage = replaceCharacterPackage
                    // TODO: ID variable, acts like a character ID, used later on in PopCharacterData packet for unloading.
                    // Changes over time, or perhaps as new objects are added, does not have large values like NetID.
                }
            };

            if (userId == 0)
            {
                _packetHandlerManager.BroadcastPacketVision(obj, newCharData.GetBytes(), Channel.CHL_S2C);
            }
            else
            {
                _packetHandlerManager.SendPacket(userId, newCharData.GetBytes(), Channel.CHL_S2C);
            }
        }

        /// <summary>
        /// Sends a packet to the specified player detailing that the specified debug object's radius has changed.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="sender">NetId of the GameObject that is responsible for this packet being sent.</param>
        /// <param name="objID">ID of the Debug Object.</param>
        /// <param name="newRadius">New radius of the Debug Object.</param>
        public void NotifyModifyDebugCircleRadius(int userId, uint sender, int objID, float newRadius)
        {
            var debugPacket = new S2C_ModifyDebugCircleRadius
            {
                SenderNetID = sender,
                ObjectID = objID,
                Radius = newRadius
            };
            _packetHandlerManager.SendPacket(userId, debugPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified player detailing that the specified Debug Object's color has changed.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="sender">NetId of the GameObject responsible for this packet being sent.</param>
        /// <param name="objID">ID of the Debug Object.</param>
        /// <param name="r">Red hex value of the Debug Object.</param>
        /// <param name="g">Green hex value of the Debug Object.</param>
        /// <param name="b">Blue hex value of the Debug Object.</param>
        public void NotifyModifyDebugObjectColor(int userId, uint sender, int objID, byte r, byte g, byte b)
        {
            var debugObjPacket = new S2C_ModifyDebugObjectColor
            {
                SenderNetID = sender,
                ObjectID = objID
            };
            var color = new LeaguePackets.Game.Common.Color();
            color.Red = r;
            color.Green = g;
            color.Blue = b;
            debugObjPacket.Color = color;
            _packetHandlerManager.SendPacket(userId, debugObjPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players detailing that the specified unit has had their shield values modified.
        /// </summary>
        /// <param name="unit">Unit who's shield is being modified.</param>
        /// <param name="amount">Shield amount.</param>
        /// <param name="IsPhysical">Whether or not the shield being modified is of the Physical type.</param>
        /// <param name="IsMagical">Whether or not the shield being modified is of the Magical type.</param>
        /// <param name="StopShieldFade">Whether the shield should stay static or fade.</param>
        public void NotifyModifyShield(IAttackableUnit unit, float amount, bool IsPhysical, bool IsMagical, bool StopShieldFade)
        {
            var mods = new ModifyShield
            {
                SenderNetID = unit.NetId,
                Physical = IsPhysical,
                Magical = IsMagical,
                Amount = amount
            };
            _packetHandlerManager.BroadcastPacket(mods.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players detailing the movement driver homing data for the given unit.
        /// Used to sync homing (target-based) dashes between client and server.
        /// </summary>
        /// <param name="unit">Unit to sync.</param>
        public void NotifyMovementDriverReplication(IObjAiBase unit)
        {
            var targetPos = unit.TargetUnit.Position;
            if (targetPos == new Vector2(float.NaN, float.NaN))
            {
                return;
            }

            var current = unit.GetPosition3D();
            var to = Vector3.Normalize(new Vector3(targetPos.X, _navGrid.GetHeightAtLocation(targetPos.X, targetPos.Y), targetPos.Y) - current);

            var hd = new MovementDriverHomingData
            {
                TargetNetID = unit.TargetUnit.NetId,
                TargetHeightModifier = 0, // TODO: Verify
                TargetPosition = unit.TargetUnit.GetPosition3D(),
                Speed = unit.MovementParameters.PathSpeedOverride,
                Gravity = 0, // TODO: Implement gravity for AttackableUnits.
                RateOfTurn = 1.0f, // TODO: Implement TurnRate.
                Duration = unit.MovementParameters.FollowTravelTime,
                MovementPropertyFlags = 0 // TODO: Implement MovementPropertyFlags.
            };

            var replication = new MovementDriverReplication
            {
                SenderNetID = unit.NetId,
                MovementTypeID = 0, // TODO: Find out these values and place in GameServerCore.Enums.MovementTypeID
                Position = unit.GetPosition3D(),
                Velocity = new Vector3(to.X * unit.MovementParameters.PathSpeedOverride, 0, to.Y * unit.MovementParameters.PathSpeedOverride),
                MovementDriverHomingData = hd
            };

            // Homing projectiles are visible regardless of vision.
            _packetHandlerManager.BroadcastPacket(replication.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players who have vision of the specified buff's target detailing that the buff has been added to the target.
        /// </summary>
        /// <param name="b">Buff being added.</param>
        public void NotifyNPC_BuffAdd2(IBuff b)
        {
            var addPacket = new NPC_BuffAdd2
            {
                SenderNetID = b.TargetUnit.NetId,
                BuffSlot = b.Slot,
                BuffType = (byte)b.BuffType,
                Count = (byte)b.StackCount,
                IsHidden = b.IsHidden,
                BuffNameHash = HashString(b.Name),
                PackageHash = b.TargetUnit.GetObjHash(),
                RunningTime = b.TimeElapsed,
                Duration = b.Duration,
            };
            if (b.SourceUnit != null)
            {
                addPacket.CasterNetID = b.SourceUnit.NetId;
            }
            _packetHandlerManager.BroadcastPacket(addPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players with vision of the specified ObjAiBase detailing that the specified group of buffs has been added to the ObjAiBase.
        /// </summary>
        /// <param name="target">Unit that is receiving the group of buffs.</param>
        /// <param name="buffs">Group of buffs being added to the target.</param>
        /// <param name="buffType">Type of buff that applies to the entire group of buffs.</param>
        /// <param name="buffName">Internal name of the buff that applies to the group of buffs.</param>
        /// <param name="runningTime">Time that has passed since the group of buffs was created.</param>
        /// <param name="duration">Total amount of time the group of buffs should be active.</param>
        public void NotifyNPC_BuffAddGroup(IAttackableUnit target, List<IBuff> buffs, BuffType buffType, string buffName, float runningTime, float duration)
        {
            var addGroupPacket = new NPC_BuffAddGroup
            {
                SenderNetID = 0,
                BuffType = (byte)buffType,
                BuffNameHash = HashFunctions.HashString(buffName),
                PackageHash = target.GetObjHash(), // TODO: Verify
                RunningTime = runningTime,
                Duration = duration
            };
            var entries = new List<BuffAddGroupEntry>();
            for (int i = 0; i < buffs.Count; i++)
            {
                var entry = new BuffAddGroupEntry
                {
                    OwnerNetID = buffs[i].TargetUnit.NetId,
                    CasterNetID = 0,
                    Slot = buffs[i].Slot,
                    Count = (byte)buffs[i].StackCount,
                    IsHidden = buffs[i].IsHidden
                };

                if (buffs[i].OriginSpell != null)
                {
                    entry.CasterNetID = buffs[i].OriginSpell.CastInfo.Owner.NetId;
                }

                entries.Add(entry);
            }
            addGroupPacket.Entries = entries;

            _packetHandlerManager.BroadcastPacketVision(target, addGroupPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players who have vision of the target of the specified buff detailing that the buff was removed from its target.
        /// </summary>
        /// <param name="b">Buff that was removed.</param>
        public void NotifyNPC_BuffRemove2(IBuff b)
        {
            var removePacket = new NPC_BuffRemove2
            {
                SenderNetID = b.TargetUnit.NetId, //TODO: Verify if this should change depending on the removal source
                BuffSlot = b.Slot,
                BuffNameHash = HashFunctions.HashString(b.Name),
                RunTimeRemove = b.Duration - b.TimeElapsed
            };
            _packetHandlerManager.BroadcastPacket(removePacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players with vision of the specified ObjAiBase detailing that the specified group of buffs is being removed from the ObjAiBase.
        /// </summary>
        /// <param name="target">Unit getting their group of buffs removed.</param>
        /// <param name="buffs">Group of buffs getting removed.</param>
        /// <param name="buffName">Internal name of the buff that is applicable to the entire group of buffs.</param>
        public void NotifyNPC_BuffRemoveGroup(IAttackableUnit target, List<IBuff> buffs, string buffName)
        {
            var removeGroupPacket = new NPC_BuffRemoveGroup
            {
                SenderNetID = 0,
                BuffNameHash = HashFunctions.HashString(buffName),
            };
            var entries = new List<BuffRemoveGroupEntry>();
            for (int i = 0; i < buffs.Count; i++)
            {
                var entry = new BuffRemoveGroupEntry
                {
                    OwnerNetID = buffs[i].TargetUnit.NetId,
                    Slot = buffs[i].Slot,
                    RunTimeRemove = buffs[i].Duration - buffs[i].TimeElapsed
                };
                entries.Add(entry);
            }
            removeGroupPacket.Entries = entries;

            _packetHandlerManager.BroadcastPacket(removeGroupPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players with vision of the target of the specified buff detailing that the buff previously in the same slot was replaced by the newly specified buff.
        /// </summary>
        /// <param name="b">Buff that will replace the old buff in the same slot.</param>
        public void NotifyNPC_BuffReplace(IBuff b)
        {
            var replacePacket = new NPC_BuffReplace
            {
                SenderNetID = b.TargetUnit.NetId,
                BuffSlot = b.Slot,
                RunningTime = b.TimeElapsed,
                Duration = b.Duration,
                CasterNetID = 0
            };

            if (b.OriginSpell != null)
            {
                replacePacket.CasterNetID = b.OriginSpell.CastInfo.Owner.NetId;
            }

            _packetHandlerManager.BroadcastPacketVision(b.TargetUnit, replacePacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players with vision of the specified unit detailing that the buffs already occupying the slots of the group of buffs were replaced by the newly specified group of buffs.
        /// </summary>
        /// <param name="target">Unit getting their group of buffs replaced.</param>
        /// <param name="buffs">Group of buffs replacing buffs in the same slots.</param>
        /// <param name="runningtime">Time since the group of buffs was created.</param>
        /// <param name="duration">Total time the group of buffs should be active.</param>
        public void NotifyNPC_BuffReplaceGroup(IAttackableUnit target, List<IBuff> buffs, float runningtime, float duration)
        {
            var replaceGroupPacket = new NPC_BuffReplaceGroup
            {
                SenderNetID = 0,
                RunningTime = runningtime,
                Duration = duration
            };
            var entries = new List<BuffReplaceGroupEntry>();
            for (int i = 0; i < buffs.Count; i++)
            {
                var entry = new BuffReplaceGroupEntry
                {
                    OwnerNetID = buffs[i].TargetUnit.NetId,
                    CasterNetID = 0,
                    Slot = buffs[i].Slot
                };

                if (buffs[i].OriginSpell != null)
                {
                    entry.CasterNetID = buffs[i].OriginSpell.CastInfo.Owner.NetId;
                }

                entries.Add(entry);
            }
            replaceGroupPacket.Entries = entries;

            _packetHandlerManager.BroadcastPacketVision(target, replaceGroupPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players with vision of the target of the specified buff detailing an update to the number of buffs in the specified buff's slot
        /// </summary>
        /// <param name="b">Buff who's count is being updated.</param>
        /// <param name="duration">Total time the buff should last.</param>
        /// <param name="runningTime">Time since the buff's creation.</param>
        public void NotifyNPC_BuffUpdateCount(IBuff b, float duration, float runningTime)
        {
            var updatePacket = new NPC_BuffUpdateCount
            {
                SenderNetID = b.TargetUnit.NetId,
                BuffSlot = b.Slot,
                Count = (byte)b.StackCount,
                Duration = duration,
                RunningTime = runningTime,
                CasterNetID = 0
            };
            if (b.SourceUnit != null)
            {
                updatePacket.CasterNetID = b.SourceUnit.NetId;
            }
            _packetHandlerManager.BroadcastPacketVision(b.TargetUnit, updatePacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players with vision of the specified target detailing an update to the number of buffs in each of the buff slots occupied by the specified group of buffs.
        /// </summary>
        /// <param name="target">Unit who's buffs will be updated.</param>
        /// <param name="buffs">Group of buffs to update.</param>
        /// <param name="duration">Total time the buff should last.</param>
        /// <param name="runningTime">Time since the buff's creation.</param>
        public void NotifyNPC_BuffUpdateCountGroup(IAttackableUnit target, List<IBuff> buffs, float duration, float runningTime)
        {
            var updateGroupPacket = new NPC_BuffUpdateCountGroup
            {
                SenderNetID = 0,
                Duration = duration,
                RunningTime = runningTime
            };
            var entries = new List<BuffUpdateCountGroupEntry>();
            for (int i = 0; i < buffs.Count; i++)
            {
                var entry = new BuffUpdateCountGroupEntry
                {
                    OwnerNetID = buffs[i].TargetUnit.NetId,
                    CasterNetID = 0,
                    BuffSlot = buffs[i].Slot,
                    Count = (byte)buffs[i].StackCount
                };

                if (buffs[i].OriginSpell != null)
                {
                    entry.CasterNetID = buffs[i].OriginSpell.CastInfo.Owner.NetId;
                }
                entries.Add(entry);
            }
            updateGroupPacket.Entries = entries;

            _packetHandlerManager.BroadcastPacketVision(target, updateGroupPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players with vision of the target of the specified buff detailing an update to the stack counter of the specified buff.
        /// </summary>
        /// <param name="b">Buff who's stacks will be updated.</param>
        public void NotifyNPC_BuffUpdateNumCounter(IBuff b)
        {
            var updateNumPacket = new NPC_BuffUpdateNumCounter
            {
                SenderNetID = b.TargetUnit.NetId,
                BuffSlot = b.Slot,
                Counter = b.StackCount // TODO: Verify if it allows stacks to go above 255 on the buff bar
            };
            _packetHandlerManager.BroadcastPacketVision(b.TargetUnit, updateNumPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players with vision of the owner of the specified spell detailing that a spell has been cast.
        /// </summary>
        /// <param name="s">Spell being cast.</param>
        public void NotifyNPC_CastSpellAns(ISpell s)
        {
            var castInfo = new CastInfo
            {
                SpellHash = (uint)s.GetId(),
                SpellNetID = s.CastInfo.SpellNetID,
                SpellLevel = s.CastInfo.SpellLevel,
                AttackSpeedModifier = s.CastInfo.AttackSpeedModifier,
                CasterNetID = s.CastInfo.Owner.NetId,
                SpellChainOwnerNetID = s.CastInfo.Owner.NetId,
                PackageHash = s.CastInfo.Owner.GetObjHash(),
                MissileNetID = s.CastInfo.MissileNetID,
                TargetPosition = s.CastInfo.TargetPosition,
                TargetPositionEnd = s.CastInfo.TargetPositionEnd,
                DesignerCastTime = s.CastInfo.DesignerCastTime,
                ExtraCastTime = s.CastInfo.ExtraCastTime,
                DesignerTotalTime = s.CastInfo.DesignerTotalTime,
                Cooldown = s.CastInfo.Cooldown,
                StartCastTime = s.CastInfo.StartCastTime,
                IsAutoAttack = s.CastInfo.IsAutoAttack,
                IsSecondAutoAttack = s.CastInfo.IsSecondAutoAttack,
                IsForceCastingOrChannel = s.CastInfo.IsForceCastingOrChannel,
                IsOverrideCastPosition = s.CastInfo.IsOverrideCastPosition,
                IsClickCasted = s.CastInfo.IsClickCasted,
                SpellSlot = s.CastInfo.SpellSlot,
                SpellCastLaunchPosition = s.CastInfo.SpellCastLaunchPosition,
                AmmoUsed = s.CastInfo.AmmoUsed,
                AmmoRechargeTime = s.CastInfo.AmmoRechargeTime
            };

            if (s.CastInfo.Targets.Count > 0)
            {
                s.CastInfo.Targets.ForEach(t =>
                {
                    if (t.Unit != null)
                    {
                        castInfo.Targets.Add(new CastInfo.Target() { UnitNetID = t.Unit.NetId, HitResult = (byte)t.HitResult });
                    }
                    else
                    {
                        castInfo.Targets.Add(new CastInfo.Target() { UnitNetID = 0, HitResult = (byte)t.HitResult });
                    }
                });
            }

            if (s.CastInfo.SpellLevel > 0)
            {
                castInfo.ManaCost = s.SpellData.ManaCost[s.CastInfo.SpellLevel];
            }
            else
            {
                castInfo.ManaCost = s.SpellData.ManaCost[0];
            }

            var castAnsPacket = new NPC_CastSpellAns
            {
                SenderNetID = s.CastInfo.Owner.NetId,
                CasterPositionSyncID = s.CastInfo.Owner.SyncId,
                // TODO: Find what this is (if false, causes CasterPositionSyncID to be used)
                Unknown1 = false,
                CastInfo = castInfo
            };
            _packetHandlerManager.BroadcastPacketVision(s.CastInfo.Owner, castAnsPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players detailing that the specified unit has been killed by the specified killer.
        /// </summary>
        /// <param name="data">Data of the death.</param>
        public void NotifyNPC_Die_Broadcast(IDeathData data)
        {
            var dieMapView = new NPC_Die_Broadcast
            {
                SenderNetID = data.Unit.NetId,
                DeathData = new DeathData
                {
                    BecomeZombie = data.BecomeZombie,
                    DieType = data.DieType,
                    KillerNetID = data.Killer.NetId,
                    DamageType = (byte)data.DamageType,
                    DamageSource = (byte)data.DamageSource,
                    DeathDuration = data.DeathDuration
                }
            };
            _packetHandlerManager.BroadcastPacket(dieMapView.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all users with vision of the given unit that it has been forced to die.
        /// </summary>
        /// <param name="unit"></param>
        public void NotifyNPC_ForceDead(IAttackableUnit unit, float duration)
        {
            var forceDead = new NPC_ForceDead
            {
                SenderNetID = unit.NetId,
                DeathDuration = duration
            };

            _packetHandlerManager.BroadcastPacketVision(unit, forceDead.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players that a champion has died and calls a death timer update packet.
        /// </summary>
        /// <param name="champion">Champion that died.</param>
        /// <param name="killer">Unit that killed the Champion.</param>
        /// <param name="goldFromKill">Amount of gold the killer received.</param>
        public void NotifyNPC_Hero_Die(IDeathData deathData)
        {
            NotifyS2C_UpdateDeathTimer(deathData.Unit as IChampion);

            var cd = new NPC_Hero_Die
            {
                SenderNetID = deathData.Unit.NetId,
                DeathData = new DeathData
                {
                    KillerNetID = deathData.Killer.NetId,
                    DieType = deathData.DieType,
                    DamageType = (byte)deathData.DamageType,
                    DamageSource = (byte)deathData.DamageSource,
                    BecomeZombie = deathData.BecomeZombie,
                    DeathDuration = (deathData.Unit as IChampion).RespawnTimer / 1000f
                }
            };
            _packetHandlerManager.BroadcastPacket(cd.GetBytes(), Channel.CHL_S2C);
        }
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
        /// TODO: Find a better way to implement these parameters
        public void NotifyNPC_InstantStop_Attack(IAttackableUnit attacker, bool isSummonerSpell,
            bool keepAnimating = false,
            bool destroyMissile = true,
            bool overrideVisibility = true,
            bool forceClient = false,
            uint missileNetID = 0)
        {
            var stopAttack = new NPC_InstantStop_Attack
            {
                SenderNetID = attacker.NetId,
                MissileNetID = missileNetID, //TODO: Fix MissileNetID, currently it only works when it is 0
                KeepAnimating = keepAnimating,
                DestroyMissile = destroyMissile,
                OverrideVisibility = overrideVisibility,
                IsSummonerSpell = isSummonerSpell,
                ForceDoClient = forceClient
            };
            _packetHandlerManager.BroadcastPacketVision(attacker, stopAttack.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players detailing that the specified Champion has leveled up.
        /// </summary>
        /// <param name="c">Champion which leveled up.</param>
        /// <param name="userId">UserId to send the packet to. If not specified or zero, the packet is broadcasted to all players that have vision of the specified unit.</param>
        public void NotifyNPC_LevelUp(IObjAiBase obj)
        {
            var levelUp = new NPC_LevelUp()
            {
                SenderNetID = obj.NetId,
                Level = obj.Stats.Level,
            };

            if(obj is IChampion ch)
            {
                // TODO: Typo >:(
                levelUp.AveliablePoints = ch.SkillPoints;
            }

            _packetHandlerManager.BroadcastPacketVision(obj, levelUp.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified player detailing that they have leveled up the specified skill.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="netId">NetId of the GameObject whos skill is being leveled up.</param>
        /// <param name="slot">Slot of the skill being leveled up.</param>
        /// <param name="level">Current level of the skill.</param>
        /// <param name="points">Number of skill points available after the skill has been leveled up.</param>
        public void NotifyNPC_UpgradeSpellAns(int userId, uint netId, byte slot, byte level, byte points)
        {
            var upgradeSpellPacket = new NPC_UpgradeSpellAns
            {
                SenderNetID = netId,
                Slot = slot,
                SpellLevel = level,
                SkillPoints = points
            };

            _packetHandlerManager.SendPacket(userId, upgradeSpellPacket.GetBytes(), Channel.CHL_GAMEPLAY);
        }

        /// <summary>
        /// Sends a packet to all users with vision of the given caster detailing that the given spell has been set to auto cast (as well as the spell in the critSlot) for the given caster.
        /// </summary>
        /// <param name="caster">Unit responsible for the autocasting.</param>
        /// <param name="spell">Spell to auto cast.</param>
        /// // TODO: Verify critSlot functionality
        /// <param name="critSlot">Optional spell slot to cast when a crit is going to occur.</param>
        public void NotifyNPC_SetAutocast(IObjAiBase caster, ISpell spell, byte critSlot = 0)
        {
            var autoCast = new NPC_SetAutocast
            {
                SenderNetID = caster.NetId,
                Slot = spell.CastInfo.SpellSlot,
                CritSlot = critSlot
            };

            if (critSlot == 0)
            {
                autoCast.CritSlot = autoCast.Slot;
            }

            _packetHandlerManager.BroadcastPacketVision(caster, autoCast.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the given user detailing that the given spell has been set to auto cast (as well as the spell in the critSlot) for the given caster.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="caster">Unit responsible for the autocasting.</param>
        /// <param name="spell">Spell to auto cast.</param>
        /// // TODO: Verify critSlot functionality
        /// <param name="critSlot">Optional spell slot to cast when a crit is going to occur.</param>
        public void NotifyNPC_SetAutocast(int userId, IObjAiBase caster, ISpell spell, byte critSlot = 0)
        {
            var autoCast = new NPC_SetAutocast
            {
                SenderNetID = caster.NetId,
                Slot = spell.CastInfo.SpellSlot,
                CritSlot = critSlot
            };

            if (critSlot == 0)
            {
                autoCast.CritSlot = autoCast.Slot;
            }

            _packetHandlerManager.SendPacket(userId, autoCast.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players with vision of the specified unit detailing that the specified unit's stats have been updated.
        /// </summary>
        /// <param name="u">Unit who's stats have been updated.</param>
        /// <param name="userId">UserId to send the packet to. If not specified or zero, the packet is broadcasted to all players that have vision of the specified unit.</param>
        /// <param name="partial">Whether or not the packet should be counted as a partial update (whether the stats have actually changed or not). *NOTE*: Use case for this parameter is unknown.</param>
        /// TODO: Replace with LeaguePackets and preferably move all uses of this function to a central EventHandler class (if one is fully implemented).
        public void NotifyOnReplication(IAttackableUnit u, int userId = 0, bool partial = true)
        {
            if (u.Replication != null)
            {
                var us = new OnReplication()
                {
                    SyncID = (uint)Environment.TickCount,
                    // TODO: Support multi-unit replication creation (perhaps via a separate function which takes in a list of units).
                    ReplicationData = new List<ReplicationData>(1){
                        u.Replication.GetData(partial)
                    }
                };
                var channel = Channel.CHL_LOW_PRIORITY;
                if (userId == 0)
                {
                    _packetHandlerManager.BroadcastPacketVision(u, us.GetBytes(), channel, PacketFlags.UNSEQUENCED);
                }
                else
                {
                    _packetHandlerManager.SendPacket(userId, us.GetBytes(), channel, PacketFlags.UNSEQUENCED);
                }
            }
        }

        /// <summary>
        /// Sends a packet to all players detailing that the game has paused.
        /// </summary>
        /// <param name="seconds">Amount of time till the pause ends.</param>
        /// <param name="showWindow">Whether or not to show a pause window.</param>
        public void NotifyPausePacket(ClientInfo player, int seconds, bool isTournament)
        {
            var pg = new PausePacket
            {
                //Check if sender ID should be the person that requested the pause or just 0
                SenderNetID = 0,
                ClientID = (int)player.ClientId,
                IsTournament = isTournament,
                PauseTimeRemaining = seconds
            };
            //I Assumed that, since the packet requires idividual client IDs, that it also sends the packets individually, by useing the SendPacket Channel, double check if that's valid.
            _packetHandlerManager.SendPacket((int)player.PlayerId, pg.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players detailing the specified client's loading screen progress.
        /// </summary>
        /// <param name="request">Info of the target client given via the client who requested loading screen progress.</param>
        /// <param name="clientInfo">Client info of the client who's progress is being requested.</param>
        public void NotifyPingLoadInfo(ClientInfo client, PingLoadInfoRequest request)
        {
            var response = new S2C_Ping_Load_Info
            {
                ConnectionInfo = new ConnectionInfo
                {
                    ClientID = request.ClientID,
                    Ping = request.Ping,
                    PlayerID = client.PlayerId,
                    ETA = request.ETA,
                    Ready = request.Ready,
                    Percentage = request.Percentage,
                    Count = request.Count
                },
            };
            //Logging->writeLine("loaded: %f, ping: %f, %f", loadInfo->loaded, loadInfo->ping, loadInfo->f3);
            _packetHandlerManager.BroadcastPacket(response.GetBytes(), Channel.CHL_LOW_PRIORITY, PacketFlags.NONE);
        }

        /// <summary>
        /// Sends a packet to all players that a champion has respawned.
        /// </summary>
        /// <param name="c">Champion that respawned.</param>
        public void NotifyHeroReincarnateAlive(IChampion c, float parToRestore)
        {
            var cr = new HeroReincarnateAlive
            {
                SenderNetID = c.NetId,
                Position = new Vector2(c.Position.X, c.Position.Y),
                PARValue = parToRestore
            };
            _packetHandlerManager.BroadcastPacket(cr.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified player detailing that the specified Debug Object has been removed.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="sender">NetId of the GameObject that is responsible for this packet being sent.</param>
        /// <param name="objID">Debug Object being removed.</param>
        public void NotifyRemoveDebugObject(int userId, uint sender, int objID)
        {
            var debugObjPacket = new S2C_RemoveDebugObject
            {
                SenderNetID = sender,
                ObjectID = objID
            };
            _packetHandlerManager.SendPacket(userId, debugObjPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players with vision of the specified AI detailing that item in the specified slot was removed (or the number of stacks of the item in that slot changed).
        /// </summary>
        /// <param name="ai">AI with the items.</param>
        /// <param name="slot">Slot of the item that was removed.</param>
        /// <param name="remaining">Number of stacks of the item left (0 if not applicable).</param>
        public void NotifyRemoveItem(IObjAiBase ai, byte slot, byte remaining)
        {
            var ria = new RemoveItemAns()
            {
                SenderNetID = ai.NetId,
                Slot = slot,
                ItemsInSlot = remaining,
                NotifyInventoryChange = true
            };
            _packetHandlerManager.BroadcastPacketVision(ai, ria.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players detailing that the specified region was removed.
        /// </summary>
        /// <param name="region">Region to remove.</param>
        public void NotifyRemoveRegion(IRegion region)
        {
            var removeRegion = new RemoveRegion()
            {
                RegionNetID = region.NetId
            };

            _packetHandlerManager.BroadcastPacket(removeRegion.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified player detailing that the highlight of the specified GameObject was removed.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="unit">GameObject that had the highlight.</param>
        public void NotifyRemoveUnitHighlight(int userId, IGameObject unit)
        {
            var highlightPacket = new S2C_RemoveUnitHighlight
            {
                SenderNetID = unit.NetId,
                NetID = unit.NetId
            };
            _packetHandlerManager.SendPacket(userId, highlightPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified user detailing skin and player name information of the specified player on the loading screen.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="player">Player information to send.</param>
        public void NotifyRequestRename(int userId, Tuple<uint, ClientInfo> player)
        {
            var loadName = new RequestRename
            {
                PlayerID = player.Item2.PlayerId,
                PlayerName = player.Item2.Name,
                // Most packets show a large default value (in place of what you would expect to be 0)
                // Seems to be randomized per-game and used for every RequestRename packet during that game.
                // So, using this SkinNo may be incorrect.
                SkinID = player.Item2.SkinNo,
            };
            _packetHandlerManager.SendPacket(userId, loadName.GetBytes(), Channel.CHL_LOADING_SCREEN);
        }

        /// <summary>
        /// Sends a packet to the specified user detailing skin information of the specified player on the loading screen.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="player">Player information to send.</param>
        public void NotifyRequestReskin(int userId, Tuple<uint, ClientInfo> player)
        {
            var loadChampion = new RequestReskin
            {
                PlayerID = player.Item2.PlayerId,
                SkinID = player.Item2.SkinNo,
                SkinName = player.Item2.Champion.Model
            };
            _packetHandlerManager.SendPacket(userId, loadChampion.GetBytes(), Channel.CHL_LOADING_SCREEN);
        }

        /// <summary>
        /// Sends a packet to all players detailing that the game has been unpaused.
        /// </summary>
        /// <param name="unpauser">Unit that unpaused the game.</param>
        /// <param name="showWindow">Whether or not to show a window before unpausing (delay).</param>
        public void NotifyResumePacket(IChampion unpauser, ClientInfo player, bool isDelayed)
        {
            var resume = new ResumePacket
            {
                Delayed = isDelayed,
                ClientID = (int)player.ClientId
            };
            if (unpauser == null)
            {
                resume.SenderNetID = 0;
            }
            else
            {
                resume.SenderNetID = unpauser.NetId;
            }

            _packetHandlerManager.SendPacket((int)player.PlayerId, resume.GetBytes(), Channel.CHL_S2C);
        }

        public void NotifyS2C_ActivateMinionCamp(IMonsterCamp monsterCamp)
        {
            var packet = new S2C_ActivateMinionCamp
            {
                SenderNetID = 0,
                Position = monsterCamp.Position,
                CampIndex = monsterCamp.CampIndex,
                SpawnDuration = monsterCamp.SpawnDuration,
                TimerType = monsterCamp.TimerType
            };
            _packetHandlerManager.BroadcastPacket(packet.GetBytes(), Channel.CHL_S2C);
        }

        public void NotifyS2C_AmmoUpdate(ISpell spell)
        {
            if(spell.CastInfo.Owner is IChampion ch)
            {
                var packet = new S2C_AmmoUpdate
                {
                    IsSummonerSpell = spell.SpellName.StartsWith("Summoner"),
                    SpellSlot = spell.CastInfo.SpellSlot,
                    CurrentAmmo = spell.CurrentAmmo,
                    // TODO: Implement this. Example spell which uses it is Syndra R.
                    MaxAmmo = -1,
                    SenderNetID = spell.CastInfo.Owner.NetId
                };

                if (spell.CurrentAmmo < spell.SpellData.MaxAmmo)
                {
                    packet.AmmoRecharge = spell.CurrentAmmoCooldown;
                    packet.AmmoRechargeTotalTime = spell.GetAmmoRechageTime();
                }

              _packetHandlerManager.SendPacket((int)ch.GetPlayerId(), packet.GetBytes(), Channel.CHL_S2C);
            }
        }

        /// <summary>
        /// Sends a packet to all players with vision of the given chain missile that it has updated (unit/position).
        /// </summary>
        /// <param name="p">Missile that should be synced.</param>
        public void NotifyS2C_ChainMissileSync(ISpellMissile m)
        {
            if (!m.HasTarget())
            {
                return;
            }

            var syncPacket = new S2C_ChainMissileSync()
            {
                SenderNetID = m.NetId,
                TargetCount = m.CastInfo.Targets.Count,
                // TODO: Verify
                OwnerNetworkID = m.CastInfo.Owner.NetId
            };

            for (int i = 0; i < syncPacket.TargetNetIDs.Length; i++)
            {
                if (m.CastInfo.Targets.Count == i)
                {
                    break;
                }

                syncPacket.TargetNetIDs[i] = m.CastInfo.Targets[i].Unit.NetId;
            }

            _packetHandlerManager.BroadcastPacketVision(m, syncPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players with vision of the given projectile that it has changed targets (unit/position).
        /// </summary>
        /// <param name="p">Projectile that has changed target.</param>
        public void NotifyS2C_ChangeMissileTarget(ISpellMissile p)
        {
            if (!p.HasTarget())
            {
                return;
            }

            var changePacket = new S2C_ChangeMissileTarget()
            {
                SenderNetID = p.NetId,
                TargetNetID = 0,
                TargetPosition = new Vector3(p.GetTargetPosition().X, _navGrid.GetHeightAtLocation(p.GetTargetPosition()), p.GetTargetPosition().Y)
            };

            if (p.CastInfo.Targets.Count > 0)
            {
                if (p.CastInfo.Targets[0].Unit != null)
                {
                    changePacket.TargetNetID = p.CastInfo.Targets[0].Unit.NetId;
                }
            }

            _packetHandlerManager.BroadcastPacketVision(p, changePacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified user or all users detailing that the hero designated to the given clientInfo has been created.
        /// </summary>
        /// <param name="clientInfo">Information about the client which had their hero created.</param>
        /// <param name="userId">User to send the packet to. Set to -1 to broadcast.</param>
        public void NotifyS2C_CreateHero(ClientInfo clientInfo, int userId = -1)
        {
            var champion = clientInfo.Champion;
            var heroPacket = new S2C_CreateHero()
            {
                NetID = champion.NetId,
                ClientID = (int)clientInfo.ClientId,
                // NetNodeID,
                // For bots (0 = Beginner, 1 = Intermediate)
                SkillLevel = 0,
                // TODO: Implement bots and unhardcode this.
                IsBot = champion.IsBot,
                // BotRank, deprecated as of v4.18
                // TODO: Unhardcode
                SpawnPositionIndex = 0,
                SkinID = champion.SkinID,
                Name = clientInfo.Name,
                Skin = champion.Model,
                DeathDurationRemaining = champion.RespawnTimer,
                // TimeSinceDeath
            };

            if (champion.Team == TeamId.TEAM_BLUE)
            {
                heroPacket.TeamIsOrder = true;
            }
            else
            {
                heroPacket.TeamIsOrder = false;
            }

            if (userId < 0)
            {
                _packetHandlerManager.BroadcastPacket(heroPacket.GetBytes(), Channel.CHL_S2C);
                return;
            }

            _packetHandlerManager.SendPacket(userId, heroPacket.GetBytes(), Channel.CHL_S2C);
        }
        public void NotifyS2C_CreateMinionCamp(IMonsterCamp monsterCamp)
        {
            var packet = new S2C_CreateMinionCamp
            {
                Position = monsterCamp.Position,
                SenderNetID = 0,
                CampIndex = monsterCamp.CampIndex,
                MinimapIcon = monsterCamp.MinimapIcon,
                RevealAudioVOComponentEvent = monsterCamp.RevealEvent,
                SideTeamID = monsterCamp.SideTeamId,
                Expire = monsterCamp.Expire,
                TimerType = monsterCamp.TimerType
            };
            _packetHandlerManager.BroadcastPacket(packet.GetBytes(), Channel.CHL_S2C);
        }

        public void NotifyS2C_CreateNeutral(IMonster monster, float time)
        {
            var packet = ConstructCreateNeutralPacket(monster, time);
            _packetHandlerManager.BroadcastPacket(packet.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to either all players or the specified player detailing that the specified LaneTurret has spawned.
        /// </summary>
        /// <param name="turret">LaneTurret that spawned.</param>
        /// <param name="userId">User to send the packet to.</param>
        public void NotifyS2C_CreateTurret(ILaneTurret turret, int userId = 0)
        {
            var createTurret = ConstructCreateTurretPacket(turret);

            if (userId != 0)
            {
                _packetHandlerManager.SendPacket(userId, createTurret.GetBytes(), Channel.CHL_S2C);
                return;
            }

            _packetHandlerManager.BroadcastPacket(createTurret.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Disables the U.I when the game ends 
        /// </summary>
        /// <param name="player"></param>
        public void NotifyS2C_DisableHUDForEndOfGame(Tuple<uint, ClientInfo> player)
        {
            var disableHud = new S2C_DisableHUDForEndOfGame { SenderNetID = 0 };
            _packetHandlerManager.SendPacket((int)player.Item2.PlayerId, disableHud.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends packets to all players notifying the result of a match (Victory or defeat)
        /// </summary>
        /// <param name="losingTeam">The Team that lost the match</param>
        /// <param name="time">The offset for the result to actually be displayed</param>
        public void NotifyS2C_EndGame(TeamId losingTeam)
        {
            var gameEndPacket = new S2C_EndGame
            {
                IsTeamOrderWin = losingTeam != TeamId.TEAM_BLUE
            };
            _packetHandlerManager.BroadcastPacket(gameEndPacket.GetBytes(), Channel.CHL_S2C);
        }

        public void NotifyS2C_HandleCapturePointUpdate(byte capturePointIndex, uint otherNetId, byte PARType, byte attackTeam, CapturePointUpdateCommand capturePointUpdateCommand)
        {
            var packet = new S2C_HandleCapturePointUpdate
            {
                CapturePointIndex = capturePointIndex,
                OtherNetID = otherNetId,
                PARType = PARType,
                AttackTeam = attackTeam,
                CapturePointUpdateCommand = (byte)capturePointUpdateCommand
            };

            _packetHandlerManager.BroadcastPacket(packet.GetBytes(), Channel.CHL_S2C, PacketFlags.NONE);
        }

        /// <summary>
        /// Notifies the game about a score
        /// </summary>
        /// <param name="team"></param>
        /// <param name="score"></param>
        public void NotifyS2C_HandleGameScore(TeamId team, int score)
        {
            var packet = new S2C_HandleGameScore
            {
                TeamID = (uint)team,
                Score = score
            };

            _packetHandlerManager.BroadcastPacket(packet.GetBytes(), Channel.CHL_S2C, PacketFlags.NONE);
        }

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
        public void NotifyS2C_HandleTipUpdatep(int userId, string title, string text, string imagePath, byte tipCommand, uint playerNetId, uint targetNetId)
        {
            var packet = new S2C_HandleTipUpdate
            {
                SenderNetID = playerNetId,
                TipCommand = tipCommand,
                TipImagePath = imagePath,
                TipName = title,
                TipOther = text,
                TipID = targetNetId
            };
            _packetHandlerManager.SendPacket(userId, packet.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players detailing the stats (CS, kills, deaths, etc) of the player who owns the specified Champion.
        /// </summary>
        /// <param name="champion">Champion owned by the player.</param>
        public void NotifyS2C_HeroStats(IChampion champion)
        {
            var response = new S2C_HeroStats { Data = champion.ChampStats.GetBytes() };
            // TODO: research how to send the packet
            _packetHandlerManager.BroadcastPacket(response.GetBytes(), Channel.CHL_S2C);
        }

        public void NotifyS2C_IncrementPlayerScore(IScoreData scoreData)
        {
            var packet = new S2C_IncrementPlayerScore
            {
                PlayerNetID = scoreData.Owner.NetId,
                TotalPointValue = scoreData.Owner.Stats.Points,
                PointValue = scoreData.Points,
                ShouldCallout = scoreData.DoCallOut,
                ScoreCategory = (byte)scoreData.ScoreCategory,
                ScoreEvent = (byte)scoreData.ScoreEvent
            };

            _packetHandlerManager.BroadcastPacketVision(scoreData.Owner, packet.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified client's team detailing a map ping.
        /// </summary>
        /// <param name="client">Info of the client that initiated the ping.</param>
        /// <param name="pos">2D top-down position of the ping.</param>
        /// <param name="targetNetId">Target of the ping (if applicable).</param>
        /// <param name="type">Type of ping; COMMAND/ATTACK/DANGER/MISSING/ONMYWAY/FALLBACK/REQUESTHELP. *NOTE*: Not all ping types are supported yet.</param>
        public void NotifyS2C_MapPing(Vector2 pos, Pings type, uint targetNetId = 0, ClientInfo client = null)
        {
            var response = new S2C_MapPing
            {
                // TODO: Verify if this is correct. Usually 0.

                TargetNetID = targetNetId,
                PingCategory = (byte)type,
                Position = pos,
                //Unhardcode these bools later
                PlayAudio = true,
                ShowChat = true,
                PingThrottled = false,
                PlayVO = true
            };

            if (targetNetId != 0)
            {
                response.TargetNetID = targetNetId;
            }

            if (client != null)
            {
                response.SenderNetID = client.Champion.NetId;
                response.SourceNetID = client.Champion.NetId;
                _packetHandlerManager.BroadcastPacketTeam(client.Team, response.GetBytes(), Channel.CHL_S2C);
            }
            else
            {
                _packetHandlerManager.BroadcastPacket(response.GetBytes(), Channel.CHL_S2C);
            }
        }

        /// <summary>
        /// Sends a packet to the specified player which forces their camera to move to a specified point given certain parameters.
        /// </summary>
        /// <param name="player">Player who'll it's camera moved</param>
        /// <param name="startPosition">The starting position of the camera (Not yet known how to get it's values)</param>
        /// <param name="endPosition">End point to where the camera will move</param>
        /// <param name="travelTime">The time the camera will have to travel the given distance</param>
        /// <param name="startFromCurretPosition">Wheter or not it starts from current position</param>
        /// <param name="unlockCamera">Whether or not the camera is unlocked</param>
        public void NotifyS2C_MoveCameraToPoint(Tuple<uint, ClientInfo> player, Vector3 startPosition, Vector3 endPosition, float travelTime = 0, bool startFromCurretPosition = true, bool unlockCamera = false)
        {
            var cam = new S2C_MoveCameraToPoint
            {
                SenderNetID = player.Item2.Champion.NetId,
                StartFromCurrentPosition = startFromCurretPosition,
                UnlockCamera = unlockCamera,
                TravelTime = travelTime,
                TargetPosition = endPosition
            };
            if (startPosition != Vector3.Zero)
            {
                cam.StartPosition = startPosition;
            }

            _packetHandlerManager.SendPacket((int)player.Item2.PlayerId, cam.GetBytes(), Channel.CHL_S2C);
        }
        public void NotifyS2C_Neutral_Camp_Empty(IMonsterCamp monsterCamp, IDeathData deathData = null)
        {
            var packet = new S2C_Neutral_Camp_Empty
            {
                SenderNetID = 0,
                KillerNetID = 0,
                //Investigate what this does, from what i see on packets, my guess is a check if the enemy team had vision of the camp dying
                DoPlayVO = true,
                CampIndex = monsterCamp.CampIndex,
                TimerType = monsterCamp.TimerType,
                //Check what the hell this is for
                TimerExpire = 0.0f
            };
            if (deathData != null)
            {
                packet.KillerNetID = deathData.Killer.NetId;
            }
            _packetHandlerManager.BroadcastPacket(packet.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players detailing that the specified unit has been killed by the specified killer.
        /// </summary>
        /// <param name="data">Data of the death.</param>
        public void NotifyS2C_NPC_Die_MapView(IDeathData data)
        {
            var dieMapView = new S2C_NPC_Die_MapView
            {
                SenderNetID = data.Unit.NetId,
                DeathData = new DeathData
                {
                    BecomeZombie = data.BecomeZombie,
                    DieType = data.DieType,
                    KillerNetID = data.Killer.NetId,
                    DamageType = (byte)data.DamageType,
                    DamageSource = (byte)data.DamageSource,
                    DeathDuration = data.DeathDuration
                }
            };
            _packetHandlerManager.BroadcastPacket(dieMapView.GetBytes(), Channel.CHL_S2C);
        }

        S2C_OnEnterTeamVisibility ConstructOnEnterTeamVisibilityPacket(IGameObject o, TeamId team)
        {
            var enterTeamVis = new S2C_OnEnterTeamVisibility()
            {
                SenderNetID = o.NetId
            };

            enterTeamVis.VisibilityTeam = 0;
            if (team == TeamId.TEAM_PURPLE || team == TeamId.TEAM_NEUTRAL)
            {
                enterTeamVis.VisibilityTeam = 1;
            }

            return enterTeamVis;
        }

        /// <summary>
        /// Sends a packet to either all players with vision of the specified GameObject or a specified user.
        /// The packet contains details of which team gained visibility of the GameObject and should only be used after it is first initialized into vision (NotifyEnterVisibility).
        /// </summary>
        /// <param name="o">GameObject coming into vision.</param>
        /// <param name="userId">User to send the packet to.</param>
        public void NotifyS2C_OnEnterTeamVisibility(IGameObject o, TeamId team, int userId = 0)
        {
            var enterTeamVis = ConstructOnEnterTeamVisibilityPacket(o, team);

            if (userId == 0)
            {
                // TODO: Verify if we should use BroadcastPacketTeam instead.
                _packetHandlerManager.BroadcastPacket(enterTeamVis.GetBytes(), Channel.CHL_S2C);
            }
            else
            {
                _packetHandlerManager.SendPacket(userId, enterTeamVis.GetBytes(), Channel.CHL_S2C);
            }
        }

        /// <summary>
        /// Sends a packet to all players that announces a specified message (ex: "Minions have spawned.")
        /// </summary>
        /// <param name="eventId">Id of the event to happen.</param>
        /// <param name="sourceNetID">Not yet know it's use.</param>
        public void NotifyS2C_OnEventWorld(IEvent mapEvent, uint sourceNetId = 0)
        {
            if (mapEvent == null)
            {
                return;
            }
            var packet = new S2C_OnEventWorld
            {
                SenderNetID = 0,
                EventWorld = new EventWorld
                {
                    Event = mapEvent,
                    Source = sourceNetId
                }
            };
            _packetHandlerManager.BroadcastPacket(packet.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to either all players with vision of the specified GameObject or a specified user.
        /// The packet contains details of which team lost visibility of the GameObject and should only be used after it is first initialized into vision (NotifyEnterVisibility).
        /// </summary>
        /// <param name="o">GameObject going out of vision.</param>
        /// <param name="userId">User to send the packet to.</param>
        public void NotifyS2C_OnLeaveTeamVisibility(IGameObject o, TeamId team, int userId = 0)
        {
            var leaveTeamVis = new S2C_OnLeaveTeamVisibility()
            {
                SenderNetID = o.NetId
            };

            leaveTeamVis.VisibilityTeam = 0;
            if (team == TeamId.TEAM_PURPLE || team == TeamId.TEAM_NEUTRAL)
            {
                leaveTeamVis.VisibilityTeam = 1;
            }

            if (userId == 0)
            {
                // TODO: Verify if we should use BroadcastPacketTeam instead.
                _packetHandlerManager.BroadcastPacket(leaveTeamVis.GetBytes(), Channel.CHL_S2C);
            }
            else
            {
                _packetHandlerManager.SendPacket(userId, leaveTeamVis.GetBytes(), Channel.CHL_S2C);
            }
        }

        /// <summary>
        /// Sends a packet to all players detailing that the specified object's current animations have been paused/unpaused.
        /// </summary>
        /// <param name="obj">GameObject that is playing the animation.</param>
        /// <param name="pause">Whether or not to pause/unpause animations.</param>
        public void NotifyS2C_PauseAnimation(IGameObject obj, bool pause)
        {
            var animPacket = new S2C_PauseAnimation
            {
                SenderNetID = obj.NetId,
                Pause = pause
            };

            _packetHandlerManager.BroadcastPacket(animPacket.GetBytes(), Channel.CHL_S2C);
        }

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
        public void NotifyS2C_PlayAnimation(IGameObject obj, string animation, AnimationFlags flags = 0, float timeScale = 1.0f, float startTime = 0.0f, float speedScale = 1.0f)
        {
            var animPacket = new S2C_PlayAnimation
            {
                SenderNetID = obj.NetId,
                AnimationFlags = (byte)flags,
                ScaleTime = timeScale,
                StartProgress = startTime,
                SpeedRatio = speedScale,
                AnimationName = animation
            };

            _packetHandlerManager.BroadcastPacketVision(obj, animPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players detailing an emotion that is being performed by the unit that owns the specified netId.
        /// </summary>
        /// <param name="type">Type of emotion being performed; DANCE/TAUNT/LAUGH/JOKE/UNK.</param>
        /// <param name="netId">NetID of the unit performing the emotion.</param>
        public void NotifyS2C_PlayEmote(Emotions type, uint netId)
        {
            // convert type
            EmoteID targetType;
            switch (type)
            {
                case Emotions.DANCE:
                    targetType = EmoteID.Dance;
                    break;
                case Emotions.TAUNT:
                    targetType = EmoteID.Taunt;
                    break;
                case Emotions.LAUGH:
                    targetType = EmoteID.Laugh;
                    break;
                case Emotions.JOKE:
                    targetType = EmoteID.Joke;
                    break;
                case Emotions.UNK:
                    targetType = (EmoteID)type;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            var packet = new S2C_PlayEmote
            {
                SenderNetID = netId,
                EmoteID = (byte)targetType
            };
            _packetHandlerManager.BroadcastPacket(packet.GetBytes(), Channel.CHL_S2C);
        }

        public void NotifyS2C_PlaySound(string soundName, IAttackableUnit soundOwner)
        {
            var packet = new S2C_PlaySound
            {
                SoundName = soundName,
                OwnerNetID = soundOwner.NetId
            };

            _packetHandlerManager.BroadcastPacket(packet.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified player which is meant as a response to the players query about the status of the game.
        /// </summary>
        /// <param name="userId">User to send the packet to; player that sent the query.</param>
        public void NotifyS2C_QueryStatusAns(int userId)
        {
            var response = new S2C_QueryStatusAns
            {
                SenderNetID = 0,
                Response = true
            };
            _packetHandlerManager.SendPacket(userId, response.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players with vision of the specified unit detailing that its animation states have changed to the specified animation pairs.
        /// Replaces the unit's normal animation behaviors with the given animation pairs. Structure of the animationPairs is expected to follow the same structure from before the replacement.
        /// </summary>
        /// <param name="u">AttackableUnit to change.</param>
        /// <param name="animationPairs">Dictionary of animations to set.</param>
        public void NotifyS2C_SetAnimStates(IAttackableUnit u, Dictionary<string, string> animationPairs)
        {
            var setAnimPacket = new S2C_SetAnimStates
            {
                SenderNetID = u.NetId,
                AnimationOverrides = animationPairs
            };

            _packetHandlerManager.BroadcastPacket(setAnimPacket.GetBytes(), Channel.CHL_S2C);
        }

        public void NotifyS2C_SetGreyscaleEnabledWhenDead(bool enabled, IAttackableUnit sender = null)
        {
            var packet = new S2C_SetGreyscaleEnabledWhenDead
            {
                Enabled = enabled,
            };

            if (sender != null)
            {
                packet.SenderNetID = sender.NetId;
            }
            _packetHandlerManager.BroadcastPacket(packet.GetBytes(), Channel.CHL_S2C);

        }

        public void NotifyS2C_SetInputLockFlag(int userId, InputLockFlags flags, bool enabled)
        {
            var inputLockPacket = new S2C_SetInputLockFlag
            {
                InputLockFlags = (uint)flags,
                Value = enabled
            };

            _packetHandlerManager.SendPacket(userId, inputLockPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified user detailing that the spell in the given slot has had its spelldata changed to the spelldata of the given spell name.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="netId">NetId of the unit that owns the spell being changed.</param>
        /// <param name="spellName">Internal name of the spell to grab spell data from (to set).</param>
        /// <param name="slot">Slot of the spell being changed.</param>
        public void NotifyS2C_SetSpellData(int userId, uint netId, string spellName, byte slot)
        {
            var spellDataPacket = new S2C_SetSpellData
            {
                SenderNetID = netId,
                ObjectNetID = netId,
                HashedSpellName = HashString(spellName),
                SpellSlot = slot
            };

            _packetHandlerManager.SendPacket(userId, spellDataPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified player detailing that the level of the spell in the given slot has changed.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="netId">NetId of the unit that owns the spell being changed.</param>
        /// <param name="slot">Slot of the spell being changed.</param>
        /// <param name="level">New level of the spell to set.</param>
        public void NotifyS2C_SetSpellLevel(int userId, uint netId, int slot, int level)
        {
            var spellLevelPacket = new S2C_SetSpellLevel
            {
                SenderNetID = netId,
                SpellSlot = slot,
                SpellLevel = level
            };

            _packetHandlerManager.SendPacket(userId, spellLevelPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified player detailing that the game has started the spawning GameObjects that occurs at the start of the game.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        public void NotifyS2C_StartSpawn(int userId = 0)
        {
            var start = new S2C_StartSpawn
            {
                // TODO: Set these values when bots are implemented.
                BotCountOrder = 0,
                BotCountChaos = 0
            };
            if (userId == 0)
            {
                _packetHandlerManager.BroadcastPacket(start.GetBytes(), Channel.CHL_S2C);
            }
            else
            {
                _packetHandlerManager.SendPacket(userId, start.GetBytes(), Channel.CHL_S2C);
            }
        }

        /// <summary>
        /// Sends a packet to all players detailing that the specified object has stopped playing an animation.
        /// </summary>
        /// <param name="obj">GameObject that is playing the animation.</param>
        /// <param name="animation">Internal name of the animation to stop.</param>
        /// <param name="stopAll">Whether or not to stop all animations. Only works if animation is empty/null.</param>
        /// <param name="fade">Whether or not the animation should fade before stopping.</param>
        /// <param name="ignoreLock">Whether or not locked animations should still be stopped.</param>
        public void NotifyS2C_StopAnimation(IGameObject obj, string animation, bool stopAll = false, bool fade = false, bool ignoreLock = true)
        {
            var animPacket = new S2C_StopAnimation
            {
                SenderNetID = obj.NetId,
                Fade = fade,
                IgnoreLock = ignoreLock,
                StopAll = stopAll,
                AnimationName = animation
            };

            _packetHandlerManager.BroadcastPacket(animPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the given user detailing that the specified input locking flags have been toggled.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="flags">InputLockFlags to toggle.</param>
        public void NotifyS2C_ToggleInputLockFlag(int userId, InputLockFlags flags)
        {
            var inputLockPacket = new S2C_ToggleInputLockFlag
            {
                InputLockFlags = (uint)flags
            };

            _packetHandlerManager.SendPacket(userId, inputLockPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players detailing spell tooltip parameters that the game does not inform automatically.
        /// </summary>
        /// <param name="data">The list of changed tool tip values.</param>
        public void NotifyS2C_ToolTipVars(List<IToolTipData> data)
        {
            List<TooltipVars> variables = new List<TooltipVars>();
            foreach (var tip in data)
            {
                var vars = new TooltipVars()
                {
                    OwnerNetID = tip.NetID,
                    SlotIndex = tip.Slot
                };

                for (var x = 0; x < tip.Values.Length; x++)
                {
                    vars.HideFromEnemy[x] = tip.Values[x].Hide;
                    vars.Values[x] = tip.Values[x].Value;
                }

                variables.Add(vars);
            }

            var answer = new S2C_ToolTipVars
            {
                Tooltips = variables
            };

            _packetHandlerManager.BroadcastPacket(answer.GetBytes(), Channel.CHL_S2C, PacketFlags.NONE);
        }

        /// <summary>
        /// Sends a packet to all players with vision of the specified attacker that it is looking at (targeting) the specified attacked unit with the given AttackType.
        /// </summary>
        /// <param name="attacker">Unit that is attacking.</param>
        /// <param name="attacked">Unit that is being attacked.</param>
        /// <param name="attackType">AttackType that the attacker is using to attack.</param>
        public void NotifyS2C_UnitSetLookAt(IAttackableUnit attacker, IAttackableUnit attacked, AttackType attackType)
        {
            var lookAtPacket = new S2C_UnitSetLookAt
            {
                SenderNetID = attacker.NetId,
                LookAtType = (byte)attackType,
                TargetPosition = attacked.GetPosition3D(),
                TargetNetID = attacked.NetId
            };

            _packetHandlerManager.BroadcastPacketVision(attacker, lookAtPacket.GetBytes(), Channel.CHL_S2C);
        }

        public void NotifyS2C_UpdateAscended(IObjAiBase ascendant = null)
        {
            var packet = new S2C_UpdateAscended();
            if (ascendant != null)
            {
                packet.AscendedNetID = ascendant.NetId;
            }
            _packetHandlerManager.BroadcastPacket(packet.GetBytes(), Channel.CHL_S2C, PacketFlags.NONE);
        }

        /// <summary>
        /// Sends a packet to all players detailing the attack speed cap overrides for this game.
        /// </summary>
        /// <param name="overrideMax">Whether or not to override the maximum attack speed cap.</param>
        /// <param name="maxAttackSpeedOverride">Value to override the maximum attack speed cap.</param>
        /// <param name="overrideMin">Whether or not to override the minimum attack speed cap.</param>
        /// <param name="minAttackSpeedOverride">Value to override the minimum attack speed cap.</param>
        public void NotifyS2C_UpdateAttackSpeedCapOverrides(bool overrideMax, float maxAttackSpeedOverride, bool overrideMin, float minAttackSpeedOverride, IAttackableUnit unit = null)
        {
            var overridePacket = new S2C_UpdateAttackSpeedCapOverrides
            {
                DoOverrideMax = overrideMax,
                DoOverrideMin = overrideMin,
                MaxAttackSpeedOverride = maxAttackSpeedOverride,
                MinAttackSpeedOverride = minAttackSpeedOverride
            };
            if (unit != null)
            {
                overridePacket.SenderNetID = unit.NetId;
            }
            _packetHandlerManager.BroadcastPacket(overridePacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players with vision of the given bounce missile that it has updated (unit/position).
        /// </summary>
        /// <param name="p">Missile that has been updated.</param>
        public void NotifyS2C_UpdateBounceMissile(ISpellMissile p)
        {
            if (!p.HasTarget())
            {
                return;
            }

            var changePacket = new S2C_UpdateBounceMissile()
            {
                SenderNetID = p.NetId,
                TargetNetID = 0,
                CasterPosition = p.CastInfo.SpellCastLaunchPosition
            };

            if (p.CastInfo.Targets.Count > 0)
            {
                if (p.CastInfo.Targets[0].Unit != null)
                {
                    changePacket.TargetNetID = p.CastInfo.Targets[0].Unit.NetId;
                }
            }

            _packetHandlerManager.BroadcastPacketVision(p, changePacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players updating a champion's death timer.
        /// </summary>
        /// <param name="champion">Champion that died.</param>
        public void NotifyS2C_UpdateDeathTimer(IChampion champion)
        {
            var cdt = new S2C_UpdateDeathTimer { SenderNetID = champion.NetId, DeathDuration = champion.RespawnTimer / 1000f };
            _packetHandlerManager.BroadcastPacket(cdt.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified user detailing that the specified spell's toggle state has been updated.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="s">Spell being updated.</param>
        public void NotifyS2C_UpdateSpellToggle(int userId, ISpell s)
        {
            var spellTogglePacket = new S2C_UpdateSpellToggle
            {
                SenderNetID = s.CastInfo.Owner.NetId,
                SpellSlot = s.CastInfo.SpellSlot,
                ToggleValue = s.Toggle
            };

            _packetHandlerManager.SendPacket(userId, spellTogglePacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players detailing a debug message.
        /// </summary>
        /// <param name="htmlDebugMessage">Debug message to send.</param>
        public void NotifyS2C_SystemMessage(string htmlDebugMessage)
        {
            var dm = new S2C_SystemMessage
            {
                SourceNetID = 0,
                //TODO: Ivestigate the cases where sender NetID is used
                SenderNetID = 0,
                Message = htmlDebugMessage
            };
            _packetHandlerManager.BroadcastPacket(dm.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified user detailing a debug message.
        /// </summary>
        /// <param name="userId">ID of the user to send the packet to.</param>
        /// <param name="message">Debug message to send.</param>
        public void NotifyS2C_SystemMessage(int userId, string message)
        {
            var dm = new S2C_SystemMessage
            {
                SourceNetID = 0,
                //TODO: Ivestigate the cases where sender NetID is used
                SenderNetID = 0,
                Message = message
            };
            _packetHandlerManager.SendPacket(userId, dm.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified team detailing a debug message.
        /// </summary>
        /// <param name="team">TeamId to send the packet to; BLUE/PURPLE/NEUTRAL.</param>
        /// <param name="message">Debug message to send.</param>
        public void NotifyS2C_SystemMessage(TeamId team, string message)
        {
            var dm = new S2C_SystemMessage
            {
                SourceNetID = 0,
                //TODO: Ivestigate the cases where sender NetID is used
                SenderNetID = 0,
                Message = message
            };
            _packetHandlerManager.BroadcastPacketTeam(team, dm.GetBytes(), Channel.CHL_S2C);
        }

        public void NotifyS2C_UnitSetMinimapIcon(IAttackableUnit unit, string iconCategory = "", bool changeIcon = false, string borderCategory = "", bool changeBorder = false, string borderScriptName = "")
        {
            var packet = new S2C_UnitSetMinimapIcon
            {
                SenderNetID = 0,
                UnitNetID = unit.NetId,
                IconCategory = iconCategory,
                ChangeIcon = changeIcon,
                BorderCategory = borderCategory,
                ChangeBorder = changeBorder,
                BorderScriptName = borderScriptName
            };
            _packetHandlerManager.BroadcastPacket(packet.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players detailing that the server has ticked within the specified time delta.
        /// Unused.
        /// </summary>
        /// <param name="delta">Time it took to tick.</param>
        public void NotifyServerTick(float delta)
        {
            var tickPacket = new ServerTick
            {
                Delta = delta
            };

            _packetHandlerManager.BroadcastPacket(tickPacket.GetBytes(), Channel.CHL_GAMEPLAY);
        }

        /// <summary>
        /// Sends a packet to all players on the specified team detailing whether the team has become able to surrender.
        /// </summary>
        /// <param name="can">Whether or not the team should be able to surrender.</param>
        /// <param name="team">Team to send the packet to.</param>
        public void NotifySetCanSurrender(bool can, TeamId team)
        {
            var canSurrender = new S2C_SetCanSurrender()
            {
                CanSurrender = can
            };
            _packetHandlerManager.BroadcastPacketTeam(team, canSurrender.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified player detailing that their range of movement has changed. This packet forces the game client to only send movement request packets when the distance from the specified center is less than the specified radius.
        /// </summary>
        /// <param name="ai">ObjAiBase that the restriction is being applied to.</param>
        /// <param name="center">Center of the restriction circle.</param>
        /// <param name="radius">Radius of the restriction circle; minimum distance from center required to move.</param>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="restrictCam">Whether or not the player's camera is also restricted to the same area.</param>
        public void NotifySetCircularMovementRestriction(IObjAiBase ai, Vector2 center, float radius, int userId, bool restrictCam = false)
        {
            var restrictPacket = new S2C_SetCircularMovementRestriction()
            {
                SenderNetID = ai.NetId,
                Center = new Vector3(center.X, ai.GetHeight(), center.Y),
                Radius = radius,
                RestrictCamera = restrictCam
            };
            _packetHandlerManager.SendPacket(userId, restrictPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified player detailing that the specified Debug Object has become hidden.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="sender">NetId that is sending this packet.</param>
        /// <param name="objID">ID of the Debug Object that will be hidden.</param>
        /// <param name="bitfield">Unknown variable.</param>
        public void NotifySetDebugHidden(int userId, uint sender, int objID, byte bitfield = 0x0)
        {
            var debugObjPacket = new S2C_SetDebugHidden
            {
                SenderNetID = sender,
                ObjectID = objID,
                Bitfield = bitfield // Not sure what this does
            };
            _packetHandlerManager.SendPacket(userId, debugObjPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players detailing that the specified unit's team has been set.
        /// </summary>
        /// <param name="unit">AttackableUnit who's team has been set.</param>
        public void NotifySetTeam(IAttackableUnit unit)
        {
            var p = new S2C_UnitChangeTeam
            {
                SenderNetID = unit.NetId,
                UnitNetID = unit.NetId,
                TeamID = (uint)unit.Team // TODO: Verify if TeamID is actually supposed to be a uint
            };
            _packetHandlerManager.BroadcastPacket(p.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Calls for the appropriate spawn packet to be sent given the specified GameObject's type and calls for a vision packet to be sent for the specified GameObject.
        /// </summary>
        /// <param name="obj">GameObject that has spawned.</param>
        /// <param name="team">The team the user belongs to.</param>
        /// <param name="userId">UserId to send the packet to.</param>
        /// <param name="gameTime">Time elapsed since the start of the game</param>
        /// <param name="doVision">Whether or not to package the packets into a vision packet.</param>
        public void NotifySpawn(IGameObject obj, TeamId team, int userId, float gameTime, bool doVision = true)
        {
            var spawnPacket = ConstructSpawnPacket(obj, gameTime);
            if (spawnPacket != null)
            {
                if (doVision)
                {
                    NotifyEnterTeamVision(obj, team, userId, spawnPacket);
                }
                else
                {
                    _packetHandlerManager.SendPacket(userId, spawnPacket.GetBytes(), Channel.CHL_S2C);
                }
            }
        }

        /// <summary>
        /// Sends a packet to the specified player detailing that the spawning (of champions & buildings) that occurs at the start of the game has ended.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        public void NotifySpawnEnd(int userId)
        {
            var endSpawnPacket = new S2C_EndSpawn();
            _packetHandlerManager.SendPacket(userId, endSpawnPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to either all players or the specified player detailing that the specified GameObject of type LevelProp has spawned.
        /// </summary>
        /// <param name="levelProp">LevelProp that has spawned.</param>
        /// <param name="userId">User to send the packet to.</param>
        public void NotifySpawnLevelPropS2C(ILevelProp levelProp, int userId = 0)
        {
            var spawnPacket = ConstructSpawnLevelPropPacket(levelProp);

            if (userId != 0)
            {
                _packetHandlerManager.SendPacket(userId, spawnPacket.GetBytes(), Channel.CHL_S2C);
            }
            else
            {
                _packetHandlerManager.BroadcastPacket(spawnPacket.GetBytes(), Channel.CHL_S2C);
            }
        }

        /// <summary>
        /// Sends a packet to all players with vision of the specified Champion detailing that the Champion's items have been swapped.
        /// </summary>
        /// <param name="c">Champion who swapped their items.</param>
        /// <param name="fromSlot">Slot the item was previously in.</param>
        /// <param name="toSlot">Slot the item was swapped to.</param>
        public void NotifySwapItemAns(IChampion c, byte fromSlot, byte toSlot)
        {
            //TODO: reorganize in alphabetic order
            var swapItem = new SwapItemAns
            {
                SenderNetID = c.NetId,
                Source = fromSlot,
                Destination = toSlot
            };
            _packetHandlerManager.BroadcastPacketVision(c, swapItem.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified player detailing the amount of time since the game started (in seconds). Used to initialize the user's in-game timer.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="time">Time since the game started (in milliseconds).</param>
        public void NotifySyncMissionStartTimeS2C(int userId, float time)
        {
            var sync = new SyncMissionStartTimeS2C()
            {
                StartTime = time / 1000.0f
            };

            _packetHandlerManager.SendPacket(userId, sync.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players detailing the amount of time since the game started (in seconds).
        /// </summary>
        /// <param name="gameTime">Time since the game started (in milliseconds).</param>
        public void NotifySynchSimTimeS2C(float gameTime)
        {
            var sync = new SynchSimTimeS2C()
            {
                SynchTime = gameTime / 1000.0f
            };

            _packetHandlerManager.BroadcastPacket(sync.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified player detailing the amount of time since the game started (in seconds).
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="time">Time since the game started (in milliseconds).</param>
        public void NotifySynchSimTimeS2C(int userId, float time)
        {
            var sync = new SynchSimTimeS2C()
            {
                SynchTime = time / 1000.0f
            };

            _packetHandlerManager.SendPacket(userId, sync.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified player detailing the results of server's the version and game info check for the specified player.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="players">List of ClientInfo of all players set to connect to the game.</param>
        /// <param name="version">Version of the player being checked.</param>
        /// <param name="gameMode">String of the internal name of the gamemode being played.</param>
        /// <param name="mapId">ID of the map being played.</param>
        public void NotifySynchVersion(int userId, List<Tuple<uint, ClientInfo>> players, string version, string gameMode, int mapId)
        {
            var syncVersion = new SynchVersionS2C
            {
                // TODO: Unhardcode all booleans below
                VersionMatches = true,
                // Logs match to file.
                WriteToClientFile = false,
                // Whether or not this game is considered a match.
                MatchedGame = true,
                // Unknown
                DradisInit = false,

                MapToLoad = mapId,
                VersionString = version,
                MapMode = gameMode,
                // TODO: Unhardcode all below
                PlatformID = "NA1",
                MutatorsNum = 0,
                OrderRankedTeamName = "",
                OrderRankedTeamTag = "",
                ChaosRankedTeamName = "",
                ChaosRankedTeamTag = "",
                // site.com
                MetricsServerWebAddress = "",
                // /messages
                MetricsServerWebPath = "",
                // 80
                MetricsServerPort = 0,
                // site.com
                DradisProdAddress = "",
                // /messages
                DradisProdResource = "",
                // 80
                DradisProdPort = 0,
                // test-lb-#.us-west-#.elb.someaws.com
                DradisTestAddress = "",
                // /messages
                DradisTestResource = "",
                // 80
                DradisTestPort = 0,
                // TODO: Create a new TipConfig class and use it here (basically, unhardcode this).
                TipConfig = new TipConfig
                {
                    TipID = 0,
                    ColorID = 0,
                    DurationID = 0,
                    Flags = 3
                },
                // Turret Range Indicators and others (taken from Map11 replay)
                GameFeatures = 662166610
            };

            for (int i = 0; i < players.Count; i++)
            {
                syncVersion.PlayerInfo[i] = new PlayerLoadInfo
                {
                    PlayerID = players[i].Item2.PlayerId,
                    // TODO: Change to players[i].Item2.SummonerLevel
                    SummonorLevel = 30,
                    SummonorSpell1 = HashString(players[i].Item2.SummonerSkills[0]),
                    SummonorSpell2 = HashString(players[i].Item2.SummonerSkills[1]),
                    // TODO
                    Bitfield = 0,
                    TeamId = (uint)players[i].Item2.Team,
                    // TODO: Change to players[i].Item2.Champion.Model + "Bot" (if players[i].Item2.IsBot)
                    BotName = "",
                    // TODO: Change to players[i].Item2.Champion.Model (if players[i].Item2.IsBot)
                    BotSkinName = "",
                    EloRanking = players[i].Item2.Rank,
                    // TODO: Change to players[i].Item2.SkinNo (if players[i].Item2.IsBot)
                    BotSkinID = 0,
                    // TODO: Change to players[i].Item2.BotDifficulty (if players[i].Item2.IsBot)
                    BotDifficulty = 0,
                    ProfileIconId = players[i].Item2.Icon,
                    // TODO: Unhardcode these two.
                    AllyBadgeID = 0,
                    EnemyBadgeID = 0
                };
            }

            // TODO: syncVersion.Mutators

            // TODO: syncVersion.DisabledItems

            // TODO: syncVersion.EnabledDradisMessages

            _packetHandlerManager.SendPacket(userId, syncVersion.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified player detailing the status (results) of a surrender vote that was called for and ended.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="team">TeamId that called for the surrender vote; BLUE/PURPLE/NEUTRAL.</param>
        /// <param name="reason">SurrenderReason of why the vote ended.</param>
        /// <param name="yesVotes">Number of votes for the surrender.</param>
        /// <param name="noVotes">Number of votes against the surrender.</param>
        public void NotifyTeamSurrenderStatus(int userId, TeamId team, SurrenderReason reason, byte yesVotes, byte noVotes)
        {
            var surrenderStatus = new S2C_TeamSurrenderStatus()
            {
                SurrenderReason = (uint)reason,
                ForVote = yesVotes,
                AgainstVote = noVotes,
                TeamID = (uint)team,
            };
            _packetHandlerManager.SendPacket(userId, surrenderStatus.GetBytes(), Channel.CHL_S2C);
        }

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
        public void NotifyTeamSurrenderVote(IChampion starter, bool open, bool votedYes, byte yesVotes, byte noVotes, byte maxVotes, float timeOut)
        {
            var surrender = new S2C_TeamSurrenderVote()
            {
                PlayerNetID = starter.NetId,
                OpenVoteMenu = open,
                VoteYes = votedYes,
                ForVote = yesVotes,
                AgainstVote = noVotes,
                NumPlayers = maxVotes,
                TeamID = (uint)starter.Team,
                TimeOut = timeOut,
            };
            _packetHandlerManager.BroadcastPacketTeam(starter.Team, surrender.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players with vision of the given unit detailing that it has teleported to the given position.
        /// </summary>
        /// <param name="unit">Unit that has teleported.</param>
        /// <param name="position">Position the unit teleported to.</param>
        public void NotifyTeleport(IAttackableUnit unit, Vector2 position)
        {
            var md = new MovementDataNormal()
            {
                SyncID = unit.SyncId,
                TeleportNetID = unit.NetId,
                HasTeleportID = true,
                TeleportID = unit.TeleportID,
                Waypoints = new List<CompressedWaypoint> { PacketExtensions.Vector2ToWaypoint(PacketExtensions.TranslateToCenteredCoordinates(position, _navGrid)) },
            };

            var wpGroup = new WaypointGroup()
            {
                SyncID = unit.SyncId,
                Movements = new List<MovementDataNormal> { md }
            };

            _packetHandlerManager.BroadcastPacketVision(unit, wpGroup.GetBytes(), Channel.CHL_LOW_PRIORITY);
        }

        /// <summary>
        /// Sends a packet to all players detailing that their screen's tint is shifting to the specified color.
        /// </summary>
        /// <param name="team">TeamID to apply the tint to.</param>
        /// <param name="enable">Whether or not to fade in the tint.</param>
        /// <param name="speed">Amount of time that should pass before tint is fully applied.</param>
        /// <param name="color">Color of the tint.</param>
        public void NotifyTint(TeamId team, bool enable, float speed, GameServerCore.Content.Color color)
        {
            var c = new LeaguePackets.Game.Common.Color
            {
                Blue = color.B,
                Green = color.G,
                Red = color.R,
                Alpha = color.A
            };
            var tint = new S2C_ColorRemapFX
            {
                IsFadingIn = enable,
                FadeTime = speed,
                TeamID = (uint)team,
                Color = c,
                MaxWeight = (c.Alpha / 255.0f) // TODO: Implement this correctly, current implementation taken from old LS packet
            };
            _packetHandlerManager.BroadcastPacket(tint.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players that the specified Champion has gained the specified amount of experience.
        /// </summary>
        /// <param name="champion">Champion that gained the experience.</param>
        /// <param name="experience">Amount of experience gained.</param>
        /// TODO: Verify if sending to all players is correct.
        public void NotifyUnitAddEXP(IChampion champion, float experience)
        {
            var xp = new UnitAddEXP
            {
                // TODO: Verify if this is correct. Usually 0.
                SenderNetID = champion.NetId,
                TargetNetID = champion.NetId,
                ExpAmmount = experience
            };
            // TODO: Verify if we should change to BroadcastPacketVision
            _packetHandlerManager.BroadcastPacket(xp.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players that the specified Champion has killed a specified player and received a specified amount of gold.
        /// </summary>
        /// <param name="c">Champion that killed a unit.</param>
        /// <param name="died">AttackableUnit that died to the Champion.</param>
        /// <param name="gold">Amount of gold the Champion gained for the kill.</param>
        /// TODO: Only use BroadcastPacket when the unit that died is a Champion.
        public void NotifyUnitAddGold(IChampion c, IAttackableUnit died, float gold)
        {
            // TODO: Verify if this handles self-gold properly.
            var ag = new UnitAddGold
            {
                SenderNetID = died.NetId,
                TargetNetID = c.NetId,
                SourceNetID = died.NetId,
                GoldAmmount = gold
            };
            _packetHandlerManager.SendPacket((int)c.GetPlayerId(), ag.GetBytes(), Channel.CHL_S2C);
        }

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
        public void NotifyUnitApplyDamage(IAttackableUnit source, IAttackableUnit target, float amount, DamageType type, DamageResultType damagetext, bool isGlobal = true, int sourceId = 0, int targetId = 0)
        {
            var damagePacket = new UnitApplyDamage
            {
                SenderNetID = source.NetId,
                DamageResultType = (byte)damagetext,
                DamageType = (byte)type,
                TargetNetID = target.NetId,
                SourceNetID = source.NetId,
                Damage = amount
            };

            if (isGlobal)
            {
                _packetHandlerManager.BroadcastPacket(damagePacket.GetBytes(), Channel.CHL_S2C);
            }
            else
            {
                if (sourceId > 0)
                {
                    _packetHandlerManager.SendPacket(sourceId, damagePacket.GetBytes(), Channel.CHL_S2C);
                }
                if (targetId > 0)
                {
                    _packetHandlerManager.SendPacket(targetId, damagePacket.GetBytes(), Channel.CHL_S2C);
                }
            }
        }

        /// <summary>
        /// Sends a packet to the specified player detailing that the specified target GameObject's (debug?) path drawing mode has been set to the specified mode.
        /// </summary>
        /// <param name="userId">User to send the packet to(?).</param>
        /// <param name="unit">Unit that has called for the packet.</param>
        /// <param name="target">GameObject who's (debug?) draw path mode is being set.</param>
        /// <param name="mode">Draw path mode to set. Refer to DrawPathMode enum.</param>
        /// TODO: Verify the functionality of this packet (and its parameters) and create an enum for the mode.
        public void NotifyUnitSetDrawPathMode(int userId, IAttackableUnit unit, IGameObject target, DrawPathMode mode)
        {
            var drawPacket = new S2C_UnitSetDrawPathMode
            {
                SenderNetID = unit.NetId,
                TargetNetID = target.NetId,
                DrawPathMode = (byte)mode,
                UpdateRate = 0.1f
            };
            _packetHandlerManager.SendPacket(userId, drawPacket.GetBytes(), Channel.CHL_S2C);
        }

        public void NotifyUpdateLevelPropS2C(UpdateLevelPropData propData)
        {
            var packet = new UpdateLevelPropS2C
            {
                SenderNetID = 0,
                UpdateLevelPropData = propData
            };
            _packetHandlerManager.BroadcastPacket(packet.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the player attempting to use an item that the item was used successfully.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="ai">GameObject of type ObjAiBase that can buy items.</param>
        /// <param name="itemInstance">Item instance housing all information about the item that has been used.</param>
        public void NotifyUseItemAns(int userId, IObjAiBase ai, IItem itemInstance)
        {
            var useItemPacket = new UseItemAns
            {
                SenderNetID = ai.NetId,
                Slot = ai.Inventory.GetItemSlot(itemInstance),
                SpellCharges = (byte)itemInstance.StackCount
            };

            _packetHandlerManager.BroadcastPacketVision(ai, useItemPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified team detailing that an object's visibility has changed.
        /// General function which will send the needed vision packet for the specific object type.
        /// </summary>
        /// <param name="obj">GameObject which had their visibility changed.</param>
        /// <param name="team">Team which is affected by this visibility change.</param>
        /// <param name="becameVisible">Whether or not the change was an entry into vision.</param>
        /// <param name="userId">UserId to send the packet to. If not specified or zero, the packet is broadcasted to the team.</param>
        public void NotifyVisibilityChange(IGameObject obj, TeamId team, bool becameVisible = true, int userId = 0)
        {
            if (becameVisible)
            {
                NotifyEnterTeamVision(obj, team, userId);
            }
            else
            {
                NotifyLeaveTeamVision(obj, team, userId);
            }
        }

        /// <summary>
        /// Sends a notification that the object has entered the team's scope and fully synchronizes its state.
        /// </summary>
        void NotifyEnterTeamVision(IGameObject obj, TeamId team, int userId = 0, GamePacket spawnPacket = null)
        {
            if (obj is IAttackableUnit u)
            {
                var visibilityPacket = spawnPacket as OnEnterVisibilityClient;
                if (visibilityPacket == null)
                {
                    List<GamePacket> packets = null;
                    if (spawnPacket != null)
                    {
                        packets = new List<GamePacket>(1) { spawnPacket };
                    }
                    visibilityPacket = ConstructEnterVisibilityClientPacket(obj, obj is IChampion, packets);
                }
                var healthbarPacket = ConstructEnterLocalVisibilityClientPacket(obj);
                //TODO: try to include it to packets too?
                var us = new OnReplication()
                {
                    SyncID = (uint)Environment.TickCount,
                    ReplicationData = new List<ReplicationData>(1){
                        u.Replication.GetData(false)
                    }
                };

                if (userId == 0)
                {
                    _packetHandlerManager.BroadcastPacketTeam(team, visibilityPacket.GetBytes(), Channel.CHL_S2C);
                    _packetHandlerManager.BroadcastPacketTeam(team, healthbarPacket.GetBytes(), Channel.CHL_S2C);
                    _packetHandlerManager.BroadcastPacketTeam(team, us.GetBytes(), Channel.CHL_S2C);
                }
                else
                {
                    _packetHandlerManager.SendPacket(userId, visibilityPacket.GetBytes(), Channel.CHL_S2C);
                    _packetHandlerManager.SendPacket(userId, healthbarPacket.GetBytes(), Channel.CHL_S2C);
                    _packetHandlerManager.SendPacket(userId, us.GetBytes(), Channel.CHL_S2C);
                }
            }
            else //if(obj is IRegion || obj is ISpellMissile || obj is ILevelProp || obj is IParticle)
            {
                var packet = spawnPacket;
                if (packet == null)
                {
                    if (obj is IParticle p)
                    {
                        packet = ConstructFXEnterTeamVisibilityPacket(p, team);
                    }
                    else
                    {
                        packet = ConstructOnEnterTeamVisibilityPacket(obj, team); // Generic visibility packet
                    }
                };
                if (userId == 0)
                {
                    _packetHandlerManager.BroadcastPacketTeam(team, packet.GetBytes(), Channel.CHL_S2C);
                }
                else
                {
                    _packetHandlerManager.SendPacket(userId, packet.GetBytes(), Channel.CHL_S2C);
                }
            }
        }

        void NotifyLeaveTeamVision(IGameObject obj, TeamId team, int userId = 0)
        {
            if (obj is IParticle p)
            {
                NotifyFXLeaveTeamVisibility(p, team, userId);
            }
            if (obj is IAttackableUnit)
            {
                NotifyLeaveVisibilityClient(obj, team, userId);
            }
            else
            {
                NotifyS2C_OnLeaveTeamVisibility(obj, team, userId);
            }
        }

        /// <summary>
        /// Sends a packet to all players that have vision of the specified unit that it has made a movement.
        /// </summary>
        /// <param name="u">AttackableUnit that is moving.</param>
        /// <param name="userId">UserId to send the packet to. If not specified or zero, the packet is broadcasted to all players that have vision of the specified unit.</param>
        /// <param name="useTeleportID">Whether or not to teleport the unit to its current position in its path.</param>
        public void NotifyWaypointGroup(IAttackableUnit u, int userId = 0, bool useTeleportID = false)
        {
            // TODO: Verify if casts correctly
            var move = (MovementDataNormal)PacketExtensions.CreateMovementData(u, _navGrid, MovementDataType.Normal, useTeleportID: useTeleportID);

            // TODO: Implement support for multiple movements.
            var packet = new WaypointGroup
            {
                SenderNetID = u.NetId,
                SyncID = u.SyncId,
                Movements = new List<MovementDataNormal>() { move }
            };

            if (userId == 0)
            {
                _packetHandlerManager.BroadcastPacketVision(u, packet.GetBytes(), Channel.CHL_LOW_PRIORITY);
            }
            else
            {
                _packetHandlerManager.SendPacket(userId, packet.GetBytes(), Channel.CHL_LOW_PRIORITY);
            }
        }

        /// <summary>
        /// Sends a packet to all players that have vision of the specified unit.
        /// The packet details a group of waypoints with speed parameters which determine what kind of movement will be done to reach the waypoints, or optionally a GameObject.
        /// Functionally referred to as a dash in-game.
        /// </summary>
        /// <param name="u">Unit that is dashing.</param>
        /// TODO: Implement ForceMovement class which houses these parameters, then have that as the only parameter to this function (and other Dash-based functions).
        public void NotifyWaypointGroupWithSpeed
        (
            IAttackableUnit u
        )
        {
            // TODO: Implement Dash class and house a List of these with waypoints.
            var speeds = new SpeedParams
            {
                PathSpeedOverride = u.MovementParameters.PathSpeedOverride,
                ParabolicGravity = u.MovementParameters.ParabolicGravity,
                // TODO: Implement as parameter (ex: Aatrox Q).
                ParabolicStartPoint = u.MovementParameters.ParabolicStartPoint,
                Facing = u.MovementParameters.KeepFacingDirection,
                FollowNetID = u.MovementParameters.FollowNetID,
                FollowDistance = u.MovementParameters.FollowDistance,
                FollowBackDistance = u.MovementParameters.FollowBackDistance,
                FollowTravelTime = u.MovementParameters.FollowTravelTime
            };

            // TODO: Verify if cast works.
            var md = (MovementDataWithSpeed)PacketExtensions.CreateMovementData(u, _navGrid, MovementDataType.WithSpeed, speeds);

            var speedWpGroup = new WaypointGroupWithSpeed
            {
                SenderNetID = 0,
                SyncID = u.SyncId,
                // TOOD: Implement support for multiple speed-based movements (functionally known as dashes).
                Movements = new List<MovementDataWithSpeed> { md }
            };

            _packetHandlerManager.BroadcastPacketVision(u, speedWpGroup.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players with vision of the given unit detailing its waypoints.
        /// </summary>
        /// <param name="unit">Unit to send.</param>
        public void NotifyWaypointList(IAttackableUnit unit)
        {
            var wpList = new WaypointList
            {
                SenderNetID = unit.NetId,
                SyncID = unit.SyncId,
                Waypoints = unit.Waypoints
            };

            _packetHandlerManager.BroadcastPacketVision(unit, wpList.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players with vision of the given GameObject detailing its waypoints.
        /// </summary>
        /// <param name="obj">GameObject to send.</param>
        public void NotifyWaypointList(IGameObject obj, List<Vector2> waypoints)
        {
            var wpList = new WaypointList
            {
                SenderNetID = obj.NetId,
                SyncID = obj.SyncId,
                Waypoints = waypoints
            };

            _packetHandlerManager.BroadcastPacketVision(obj, wpList.GetBytes(), Channel.CHL_S2C);
        }

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
        public void NotifyWaypointListWithSpeed
        (
            IAttackableUnit u,
            float dashSpeed,
            float leapGravity = 0,
            bool keepFacingLastDirection = false,
            IGameObject target = null,
            float followTargetMaxDistance = 0,
            float backDistance = 0,
            float travelTime = 0
        )
        {
            // TODO: Implement ForceMovement class/interface and house a List of these with waypoints.
            var speeds = new SpeedParams
            {
                PathSpeedOverride = dashSpeed,
                ParabolicGravity = leapGravity,
                // TODO: Implement as parameter (ex: Aatrox Q).
                ParabolicStartPoint = u.Position,
                Facing = keepFacingLastDirection,
                FollowNetID = 0,
                FollowDistance = followTargetMaxDistance,
                FollowBackDistance = backDistance,
                FollowTravelTime = travelTime
            };

            if (target != null)
            {
                speeds.FollowNetID = target.NetId;
            }

            var speedWpGroup = new WaypointListHeroWithSpeed
            {
                SenderNetID = u.NetId,
                SyncID = u.SyncId,
                // TOOD: Implement support for multiple speed-based movements (functionally known as dashes).
                WaypointSpeedParams = speeds,
                Waypoints = u.Waypoints
            };

            _packetHandlerManager.BroadcastPacketVision(u, speedWpGroup.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified player detailing that their request to view something with their camera has been acknowledged.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="request">ViewRequest housing information about the camera's view.</param>
        /// TODO: Verify if this is the correct implementation.
        public void NotifyWorld_SendCamera_Server_Acknologment(ClientInfo client, ViewRequest request)
        {
            var answer = new World_SendCamera_Server_Acknologment
            {
                //TODO: Check these values
                SenderNetID = client.Champion.NetId,
                SyncID = request.SyncID,
            };
            _packetHandlerManager.SendPacket((int)client.PlayerId, answer.GetBytes(), Channel.CHL_S2C, PacketFlags.NONE);
        }
    }
}
