using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet.Native;
using System.Runtime.InteropServices;
using IntWarsSharp.Logic;
using IntWarsSharp.Logic.Enet;
using IntWarsSharp.Core.Logic.PacketHandlers;
using IntWarsSharp.Logic.GameObjects;
using ENet;
using IntWarsSharp.Logic.Packets;

namespace IntWarsSharp.Core.Logic
{
    unsafe class Game
    {
        ENetHost* _server;
        BlowFish _blowfish;
        bool _isAlive = false;
        private int dwStart = 0x40000001;

        bool _started = false;
        int playersReady = 0;

        ENetPeer* currentPeer;
        List<ClientInfo> players = new List<ClientInfo>();
        Map map;
        private const int PEER_MTU = 996;
        private const PacketFlags RELIABLE = PacketFlags.Reliable;
        private const PacketFlags UNRELIABLE = PacketFlags.None;
        private const string GAME_VERSION = "Version 4.20.0.315 [PUBLIC]";
        private const int REFRESH_RATE = 16666; // 60 fps

        public bool initialize(ENetAddress address, string baseKey)
        {
            /* if (ENetApi.enet_initialize() != 0)
                 return false;*/

            _server = ENetApi.enet_host_create(ref address, new IntPtr(0), new IntPtr(32), 0, 0);
            if (_server == null)
                return false;

            string key = Encoding.UTF8.GetString(Convert.FromBase64String(baseKey));

            if (key.Length <= 0)
                return false;

            _blowfish = new BlowFish(key);
            PacketHandlerManager.getInstace().InitHandlers();

            map = new SummonersRift(this);

            //TODO: better lua implementation

            var id = 0;
            foreach (var p in Config.players)
            {
                var player = new ClientInfo(p.Value.rank, ((p.Value.team.ToLower() == "blue") ? TeamId.TEAM_BLUE : TeamId.TEAM_PURPLE), p.Value.ribbon, p.Value.icon);

                player.setName(p.Value.name);

                player.setSkinNo(p.Value.skin);
                player.userId = id; // same as StartClient.bat
                id++;

                player.setSummoners(strToId(p.Value.summoner1), strToId(p.Value.summoner2));

                Champion c = ChampionFactory.getChampionFromType(p.Value.champion, map, GetNewNetID(), player.userId);
                var pos = c.getRespawnPosition();

                c.setPosition(pos.Item1, pos.Item2);
                c.setTeam((p.Value.team.ToLower() == "blue") ? 0 : 1);
                c.levelUp();

                player.setChampion(c);

                players.Add(player);
            }

            // Uncomment the following to get 2-players
            /*ClientInfo* player2 = new ClientInfo("GOLD", TEAM_PURPLE);
            player2->setName("tseT");
            Champion* c2 = ChampionFactory::getChampionFromType("Ezreal", map, GetNewNetID());
            c2->setPosition(100.f, 273.55f);
            c2->setTeam(1);
            map->addObject(c2);
            player2->setChampion(c2);
            player2->setSkinNo(4);
            player2->userId = 2; // same as StartClient.bat
            player2->setSummoners(SPL_Ignite, SPL_Flash);

            players.push_back(player2);*/

            return _isAlive = true;
        }
        public void netLoop()
        {
            var enetEvent = new ENetEvent();
            var watch = System.Diagnostics.Stopwatch.StartNew();

            while (true)
            {
                while (ENetApi.enet_host_service(_server, &enetEvent, 0) > 0)
                {
                    switch (enetEvent.type)
                    {
                        case EventType.Connect:
                            //Logging->writeLine("A new client connected: %i.%i.%i.%i:%i", event.peer->address.host & 0xFF, (event.peer->address.host >> 8) & 0xFF, (event.peer->address.host >> 16) & 0xFF, (event.peer->address.host >> 24) & 0xFF, event.peer->address.port);

                            /* Set some defaults */
                            //enetEvent.peer->mtu = PEER_MTU;
                            enetEvent.data = 0;
                            break;

                        case EventType.Receive:
                            currentPeer = enetEvent.peer;
                            if (!handlePacket(enetEvent.peer, enetEvent.packet, enetEvent.channelID))
                            {
                                //enet_peer_disconnect(event.peer, 0);
                            }

                            /* Clean up the packet now that we're done using it. */
                            ENetApi.enet_packet_destroy(enetEvent.packet);
                            break;

                        case EventType.Disconnect:
                            handleDisconnect(enetEvent.peer);
                            break;
                    }
                }

                watch.Stop();
                var elapsed = watch.ElapsedMicroSeconds();
                watch = System.Diagnostics.Stopwatch.StartNew();

                if (_started)
                    map.update(elapsed);

                watch.Stop();
                elapsed = watch.ElapsedMicroSeconds();
                watch = System.Diagnostics.Stopwatch.StartNew();
                if (elapsed < REFRESH_RATE)
                    System.Threading.Thread.Sleep(TimeSpan.FromMilliseconds((REFRESH_RATE - elapsed) * 1000)); //this is so not going to work
            }
        }


