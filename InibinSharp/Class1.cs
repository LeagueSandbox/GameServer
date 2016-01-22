using InibinSharp.RAF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InibinSharp
{
    class Class1
    {
        static void Main(string[] args)
        {
            var root = new RAFMasterFileList(@"C:\Riot Games\League of Legends\RADS\projects\lol_game_client\filearchives");
            var ss = root.SearchFileEntries(@"LEVELS/Map1/AIPath.aimesh").First();
            var aimesh = new AIMesh(ss);
        }
    }
}
