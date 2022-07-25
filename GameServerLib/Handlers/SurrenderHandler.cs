using GameServerCore.Domain;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.Buildings.AnimatedBuildings;
using LeagueSandbox.GameServer.Logging;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LeagueSandbox.GameServer.Handlers
{
    // TODO: Make the surrender UI button become clickable upon hitting SurrenderMinimumTime
    public class SurrenderHandler : IUpdate
    {
        private Dictionary<Champion, bool> _votes = new Dictionary<Champion, bool>();
        private Game _game;
        private static ILog _logger = LoggerProvider.GetLogger();
        private bool toEnd = false;
        private float toEndTimer = 3000.0f;

        public float SurrenderMinimumTime { get; set; }
        public float SurrenderRestTime { get; set; }
        public float SurrenderLength { get; set; }
        public float LastSurrenderTime { get; set; }
        public bool IsSurrenderActive { get; set; }
        public TeamId Team { get; set; }

        // TODO: The first two parameters are in milliseconds, the third is seconds. QoL fix this?
        public SurrenderHandler(Game g, TeamId team, float minTime, float restTime, float length)
        {
            _game = g;
            Team = team;
            SurrenderMinimumTime = minTime;
            SurrenderRestTime = restTime;
            SurrenderLength = length;
        }

        public Tuple<int, int> GetVoteCounts()
        {
            int yes = _votes.Count(kv => kv.Value == true);
            int no = _votes.Count - yes;
            return new Tuple<int, int>(yes, no);
        }

        public void HandleSurrender(int userId, Champion who, bool vote)
        {
            if (_game.GameTime < SurrenderMinimumTime)
            {
                _game.PacketNotifier.NotifyTeamSurrenderStatus(userId, who.Team, SurrenderReason.NotAllowedYet, 0, 0);
                return;
            }
            
            bool open = !IsSurrenderActive;
            if (!IsSurrenderActive && _game.GameTime < LastSurrenderTime + SurrenderRestTime)
            {
                _game.PacketNotifier.NotifyTeamSurrenderStatus(userId, who.Team, SurrenderReason.DontSpamSurrender, 0, 0);
                return;
            }
            IsSurrenderActive = true;
            LastSurrenderTime = _game.GameTime;
            _votes.Clear();

            if (_votes.ContainsKey(who))
            {
                _game.PacketNotifier.NotifyTeamSurrenderStatus(userId, who.Team, SurrenderReason.AlreadyVoted, 0, 0);
                return;
            }
            _votes[who] = vote;
            Tuple<int, int> voteCounts = GetVoteCounts();
            var players = _game.PlayerManager.GetPlayers(false);
            int total = players.Count;

            _logger.Info($"Champion {who.Model} voted {vote}. Currently {voteCounts.Item1} yes votes, {voteCounts.Item2} no votes, with {total} total players");

            _game.PacketNotifier.NotifyTeamSurrenderVote(who, open, vote, (byte)voteCounts.Item1, (byte)voteCounts.Item2, (byte)total, SurrenderLength);

            if (voteCounts.Item1 >= total - 1)
            {
                IsSurrenderActive = false;
                foreach (var p in players)
                {
                    _game.PacketNotifier.NotifyTeamSurrenderStatus(p.ClientId, Team, SurrenderReason.SurrenderAgreed, (byte)voteCounts.Item1, (byte)voteCounts.Item2);
                }

                toEnd = true;
            }
        }

        public void Update(float diff)
        {
            if (IsSurrenderActive && _game.GameTime >= LastSurrenderTime + (SurrenderLength * 1000.0f))
            {
                IsSurrenderActive = false;
                Tuple<int, int> count = GetVoteCounts();
                var players = _game.PlayerManager.GetPlayers(false);
                foreach (var p in players)
                {
                    if(p.Team == Team)
                    {
                        _game.PacketNotifier.NotifyTeamSurrenderStatus(p.ClientId, Team, SurrenderReason.VoteWasNoSurrender, (byte)count.Item1, (byte)count.Item2);
                    }
                }
            }

            if (toEnd)
            {
                toEndTimer -= diff;
                if(toEndTimer <= 0)
                {
                    //This will have to be changed in the future in order to properly support Map8 surrender.
                    Nexus ourNexus = (Nexus)_game.ObjectManager.GetObjects().First(o => o.Value is Nexus && o.Value.Team == Team).Value;
                    if (ourNexus == null)
                    {
                        _logger.Error("Unable to surrender correctly, couldn't find the nexus!");
                        return;
                    }
                    ourNexus.Die(null);
                }
            }
        }
    }
}
