using GameServerCore.Content;
using Newtonsoft.Json.Linq;

namespace GameServerCore.Domain
{
    public interface IPackage
    {
        string PackageName { get; }
        string PackagePath { get; }
        IContentFile GetContentFileFromJson(string contentType, string itemName);
        INavGrid GetNavGrid(int mapId);
        bool LoadScripts();
    }
}
