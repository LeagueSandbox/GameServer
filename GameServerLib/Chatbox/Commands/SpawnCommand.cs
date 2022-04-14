using GameServerCore;
using GameServerCore.Enums;
using GameServerCore.NetInfo;
using LeagueSandbox.GameServer.Content;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Inventory;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace LeagueSandbox.GameServer.Chatbox.Commands
{
    public class SpawnCommand : ChatCommandBase
    {
        private readonly IPlayerManager _playerManager;

        Game _game;
        public override string Command => "spawn";
        public override string Syntax => $"{Command} champblue [champion], champpurple [champion], minionsblue, minionspurple, regionblue [size, time], regionpurple [size, time]";

        public SpawnCommand(ChatCommandManager chatCommandManager, Game game)
            : base(chatCommandManager, game)
        {
            _game = game;
            _playerManager = game.PlayerManager;
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');

            if (split.Length < 2)
            {
                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ShowSyntax();
            }
            else if (split[1].StartsWith("minions"))
            {
                split[1] = split[1].Replace("minions", "team_").ToUpper();
                if (!Enum.TryParse(split[1], out TeamId team) || team == TeamId.TEAM_NEUTRAL)
                {
                    ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                    ShowSyntax();
                    return;
                }

                SpawnMinionsForTeam(team, userId);
            }
            else if (split[1].StartsWith("champ"))
            {
                string championModel = "";

                split[1] = split[1].Replace("champ", "team_").ToUpper();
                if (!Enum.TryParse(split[1], out TeamId team) || team == TeamId.TEAM_NEUTRAL)
                {
                    ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                    ShowSyntax();
                    return;
                }

                if (split.Length > 2)
                {
                    championModel = arguments.Split(' ')[2];

                    try
                    {
                        Game.Config.ContentManager.GetContentFileFromJson("Stats", championModel);
                    }
                    catch (ContentNotFoundException)
                    {
                        ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR, "Character Name: " + championModel + " invalid.");
                        ShowSyntax();
                        return;
                    }

                    SpawnChampForTeam(team, userId, championModel);

                    return;
                }

                SpawnChampForTeam(team, userId, "Katarina");
            }
            else if (split[1].StartsWith("region"))
            {
                float size = 250.0f;
                float time = -1f;

                split[1] = split[1].Replace("region", "team_").ToUpper();
                if (!Enum.TryParse(split[1], out TeamId team) || team == TeamId.TEAM_NEUTRAL)
                {
                    ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                    ShowSyntax();
                    return;
                }

                if (split.Length > 2)
                {
                    size = float.Parse(arguments.Split(' ')[2]);

                    if (split.Length > 3)
                    {
                        time = float.Parse(arguments.Split(' ')[2]);
                    }
                }
                else if (split.Length > 4)
                {
                    ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                    ShowSyntax();
                    return;
                }

                SpawnRegionForTeam(team, userId, size, time);
            }
        }

        public void SpawnMinionsForTeam(TeamId team, int userId)
        {
            var championPos = _playerManager.GetPeerInfo(userId).Champion.Position;
            var random = new Random();

            var casterModel = Game.Map.MapScript.MinionModels[team][MinionSpawnType.MINION_TYPE_CASTER];
            var cannonModel = Game.Map.MapScript.MinionModels[team][MinionSpawnType.MINION_TYPE_CANNON];
            var meleeModel = Game.Map.MapScript.MinionModels[team][MinionSpawnType.MINION_TYPE_MELEE];
            var superModel = Game.Map.MapScript.MinionModels[team][MinionSpawnType.MINION_TYPE_SUPER];

            var minions = new[]
            {
                new Minion(Game, null, championPos, casterModel, casterModel, 0, team),
                new Minion(Game, null, championPos, cannonModel, cannonModel, 0, team),
                new Minion(Game, null, championPos, meleeModel, meleeModel, 0, team),
                new Minion(Game, null, championPos, superModel, superModel, 0, team)
            };

            const int X = 400;
            foreach (var minion in minions)
            {
                minion.SetPosition(championPos.X + random.Next(-X, X), championPos.Y + random.Next(-X, X));
                minion.PauseAi(true);
                minion.StopMovement();
                minion.UpdateMoveOrder(OrderType.Hold);
                Game.ObjectManager.AddObject(minion);
            }
        }

        public void SpawnChampForTeam(TeamId team, int userId, string model)
        {
            var championPos = _playerManager.GetPeerInfo(userId).Champion.Position;

            var runesTemp = new RuneCollection();
            var talents = new TalentInventory();
            var clientInfoTemp = new ClientInfo("", team, 0, 0, 0, model, new string[] { "SummonerHeal", "SummonerFlash" }, -1);
            uint num = (uint)_playerManager.GetPlayers(true).Count + 1;
            var playerTemp = new Tuple<uint, ClientInfo>(num, clientInfoTemp);

            _playerManager.AddPlayer(playerTemp);

            var c = new Champion(
                Game,
                model,
                0,
                0, // Doesnt matter at this point
                runesTemp,
                talents,
                clientInfoTemp,
                team: team
            );

            clientInfoTemp.Champion = c;

            c.SetPosition(championPos, false);
            c.StopMovement();
            c.UpdateMoveOrder(OrderType.Stop);
            c.LevelUp();

            Game.PacketNotifier.NotifyS2C_CreateHero(clientInfoTemp);
            Game.PacketNotifier.NotifyAvatarInfo(clientInfoTemp);
            Game.ObjectManager.AddObject(c);
            Game.PacketNotifier.NotifyEnterLocalVisibilityClient(c, ignoreVision: true);
            Game.PacketNotifier.NotifyOnReplication(c, partial: false);

            c.Stats.SetSpellEnabled(13, true);
            c.Stats.SetSummonerSpellEnabled(0, true);
            c.Stats.SetSummonerSpellEnabled(1, true);

            Game.PacketNotifier.NotifyEnterVisibilityClient(c, 0, true, true);

            ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.INFO, "Spawned Bot" + clientInfoTemp.Name + " as " + c.Model + " with NetID: " + c.NetId + ".");
        }

        public void SpawnRegionForTeam(TeamId team, int userId, float radius = 250.0f, float lifetime = -1.0f)
        {
            var championPos = _playerManager.GetPeerInfo(userId).Champion.Position;
            API.ApiFunctionManager.AddPosPerceptionBubble(new Vector2(championPos.X, championPos.Y), radius, lifetime, team, true);
        }
    }
}