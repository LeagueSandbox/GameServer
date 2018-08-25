using System.Collections.Generic;

namespace GameServerCore.Domain
{
    public interface IRuneCollection
    {
        Dictionary<int, int> Runes { get; }
    }
}
