using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServerCore.Logic.Domain.GameObjects
{
    public interface IParticle : IGameObject
    {
        IChampion Owner { get; }
        string Name { get; }
        string BoneName { get; }
        float Size { get; }
    }
}
