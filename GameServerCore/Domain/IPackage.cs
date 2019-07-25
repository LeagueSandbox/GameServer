using Newtonsoft.Json.Linq;

namespace GameServerCore.Domain
{
    public interface IPackage
    {
        IContentFile GetContentFileFromJson(string contentType, string itemName);
        JToken GetMapSpawnData(int mapId);
        bool LoadScripts();
    }
}
