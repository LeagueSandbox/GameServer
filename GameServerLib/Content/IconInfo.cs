using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using System;
using System.Collections.Generic;

namespace GameServerLib.Content
{
    public class IconInfo : IIconInfo
    {
        public string IconCategory { get; private set; } = string.Empty;
        public bool ChangeIcon { get; private set; } = false;
        public string BorderCategory { get; private set; } = string.Empty;
        public bool ChangeBorder { get; private set; } = false;
        public string BorderScriptName { get; private set; } = string.Empty;
        public List<TeamId> TeamsNotified { get; private set; } = new List<TeamId>((TeamId[])Enum.GetValues(typeof(TeamId)));
        
        private IAttackableUnit _owner;
        public IconInfo(IAttackableUnit owner)
        {
            _owner = owner;
        }

        public void SwapIcon(string iconCategory, bool changeIcon)
        {
            IconCategory = iconCategory;
            ChangeIcon = changeIcon;

            _owner.UpdateIcon();
        }

        public void SwapBorder(string borderCategory, bool changeBorder, string borderScriptName = "")
        {
            BorderCategory = borderCategory;
            ChangeBorder = changeBorder;
            BorderScriptName = borderScriptName;

            _owner.UpdateIcon();
        }

        public void AddNotifiedTeam(TeamId team)
        {
            if (!TeamsNotified.Contains(team))
            {
                TeamsNotified.Add(team);
            }
        }
    }
}
