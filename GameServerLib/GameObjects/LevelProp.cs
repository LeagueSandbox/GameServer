using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using System.Numerics;

namespace LeagueSandbox.GameServer.GameObjects
{
    public class LevelProp : GameObject, ILevelProp
    {
        public string Name { get; }
        public string Model { get; }
        public float Height { get; }
        public float Unk1 { get; }
        public float Unk2 { get; }
        public byte SkinId { get; }

        public LevelProp(
            Game game,
            Vector2 position,
            float z,
            Vector3 direction,
            float unk1,
            float unk2,
            string name,
            string model,
            byte skin = 0,
            uint netId = 0
        ) : base(game, position, 0, 0, netId)
        {
            Height = z;
            Direction = direction;
            Unk1 = unk1;
            Unk2 = unk2;
            Name = name;
            Model = model;
            SkinId = skin;
        }
    }
}
