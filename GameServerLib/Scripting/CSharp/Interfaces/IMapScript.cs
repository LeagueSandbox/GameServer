using System.Collections.Generic;
using System.Numerics;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.Content;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace GameServerCore.Domain
{
    public interface IMapScript : IUpdate
    {
        MapScriptMetadata MapScriptMetadata { get; }
        bool HasFirstBloodHappened { get; set; }
        string LaneMinionAI { get; }
        Dictionary<TeamId, Dictionary<int, Dictionary<int, Vector2>>> PlayerSpawnPoints { get; }
        Dictionary<TeamId, Dictionary<MinionSpawnType, string>> MinionModels { get; }
        void Init(Dictionary<GameObjectTypes, List<MapObject>> mapObjects);
        void OnMatchStart()
        {
        }
        void SpawnAllCamps()
        {
        }
        Vector2 GetFountainPosition(TeamId team)
        {
            return Vector2.Zero;
        }
    }
}