using System;
using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SpawnParticle : BasePacket
    {
        public SpawnParticle(Particle particle) 
            : base(PacketCmd.PKT_S2_C_SPAWN_PARTICLE, particle.Owner.NetId)
        {
            _buffer.Write((byte)1); // number of particles
            _buffer.Write((uint)particle.Owner.GetChampionHash());
            _buffer.Write(HashFunctions.HashString(particle.Name));
            _buffer.Write(0x00000020); // flags ?

            _buffer.Write((short)0); // Unk
            _buffer.Write(HashFunctions.HashString(particle.BoneName));

            _buffer.Write((byte)1); // number of targets ?
            _buffer.Write(particle.Owner.NetId);
            _buffer.Write(particle.NetId); // Particle net id ?
            _buffer.Write(particle.Owner.NetId);

            if (particle.Target.IsSimpleTarget)
                _buffer.Write(0);
            else
                _buffer.Write((particle.Target as GameObject).NetId);

            _buffer.Write(0); // unk

            for (var i = 0; i < 3; ++i)
            {
                var map = Game.Map;
                var ownerHeight = map.NavGrid.GetHeightAtLocation(particle.Owner.X, particle.Owner.Y);
                var particleHeight = map.NavGrid.GetHeightAtLocation(particle.X, particle.Y);
                var higherValue = Math.Max(ownerHeight, particleHeight);
                _buffer.Write((short)((particle.Target.X - Game.Map.NavGrid.MapWidth / 2) / 2));
                _buffer.Write(higherValue);
                _buffer.Write((short)((particle.Target.Y - Game.Map.NavGrid.MapHeight / 2) / 2));
            }

            _buffer.Write((uint)0); // unk
            _buffer.Write((uint)0); // unk
            _buffer.Write((uint)0); // unk
            _buffer.Write((uint)0); // unk
            _buffer.Write(particle.Size); // Particle size
        }
    }
}