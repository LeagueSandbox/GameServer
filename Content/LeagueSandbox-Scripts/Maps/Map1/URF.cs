using System;
using GameServerCore.Domain;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Maps;
using LeagueSandbox.GameServer.Content;

namespace MapScripts.Map1
{
    public class URF : CLASSIC
    {
        public override IGlobalData GlobalData { get; set; } = new GlobalData { PercentCooldownModMinimun = 0.8f};
        public override void Init(IMapScriptHandler map)
        {
            base.Init(map);
            map.SetGameFeatures(GameServerCore.Enums.FeatureFlags.EnableManaCosts, false);
        }

        public override void OnMatchStart()
        {
            foreach(var player in GetAllPlayers())
            {
                //There are mentions to a "rewindcof" buff being loaded too, but it's functions are yet unknown
                AddBuff("InternalTestBuff", float.MaxValue, 1, null, player, null, true);
            }
        }
    }
}
