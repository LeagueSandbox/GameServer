using GameServerCore.Enums;
using Newtonsoft.Json.Linq;

namespace GameServerCore.Domain
{
    public interface IGameFeatures
    {
        FeatureFlags CooldownsEnabled { get; }
        FeatureFlags ManaCostsEnabled { get; }
        FeatureFlags MinionSpawnsEnabled { get; }
        IGameFeatures SetGameFeatures(JToken gameInfo);
    }
}