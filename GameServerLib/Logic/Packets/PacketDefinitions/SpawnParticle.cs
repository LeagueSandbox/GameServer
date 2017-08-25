using System;
using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets
{
    public class SpawnParticle : BasePacket
    {
        public SpawnParticle(Particle particle) : base(PacketCmd.PKT_S2C_SpawnParticle, particle.Owner.NetId)
        {
            buffer.Write((byte)1); // number of particles
            buffer.Write((uint)particle.Owner.getChampionHash());
            buffer.Write((uint)HashFunctions.HashString(particle.Name));
            buffer.Write((int)0x00000020); // flags ?

            buffer.Write((short)0); // Unk
            buffer.Write((uint)HashFunctions.HashString(particle.BoneName));

            buffer.Write((byte)1); // number of targets ?
            buffer.Write((uint)particle.Owner.NetId);
            buffer.Write((uint)particle.NetId); // Particle net id ?
            buffer.Write((uint)particle.Owner.NetId);

            if (particle.Target.IsSimpleTarget)
                buffer.Write((int)0);
            else
                buffer.Write((particle.Target as GameObject).NetId);

            buffer.Write((int)0); // unk

            for (var i = 0; i < 3; ++i)
            {
                var map = Game.Map;
                var ownerHeight = map.AIMesh.GetHeightAtLocation(particle.Owner.X, particle.Owner.Y);
                var particleHeight = map.AIMesh.GetHeightAtLocation(particle.X, particle.Y);
                var higherValue = Math.Max(ownerHeight, particleHeight);
                buffer.Write((short)((particle.Target.X - Game.Map.AIMesh.getWidth() / 2) / 2));
                buffer.Write((float)higherValue);
                buffer.Write((short)((particle.Target.Y - Game.Map.AIMesh.getHeight() / 2) / 2));
            }

            buffer.Write((uint)0); // unk
            buffer.Write((uint)0); // unk
            buffer.Write((uint)0); // unk
            buffer.Write((uint)0); // unk
            buffer.Write((float)particle.Size); // Particle size
        }
    }
}