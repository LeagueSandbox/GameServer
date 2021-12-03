using System;
using GameServerCore.Domain;
using GameServerLib.GameObjects.GlobalData;
using LeagueSandbox.GameServer.GameObjects.Spell;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer;
using GameServerCore.Maps;

namespace MapScripts.Map1
{
    public class URF : CLASSIC
    {
        public override IGlobalData GlobalData { get; set; } = new GlobalData { PercentCooldownModMinimun = 0.8f};
        public override void Init(IMapScriptHandler map)
        {
            base.Init(map);
            base.GameFeatures = new GameFeatures
            {
                CooldownsEnabled = map.GetGameFeatures().CooldownsEnabled,
                ManaCostsEnabled = false,
                MinionSpawnsEnabled = map.GetGameFeatures().MinionSpawnsEnabled
            };
        }

        public override void OnMatchStart()
        {
            foreach(var player in base._map.GetPlayers())
            {
                //There are mentions to a "rewindcof" buff being loaded too, but it's functions are yet unknown
                AddBuff("InternalTestBuff", float.MaxValue, 1, null, player, null, true);
            }
        }
    }
}
