using GameServerCore.Enums;
using System;
using System.Collections.Generic;

namespace GameServerCore.Domain
{
    public interface IIconInfo
    {
        string IconCategory { get; }
        bool ChangeIcon { get; }
        string BorderCategory { get; }
        bool ChangeBorder { get; }
        string BorderScriptName { get; }
        List<TeamId> TeamsNotified { get; }
        void SwapIcon(string iconCategory, bool changeIcon);
        void SwapBorder(string borderCategory, bool changeBorder, string borderScriptName = "");
        void AddNotifiedTeam(TeamId team);
    }
}
