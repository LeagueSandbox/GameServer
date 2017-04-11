using LeagueSandbox.GameServer.Logic.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Scripting.CSharp
{
    public interface IGameScript
    {
        void OnActivate(GameScriptInformation gameScriptInformation);
        void OnDeactivate();
    }
}
