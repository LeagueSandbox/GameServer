﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 
namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public interface IBuff
    {
        Unit Owner { get; }
    }
}
