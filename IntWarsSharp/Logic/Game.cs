using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet.Native;
using System.Runtime.InteropServices;

namespace IntWarsSharp.Core.Logic
{
    unsafe class Game
    {
        ENetHost* _server;
        BlowFish _blowfish;
        bool _isAlive = false;

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

            LuaScript script(false);

            script.loadScript("../../lua/config.lua");

            //  sol::state lua;
            //  lua.open_libraries(sol::lib::base, sol::lib::table);

            //  lua.open_file("../../lua/config.lua");
            sol::table playerList = script.getTable("players");
            for (int i = 1; i < 12; i++)
            {
                try
                {
                    std::string playerIndex = "player" + toString(i);

                    sol::table playerData = playerList.get<sol::table>(playerIndex);

                    std::string rank = playerData.get < std::string> ("rank");
                    std::string name = playerData.get < std::string> ("name");
                    std::string champion = playerData.get < std::string> ("champion");
                    std::string team = playerData.get < std::string> ("team");
                    int skin = playerData.get<int>("skin");
                    int ribbon = playerData.get<int>("ribbon");
                    int icon = playerData.get<int>("icon");
                    std::string summoner1 = playerData.get < std::string> ("summoner1");
                    std::string summoner2 = playerData.get < std::string> ("summoner2");

                    ClientInfo* player = new ClientInfo(rank, ((team == "BLUE") ? TEAM_BLUE : TEAM_PURPLE), ribbon, icon);

                    player->setName(name);


                    player->setSkinNo(skin);
                    static int id = 1;
                    player->userId = id; // same as StartClient.bat
                    id++;
                    player->setSummoners(strToId(summoner1), strToId(summoner2));
                    Champion* c = ChampionFactory::getChampionFromType(champion, map, GetNewNetID(), player->userId);
                    float respawnX, respawnY;
                    std::tie(respawnX, respawnY) = c->getRespawnPosition();
                    c->setPosition(respawnX, respawnY);
                    c->setTeam((team == "BLUE") ? 0 : 1);
                    c->levelUp();


                    player->setChampion(c);




                    players.push_back(player);

                }
                catch (sol::error e)
                {
                    //CORE_ERROR("Error loading champion: %s", e.what());
                    break;
                }
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
    }
}
