using System;
using GameServerCore.Content;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class SpawnParticle : BasePacket
    {
        public SpawnParticle(INavGrid navGrid, IParticle particle)
            : base(PacketCmd.PKT_S2C_SPAWN_PARTICLE, particle.Owner.NetId)
        {
            Write((byte)1); // number of particles
            Write((uint)particle.Owner.GetChampionHash());
            WriteStringHash(particle.Name);
            Write(0x00000020); // flags ?

            Write((short)0); // Unk
            WriteStringHash(particle.BoneName);

            Write((byte)1); // number of targets ?
            WriteNetId(particle.Owner);
            WriteNetId(particle); // Particle net id ?
            WriteNetId(particle.Owner);

            if (particle.Target.IsSimpleTarget)
                Write(0);
            else
                WriteNetId(particle.Target as IGameObject);

            Write(0); // unk

            for (var i = 0; i < 3; ++i)
            {
                var ownerHeight = navGrid.GetHeightAtLocation(particle.Owner.X, particle.Owner.Y);
                var particleHeight = navGrid.GetHeightAtLocation(particle.X, particle.Y);
                var higherValue = Math.Max(ownerHeight, particleHeight);
                Write((short)((particle.Target.X - navGrid.MapWidth / 2) / 2));
                Write(higherValue);
                Write((short)((particle.Target.Y - navGrid.MapHeight / 2) / 2));
            }

            Write((uint)0); // unk
            Write((uint)0); // unk
            Write((uint)0); // unk
            Write((uint)0); // unk
            Write(particle.Size); // Particle size
        }
    }
}