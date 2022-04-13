using System.Collections.Generic;

namespace GameServerCore.Domain
{
    public interface IMasteryCollection
    {
        public void Add(string name, byte level);
    }
}
