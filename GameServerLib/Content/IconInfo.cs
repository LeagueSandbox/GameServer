using System.Collections.Generic;
using LeagueSandbox.GameServer;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;

namespace GameServerLib.Content
{
    public class IconInfo
    {
        public string IconCategory { get; private set; } = string.Empty;
        public string BorderCategory { get; private set; } = string.Empty;
        public string BorderScriptName { get; private set; } = string.Empty;

        private int _iconState = 0;
        private int _borderState = 0;
        private Dictionary<int, (int Icon, int Border)> _lastStateSeenByPlayer
          = new Dictionary<int, (int, int)>();

        private AttackableUnit _owner;
        private Game _game;
        public IconInfo(Game game, AttackableUnit owner)
        {
            _game = game;
            _owner = owner;
        }
        
        public void ChangeIcon(string iconCategory)
        {
            IconCategory = iconCategory;
            _iconState++;
        }

        public void ResetIcon()
        {
            IconCategory = string.Empty;
            _iconState = 0;
        }

        public void ChangeBorder(string borderCategory, string borderScriptName)
        {
            BorderCategory = borderCategory;
            BorderScriptName = borderScriptName;
            _borderState++;
        }

        public void ResetBorder()
        {
            BorderCategory = string.Empty;
            BorderScriptName = string.Empty;
            _borderState = 0;
        }

        public void Sync(int userId, bool visible, bool force = false)
        {
            if(visible)
            {
                bool changeIcon;
                bool changeBorder;
                if(force)
                {
                    changeIcon = _iconState != 0;
                    changeBorder = _borderState != 0;
                }
                else
                {
                    (int Icon, int Border) lastSeenState = _lastStateSeenByPlayer.GetValueOrDefault(userId, (0, 0));
                    changeIcon = lastSeenState.Icon != _iconState;
                    changeBorder = lastSeenState.Border != _borderState;
                }
                if(changeIcon || changeBorder)
                {
                    _game.PacketNotifier.NotifyS2C_UnitSetMinimapIcon(userId, _owner, changeIcon, changeBorder);
                    _lastStateSeenByPlayer[userId] = (_iconState, _borderState);
                }
            }
            else if(force)
            {
                _lastStateSeenByPlayer[userId] = (0, 0);
            }
        }
    }
}
