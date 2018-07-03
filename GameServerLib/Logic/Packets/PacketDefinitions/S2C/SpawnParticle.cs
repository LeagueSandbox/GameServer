using System;
using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SpawnParticle : BasePacket
    {
        public SpawnParticle(Particle particle)
            : base(PacketCmd.PKT_S2C_SPAWN_PARTICLE, particle.Owner.NetId)
        {
            Write((byte)1); // number of particles
            Write((uint)particle.Owner.GetChampionHash());
            Write(HashFunctions.HashString(particle.Name));
            Write(0x00000020); // flags ?

            Write((short)0); // Unk
            Write(HashFunctions.HashString(particle.BoneName));

            Write((byte)1); // number of targets ?
            Write(particle.Owner.NetId);
            Write(particle.NetId); // Particle net id ?
            Write(particle.Owner.NetId);

            if (particle.Target.IsSimpleTarget)
                Write(0);
            else
                Write((particle.Target as GameObject).NetId);

            Write(0); // unk

            for (var i = 0; i < 3; ++i)
            {
                var map = Game.Map;
                var ownerHeight = map.NavGrid.GetHeightAtLocation(particle.Owner.X, particle.Owner.Y);
                var particleHeight = map.NavGrid.GetHeightAtLocation(particle.X, particle.Y);
                var higherValue = Math.Max(ownerHeight, particleHeight);
                Write((short)((particle.Target.X - Game.Map.NavGrid.MapWidth / 2) / 2));
                Write(higherValue);
                Write((short)((particle.Target.Y - Game.Map.NavGrid.MapHeight / 2) / 2));
            }

            Write((uint)0); // unk
            Write((uint)0); // unk
            Write((uint)0); // unk
            Write((uint)0); // unk
            Write(particle.Size); // Particle size
        }
    }
}