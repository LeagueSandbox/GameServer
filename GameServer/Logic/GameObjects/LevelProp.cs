﻿namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class LevelProp : GameObject
    {
        public string Name { get; private set; }
        public string Model { get; private set; }
        public float Z { get; private set; }
        public float DirX { get; private set; }
        public float DirY { get; private set; }
        public float DirZ { get; private set; }
        public float Unk1 { get; private set; }
        public float Unk2 { get; private set; }

        public LevelProp(
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
            uint netId = 0
        ) : base(x, y, 0, 0, netId)
        {
            SetTeam(Enet.TeamId.TEAM_NEUTRAL);
            Z = z;
            DirX = dirX;
            DirY = dirY;
            DirZ = dirZ;
            Unk1 = unk1;
            Unk2 = unk2;
            Name = name;
            Model = model;
            Team = Enet.TeamId.TEAM_NEUTRAL;
        }

        public override float getMoveSpeed()
        {
            return 0.0f;
        }
    }
}
