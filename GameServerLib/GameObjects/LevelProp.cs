using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;

namespace LeagueSandbox.GameServer.GameObjects
{
    public class LevelProp : GameObject, ILevelProp
    {
        public string Name { get; }
        public string Model { get; }
        public float Z { get; }
        public float DirX { get; }
        public float DirY { get; }
        public float DirZ { get; }
        public float Unk1 { get; }
        public float Unk2 { get; }
        public byte SkinId { get; }

        public LevelProp(
            Game game,
            float x,
            float y,
            float z,
            float dirX,
            float dirY,
            float dirZ,
            float unk1,
            float unk2,
            string name,
            string model,
            byte skin = 0,
            uint netId = 0
        ) : base(game, x, y, 0, 0, netId)
        {
            Z = z;
            DirX = dirX;
            DirY = dirY;
            DirZ = dirZ;
            Unk1 = unk1;
            Unk2 = unk2;
            Name = name;
            Model = model;
            SkinId = skin;
            SetTeam(TeamId.TEAM_NEUTRAL);
        }

        public override float GetMoveSpeed()
        {
            return 0.0f;
        }
    }
}
