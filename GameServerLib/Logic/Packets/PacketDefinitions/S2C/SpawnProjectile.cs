using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SpawnProjectile : BasePacket
    {
        public SpawnProjectile(Projectile p)
            : base(PacketCmd.PKT_S2_C_SPAWN_PROJECTILE, p.NetId)
        {
            float targetZ = Game.Map.NavGrid.GetHeightAtLocation(p.Target.X, p.Target.Y);

            _buffer.Write(p.X);
            _buffer.Write(p.GetZ() + 100.0f);
            _buffer.Write(p.Y);
            _buffer.Write(p.X);
            _buffer.Write(p.GetZ());
            _buffer.Write(p.Y);
            _buffer.Write(-0.992436f); // Rotation X
            _buffer.Write(0); // Rotation Z
            _buffer.Write(-0.122766f); // Rotation Y
            _buffer.Write(-1984.871338f); // Unk
            _buffer.Write(-166.666656f); // Unk
            _buffer.Write(-245.531418f); // Unk
            _buffer.Write(p.X);
            _buffer.Write(p.GetZ() + 100.0f);
            _buffer.Write(p.Y);
            _buffer.Write(p.Target.X);
            _buffer.Write(Game.Map.NavGrid.GetHeightAtLocation(p.Target.X, p.Target.Y));
            _buffer.Write(p.Target.Y);
            _buffer.Write(p.X);
            _buffer.Write(p.GetZ());
            _buffer.Write(p.Y);
            _buffer.Write(0); // Unk ((float)castDelay ?)
            _buffer.Write(p.GetMoveSpeed()); // Projectile speed
            _buffer.Write(0); // Unk
            _buffer.Write(0); // Unk
            _buffer.Write(0x7f7fffff); // Unk
            _buffer.Write((byte)0); // Unk
            if (!p.Target.IsSimpleTarget)
            {
                _buffer.Write((short)0x6B); // Buffer size from here
            }
            else
            {
                _buffer.Write((short)0x66); // Buffer size from here
            }
            _buffer.Write(p.ProjectileId); // projectile ID (hashed name)
            _buffer.Write(0); // Second net ID
            _buffer.Write((byte)0); // spellLevel
            _buffer.Write(1.0f); // attackSpeedMod
            _buffer.Write((int)p.Owner.NetId);
            _buffer.Write((int)p.Owner.NetId);

            var c = p.Owner as Champion;
            if (c != null)
            {
                _buffer.Write(c.GetChampionHash());
            }
            else
            {
                _buffer.Write(0);
            }

            _buffer.Write((int)p.NetId);
            _buffer.Write(p.Target.X);
            _buffer.Write(Game.Map.NavGrid.GetHeightAtLocation(p.Target.X, p.Target.Y));
            _buffer.Write(p.Target.Y);
            _buffer.Write(p.Target.X);
            _buffer.Write(Game.Map.NavGrid.GetHeightAtLocation(p.Target.X, p.Target.Y) + 100.0f);
            _buffer.Write(p.Target.Y);
            if (!p.Target.IsSimpleTarget)
            {
                _buffer.Write((byte)0x01); // numTargets
                _buffer.Write((p.Target as AttackableUnit).NetId);
                _buffer.Write((byte)0); // hitResult
            }
            else
            {
                _buffer.Write((byte)0); // numTargets
            }
            _buffer.Write(1.0f); // designerCastTime -- Doesn't seem to matter
            _buffer.Write(0); // extraTimeForCast -- Doesn't seem to matter
            _buffer.Write(1.0f); // designerTotalTime -- Doesn't seem to matter
            _buffer.Write(0.0f); // cooldown -- Doesn't seem to matter
            _buffer.Write(0.0f); // startCastTime -- Doesn't seem to matter
            _buffer.Write((byte)0x00); // flags?
            _buffer.Write((byte)0x30); // slot?
            _buffer.Write(0.0f); // manaCost?
            _buffer.Write(p.X);
            _buffer.Write(p.GetZ());
            _buffer.Write(p.Y);
            _buffer.Write(0); // Unk
            _buffer.Write(0); // Unk
        }
    }
}