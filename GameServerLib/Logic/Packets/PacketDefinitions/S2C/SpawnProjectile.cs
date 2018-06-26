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

            _buffer.Write((float)p.X);
            _buffer.Write((float)p.GetZ() + 100.0f);
            _buffer.Write((float)p.Y);
            _buffer.Write((float)p.X);
            _buffer.Write((float)p.GetZ());
            _buffer.Write((float)p.Y);
            _buffer.Write((float)-0.992436f); // Rotation X
            _buffer.Write((int)0); // Rotation Z
            _buffer.Write((float)-0.122766f); // Rotation Y
            _buffer.Write((float)-1984.871338f); // Unk
            _buffer.Write((float)-166.666656f); // Unk
            _buffer.Write((float)-245.531418f); // Unk
            _buffer.Write((float)p.X);
            _buffer.Write((float)p.GetZ() + 100.0f);
            _buffer.Write((float)p.Y);
            _buffer.Write((float)p.Target.X);
            _buffer.Write((float)Game.Map.NavGrid.GetHeightAtLocation(p.Target.X, p.Target.Y));
            _buffer.Write((float)p.Target.Y);
            _buffer.Write((float)p.X);
            _buffer.Write((float)p.GetZ());
            _buffer.Write((float)p.Y);
            _buffer.Write((int)0); // Unk ((float)castDelay ?)
            _buffer.Write((float)p.GetMoveSpeed()); // Projectile speed
            _buffer.Write((int)0); // Unk
            _buffer.Write((int)0); // Unk
            _buffer.Write((int)0x7f7fffff); // Unk
            _buffer.Write((byte)0); // Unk
            if (!p.Target.IsSimpleTarget)
            {
                _buffer.Write((short)0x6B); // Buffer size from here
            }
            else
            {
                _buffer.Write((short)0x66); // Buffer size from here
            }
            _buffer.Write((int)p.ProjectileId); // projectile ID (hashed name)
            _buffer.Write((int)0); // Second net ID
            _buffer.Write((byte)0); // spellLevel
            _buffer.Write((float)1.0f); // attackSpeedMod
            _buffer.Write((int)p.Owner.NetId);
            _buffer.Write((int)p.Owner.NetId);

            var c = p.Owner as Champion;
            if (c != null)
            {
                _buffer.Write((int)c.GetChampionHash());
            }
            else
            {
                _buffer.Write((int)0);
            }

            _buffer.Write((int)p.NetId);
            _buffer.Write((float)p.Target.X);
            _buffer.Write((float)Game.Map.NavGrid.GetHeightAtLocation(p.Target.X, p.Target.Y));
            _buffer.Write((float)p.Target.Y);
            _buffer.Write((float)p.Target.X);
            _buffer.Write((float)Game.Map.NavGrid.GetHeightAtLocation(p.Target.X, p.Target.Y) + 100.0f);
            _buffer.Write((float)p.Target.Y);
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
            _buffer.Write((float)1.0f); // designerCastTime -- Doesn't seem to matter
            _buffer.Write((int)0); // extraTimeForCast -- Doesn't seem to matter
            _buffer.Write((float)1.0f); // designerTotalTime -- Doesn't seem to matter
            _buffer.Write((float)0.0f); // cooldown -- Doesn't seem to matter
            _buffer.Write((float)0.0f); // startCastTime -- Doesn't seem to matter
            _buffer.Write((byte)0x00); // flags?
            _buffer.Write((byte)0x30); // slot?
            _buffer.Write((float)0.0f); // manaCost?
            _buffer.Write((float)p.X);
            _buffer.Write((float)p.GetZ());
            _buffer.Write((float)p.Y);
            _buffer.Write((int)0); // Unk
            _buffer.Write((int)0); // Unk
        }
    }
}