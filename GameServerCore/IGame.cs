﻿using GameServerCore.Maps;
using GameServerCore.Domain;
using System.Collections.Generic;

namespace GameServerCore
{
    public interface IGame: IUpdate
    {
        IMap Map { get; }
        IObjectManager ObjectManager { get; }
        IPlayerManager PlayerManager { get; }
        bool IsRunning { get;}
        bool IsPaused { get; set; }

        bool SetToExit { get; set; }

        bool HandleDisconnect(ulong userId);
    }
}