        bool handlePacket(ENetPeer* peer, ENetPacket* packet, byte channelID)
        {
            if ((int)packet->dataLength >= 8)
            {
                //if (peerInfo(peer))
                {
                    byte[] data = new byte[(int)packet->dataLength - ((int)packet->dataLength % 8)];
                    Marshal.Copy(packet->data, data, 0, data.Length); //not gonna work lmao
                    _blowfish.Decrypt(data, BlowFishMode.ECB); //Encrypt everything minus the last bytes that overflow the 8 byte boundary
                }
            }

            PacketHeader header = Marshal.PtrToStructure<PacketHeader>(packet->data); //pls work
            var handler = PacketHandlerManager.getInstace().GetHandler(header.cmd, channelID);

            if (handler != null)
            {
                return handler.HandlePacket(peer, packet);
            }
            else
            {
                Logger.LogCoreWarning("Unhandled OpCode " + header.cmd);
                printPacket(packet->data, packet->dataLength);
            }
            return false;
        }
        bool handleDisconnect(ENetPeer* peer);


        // Notifiers
        void notifyMinionSpawned(Minion* m, int team);
        void notifySetHealth(Unit* u);
        void notifyUpdatedStats(Unit* u, bool partial = true);
        void notifyMovement(Object* o);
        void notifyDamageDone(Unit* source, Unit* target, float amount, DamageType type = DAMAGE_TYPE_PHYSICAL);
        void notifyBeginAutoAttack(Unit* attacker, Unit* victim, uint32 futureProjNetId, bool isCritical);
        void notifyTeleport(Unit* u, float _x, float _y);
        void notifyProjectileSpawn(Projectile* p);
        void notifyProjectileDestroy(Projectile* p);
        void notifyParticleSpawn(Champion* source, Target* target, const std::string& particleName);
void notifyModelUpdate(Unit* object);
        void notifyLevelUp(Champion* c);
        void notifyItemBought(Champion* c, const ItemInstance* i);
        void notifyItemsSwapped(Champion* c, uint8 fromSlot, uint8 toSlot);
        void notifyRemoveItem(Champion* c, uint8 slot, uint8 remaining);
        void notifySetTarget(Unit* attacker, Unit* target);
        void notifyChampionDie(Champion* die, Unit* killer, uint32 goldFromKill);
        void notifyChampionDeathTimer(Champion* die);
        void notifyChampionRespawn(Champion* c);
        void notifyShowProjectile(Projectile* p);
        void notifyNpcDie(Unit* die, Unit* killer);
        void notifyNextAutoAttack(Unit* attacker, Unit* target, uint32 futureProjNetId, bool isCritical, bool nextAttackFlag);
        void notifyOnAttack(Unit* attacker, Unit* attacked, AttackType attackType);
        void notifyAddBuff(Unit* u, Unit* source, std::string buffName);
        void notifyRemoveBuff(Unit* u, std::string buffName);
        void notifyAddGold(Champion* c, Unit* died, float gold);
        void notifyStopAutoAttack(Unit* attacker);
        void notifyDebugMessage(std::string htmlDebugMessage);
        void notifySpawn(Unit* u);
        void notifyLeaveVision(Object* o, uint32 team);
        void notifyEnterVision(Object* o, uint32 team);
        void notifyChampionSpawned(Champion* c, uint32 team);
        void notifySetCooldown(Champion* c, uint8 slotId, float currentCd, float totalCd = 0.0f);
        void notifyGameTimer();
        void notifyAnnounceEvent(uint8 messageId, bool isMapSpecific);
        void notifySpellAnimation(Unit* u, const std::string& animation);
void notifySetAnimation(Unit* u, const std::vector<std::pair<std::string, std::string>>& animationPairs);
void notifyDash(Unit* u, float _x, float _y, float dashSpeed);

        // Tools
        static void printPacket(const uint8* buf, uint32 len);
void printLine(uint8* buf, uint32 len);
        protected:
		bool sendPacket(ENetPeer* peer, const uint8* data, uint32 length, uint8 channelNo, uint32 flag = RELIABLE);
bool sendPacket(ENetPeer* peer, const Packet& packet, uint8 channelNo, uint32 flag = RELIABLE);
bool broadcastPacket(uint8* data, uint32 length, uint8 channelNo, uint32 flag = RELIABLE);
        bool broadcastPacket(const Packet& packet, uint8 channelNo, uint32 flag = RELIABLE);
bool broadcastPacketTeam(uint8 team, const uint8* data, uint32 length, uint8 channelNo, uint32 flag = RELIABLE);
bool broadcastPacketTeam(uint8 team, const Packet& packet, uint8 channelNo, uint32 flag = RELIABLE);
bool broadcastPacketVision(Object* o, const Packet& packet, uint8 channelNo, uint32 flag = RELIABLE);
bool broadcastPacketVision(Object* o, const uint8* data, uint32 length, uint8 channelNo, uint32 flag = RELIABLE);



        SpellIds strToId(string str)
        {
            if (str == "FLASH")
            {
                return SpellIds.SPL_Flash;
            }
            else if (str == "IGNITE")
            {
                return SpellIds.SPL_Ignite;
            }
            else if (str == "HEAL")
            {
                return SpellIds.SPL_Heal;
            }
            else if (str == "BARRIER")
            {
                return SpellIds.SPL_Barrier;
            }
            else if (str == "SMITE")
            {
                return SpellIds.SPL_Smite;
            }
            else if (str == "GHOST")
            {
                return SpellIds.SPL_Ghost;
            }
            else if (str == "REVIVE")
            {
                return SpellIds.SPL_Revive;
            }
            else if (str == "CLEANSE")
            {
                return SpellIds.SPL_Cleanse;
            }
            else if (str == "TELEPORT")
            {
                return SpellIds.SPL_Teleport;
            }

            return 0;
        }
        private int GetNewNetID()
        {
            dwStart++;
            return dwStart;
        }
    }
}
