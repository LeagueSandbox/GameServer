using ENet;
using LeagueSandbox.GameServer.Logic.GameObjects;
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
            var split = arguments.ToLower().Split(' ');

            if (split.Length < 2)
            {
                _owner.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ShowSyntax();
            }
            else if (split[1] == "minions")
            {
                var game = _owner.GetGame();
                var champion = game.GetPeerInfo(peer).GetChampion();
                var random = new Random();

                Minion caster = new Minion(game, game.GetNewNetID(), MinionSpawnType.MINION_TYPE_CASTER, MinionSpawnPosition.SPAWN_RED_MID);
                Minion cannon = new Minion(game, game.GetNewNetID(), MinionSpawnType.MINION_TYPE_CANNON, MinionSpawnPosition.SPAWN_RED_MID);
                Minion melee = new Minion(game, game.GetNewNetID(), MinionSpawnType.MINION_TYPE_MELEE, MinionSpawnPosition.SPAWN_RED_MID);
                Minion super = new Minion(game, game.GetNewNetID(), MinionSpawnType.MINION_TYPE_SUPER, MinionSpawnPosition.SPAWN_RED_MID);

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

                game.GetMap().AddObject(caster);
                game.GetMap().AddObject(cannon);
                game.GetMap().AddObject(melee);
                game.GetMap().AddObject(super);
            }
        }
    }
}
