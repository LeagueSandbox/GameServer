using GameServerCore;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Maps;
using GameServerCore.NetInfo;
using GameServerCore.Packets.Enums;
using LeagueSandbox.GameServer.Logging;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LeagueSandbox.GameServer.Maps
{
    // TODO: Make the surrender UI button become clickable upon hitting SurrenderMinimumTime
    public class SurrenderHandler : IUpdate
    {
        private Dictionary<IChampion, bool> _votes = new Dictionary<IChampion, bool>();
        private Game _game;
        private ILog _log;

        public float SurrenderMinimumTime { get; set; }
        public float SurrenderRestTime { get; set; }
        public float SurrenderLength { get; set; }
        public float LastSurrenderTime { get; set; }
        public bool IsSurrenderActive { get; set; }
        public TeamId Team { get; set; }

        // TODO: The first two parameters are in milliseconds, the third is seconds. QoL fix this?
        public SurrenderHandler(Game g, TeamId team, float minTime, float restTime, float length)
        {
            _log = LoggerProvider.GetLogger();
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

        public void HandleSurrender(int userId, IChampion who, bool vote)
        {
            if (_game.GameTime < SurrenderMinimumTime)
            {
                _game.PacketNotifier.NotifyTeamSurrenderStatus(userId, who.Team, SurrenderReason.SURRENDER_TOO_EARLY, 0, 0);
                return;
            }
            
            bool open = !IsSurrenderActive;
            if (!IsSurrenderActive && _game.GameTime < LastSurrenderTime + SurrenderRestTime)
            {
                _game.PacketNotifier.NotifyTeamSurrenderStatus(userId, who.Team, SurrenderReason.SURRENDER_TOO_QUICKLY, 0, 0);
                return;
            }
            IsSurrenderActive = true;
            LastSurrenderTime = _game.GameTime;
            _votes.Clear();

            if (_votes.ContainsKey(who))
            {
                _game.PacketNotifier.NotifyTeamSurrenderStatus(userId, who.Team, SurrenderReason.SURRENDER_ALREADY_VOTED, 0, 0);
                return;
            }
            _votes[who] = vote;
            Tuple<int, int> voteCounts = GetVoteCounts();
            int total = _game.PlayerManager.GetPlayers().Count;

            _log.Info($"Champion {who.Model} voted {vote}. Currently {voteCounts.Item1} yes votes, {voteCounts.Item2} no votes, with {total} total players");

            _game.PacketNotifier.NotifyTeamSurrenderVote(who, open, vote, (byte)voteCounts.Item1, (byte)voteCounts.Item2, (byte)total, SurrenderLength);

            if (voteCounts.Item1 >= total - 1)
            {
                IsSurrenderActive = false;
                foreach (var p in _game.PlayerManager.GetPlayers())
                {
                    _game.PacketNotifier.NotifyTeamSurrenderStatus((int)p.Item1, Team, SurrenderReason.SURRENDER_PASSED, (byte)voteCounts.Item1, (byte)voteCounts.Item2); // TOOD: fix id casting
                }

                API.ApiFunctionManager.CreateTimer(3.0f, () =>
                {
                    INexus ourNexus = (INexus)_game.ObjectManager.GetObjects().First(o => o.Value is INexus && o.Value.Team == Team).Value;
                    if (ourNexus == null)
                    {
                        _log.Error("Unable to surrender correctly, couldn't find the nexus!");
                        return;
                    }
                    ourNexus.Die(null);
                });
            }
        }

        public void Update(float diff)
        {
            if (IsSurrenderActive && _game.GameTime >= LastSurrenderTime + (SurrenderLength * 1000.0f))
            {
                IsSurrenderActive = false;
                Tuple<int, int> count = GetVoteCounts();
                foreach (var p in _game.PlayerManager.GetPlayers().Where(kv => kv.Item2.Team == Team))
                    _game.PacketNotifier.NotifyTeamSurrenderStatus((int)p.Item1, Team, SurrenderReason.SURRENDER_FAILED, (byte)count.Item1, (byte)count.Item2); // TODO: fix id casting
            }
        }
    }
}
