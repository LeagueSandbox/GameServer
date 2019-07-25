using Newtonsoft.Json.Linq;

namespace GameServerCore.Domain
{
    public interface IPackage
    {
        string PackageName { get; }
        string PackagePath { get; }
        IContentFile GetContentFileFromJson(string contentType, string itemName);
        JToken GetMapSpawnData(int mapId);
        bool LoadScripts();
    }
}
