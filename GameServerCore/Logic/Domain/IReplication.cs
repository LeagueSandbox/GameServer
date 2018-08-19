using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServerCore.Logic.Domain
{
    public interface IReplication
    {
        uint NetId { get; }
        /// <summary> Writing to this array will cause an exception </summary>
        IReplicate[,] Values { get; }
        bool Changed { get; }
        void MarkAsUnchanged();
    }
}
