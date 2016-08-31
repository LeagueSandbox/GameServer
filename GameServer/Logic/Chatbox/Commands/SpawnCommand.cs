using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets;
using LeagueSandbox.GameServer.Logic.Players;
using System;
using System.Collections.Generic;
using System.Numerics;
using static LeagueSandbox.GameServer.Logic.Chatbox.ChatboxManager;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    class SpawnCommand : ChatCommand
    {
        public SpawnCommand(string command, string syntax, ChatboxManager owner) : base(command, syntax, owner) { }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            Game _game = Program.ResolveDependency<Game>();
            NetworkIdManager _networkIdManager = Program.ResolveDependency<NetworkIdManager>();
            PlayerManager _playerManager = Program.ResolveDependency<PlayerManager>();

            var split = arguments.ToLower().Split(' ');

            if (split.Length < 2)
            {
                _owner.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ShowSyntax();
            }
            else if (split[1] == "minions")
            {
                var champion = _playerManager.GetPeerInfo(peer).GetChampion();
                var random = new Random();

                Minion caster = new Minion(_networkIdManager.GetNewNetID(), MinionSpawnType.MINION_TYPE_CASTER, MinionSpawnPosition.SPAWN_RED_MID);
                Minion cannon = new Minion(_networkIdManager.GetNewNetID(), MinionSpawnType.MINION_TYPE_CANNON, MinionSpawnPosition.SPAWN_RED_MID);
                Minion melee = new Minion(_networkIdManager.GetNewNetID(), MinionSpawnType.MINION_TYPE_MELEE, MinionSpawnPosition.SPAWN_RED_MID);
                Minion super = new Minion(_networkIdManager.GetNewNetID(), MinionSpawnType.MINION_TYPE_SUPER, MinionSpawnPosition.SPAWN_RED_MID);

                int x = 400;
                caster.setPosition(champion.getX() + random.Next(-x, x), champion.getY() + random.Next(-x, x));
                cannon.setPosition(champion.getX() + random.Next(-x, x), champion.getY() + random.Next(-x, x));
                melee.setPosition(champion.getX() + random.Next(-x, x), champion.getY() + random.Next(-x, x));
                super.setPosition(champion.getX() + random.Next(-x, x), champion.getY() + random.Next(-x, x));

                caster.PauseAI(true);
                cannon.PauseAI(true);
                melee.PauseAI(true);
                super.PauseAI(true);

                caster.setWaypoints(new List<Vector2> { new Vector2(caster.getX(), caster.getY()), new Vector2(caster.getX(), caster.getY()) });
                cannon.setWaypoints(new List<Vector2> { new Vector2(cannon.getX(), cannon.getY()), new Vector2(cannon.getX(), cannon.getY()) });
                melee.setWaypoints(new List<Vector2> { new Vector2(melee.getX(), melee.getY()), new Vector2(melee.getX(), melee.getY()) });
                super.setWaypoints(new List<Vector2> { new Vector2(super.getX(), super.getY()), new Vector2(super.getX(), super.getY()) });

                caster.setVisibleByTeam(Enet.TeamId.TEAM_BLUE, true);
                cannon.setVisibleByTeam(Enet.TeamId.TEAM_BLUE, true);
                melee.setVisibleByTeam(Enet.TeamId.TEAM_BLUE, true);
                super.setVisibleByTeam(Enet.TeamId.TEAM_BLUE, true);

                _game.GetMap().AddObject(caster);
                _game.GetMap().AddObject(cannon);
                _game.GetMap().AddObject(melee);
                _game.GetMap().AddObject(super);
            }
        }
    }
}
