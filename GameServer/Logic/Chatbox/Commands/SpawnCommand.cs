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
                var champion = _playerManager.GetPeerInfo(peer).Champion;
                var random = new Random();

                Minion caster = new Minion(MinionSpawnType.MINION_TYPE_CASTER, MinionSpawnPosition.SPAWN_RED_MID);
                Minion cannon = new Minion(MinionSpawnType.MINION_TYPE_CANNON, MinionSpawnPosition.SPAWN_RED_MID);
                Minion melee = new Minion(MinionSpawnType.MINION_TYPE_MELEE, MinionSpawnPosition.SPAWN_RED_MID);
                Minion super = new Minion(MinionSpawnType.MINION_TYPE_SUPER, MinionSpawnPosition.SPAWN_RED_MID);

                int x = 400;
                caster.setPosition(champion.X + random.Next(-x, x), champion.Y + random.Next(-x, x));
                cannon.setPosition(champion.X + random.Next(-x, x), champion.Y + random.Next(-x, x));
                melee.setPosition(champion.X + random.Next(-x, x), champion.Y + random.Next(-x, x));
                super.setPosition(champion.X + random.Next(-x, x), champion.Y + random.Next(-x, x));

                caster.PauseAI(true);
                cannon.PauseAI(true);
                melee.PauseAI(true);
                super.PauseAI(true);

                caster.SetWaypoints(new List<Vector2> { new Vector2(caster.X, caster.Y), new Vector2(caster.X, caster.Y) });
                cannon.SetWaypoints(new List<Vector2> { new Vector2(cannon.X, cannon.Y), new Vector2(cannon.X, cannon.Y) });
                melee.SetWaypoints(new List<Vector2> { new Vector2(melee.X, melee.Y), new Vector2(melee.X, melee.Y) });
                super.SetWaypoints(new List<Vector2> { new Vector2(super.X, super.Y), new Vector2(super.X, super.Y) });

                caster.SetVisibleByTeam(Enet.TeamId.TEAM_BLUE, true);
                cannon.SetVisibleByTeam(Enet.TeamId.TEAM_BLUE, true);
                melee.SetVisibleByTeam(Enet.TeamId.TEAM_BLUE, true);
                super.SetVisibleByTeam(Enet.TeamId.TEAM_BLUE, true);

                _game.Map.AddObject(caster);
                _game.Map.AddObject(cannon);
                _game.Map.AddObject(melee);
                _game.Map.AddObject(super);
            }
        }
    }
}
