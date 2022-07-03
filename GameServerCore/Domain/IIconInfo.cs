using GameServerCore.Enums;
using System;
using System.Collections.Generic;

namespace GameServerCore.Domain
{
    public interface IIconInfo
    {
        string IconCategory { get; }
        string BorderCategory { get; }
        string BorderScriptName { get; }
        void ChangeIcon(string iconCategory);
        void ResetIcon();
        void ChangeBorder(string borderCategory, string borderScriptName);
        void ResetBorder();
        void Sync(int userId, bool visible, bool force = false);
    }
}
