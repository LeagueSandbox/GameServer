using System.Numerics;

namespace LeagueSandbox.GameServer.GameObjects
{
    public class LevelProp : GameObject
    {
        public byte NetNodeID { get; set; }
        public int SkinID { get; set; }
        public float Height { get; set; }
        public Vector3 PositionOffset { get; set; }
        public Vector3 Scale { get; set; }
        public byte SkillLevel { get; set; }
        public byte Rank { get; set; }
        public byte Type { get; set; }
        public string Name { get; set; }
        public string Model { get; set; }

        public LevelProp(
            Game game,
            byte netNodeId,
            string name,
            string model,
            Vector3 position,
            Vector3 direction,
            Vector3 posOffset,
            Vector3 scale,
            int skinId = 0,
            byte skillLevel = 0,
            byte rank = 0,
            byte type = 2,
            uint netId = 64


        ) : base(game, new Vector2(position.X, position.Z),0, 0, 0, netId)
        {
            NetNodeID = netNodeId;
            SkinID = skinId;
            Height = position.Y;
            Direction = direction;
            PositionOffset = posOffset;
            Scale = scale;
            SkillLevel = skillLevel;
            Rank = rank;
            Type = type;
            Name = name;
            Model = model;
        }
    }
}