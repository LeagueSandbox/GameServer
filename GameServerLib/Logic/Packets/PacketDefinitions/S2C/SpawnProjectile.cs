using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SpawnProjectile : BasePacket
    {
        public SpawnProjectile(Projectile p) 
            : base(PacketCmd.PKT_S2C_SpawnProjectile, p.NetId)
        {
            float targetZ = Game.Map.NavGrid.GetHeightAtLocation(p.Target.X, p.Target.Y);

            buffer.Write((float)p.X);
            buffer.Write((float)p.GetZ() + 100.0f);
            buffer.Write((float)p.Y);
            buffer.Write((float)p.X);
            buffer.Write((float)p.GetZ());
            buffer.Write((float)p.Y);
            buffer.Write((float)-0.992436f); // Rotation X
            buffer.Write((int)0); // Rotation Z
            buffer.Write((float)-0.122766f); // Rotation Y
            buffer.Write((float)-1984.871338f); // Unk
            buffer.Write((float)-166.666656f); // Unk
            buffer.Write((float)-245.531418f); // Unk
            buffer.Write((float)p.X);
            buffer.Write((float)p.GetZ() + 100.0f);
            buffer.Write((float)p.Y);
            buffer.Write((float)p.Target.X);
            buffer.Write((float)Game.Map.NavGrid.GetHeightAtLocation(p.Target.X, p.Target.Y));
            buffer.Write((float)p.Target.Y);
            buffer.Write((float)p.X);
            buffer.Write((float)p.GetZ());
            buffer.Write((float)p.Y);
            buffer.Write((int)0); // Unk ((float)castDelay ?)
            buffer.Write((float)p.getMoveSpeed()); // Projectile speed
            buffer.Write((int)0); // Unk
            buffer.Write((int)0); // Unk
            buffer.Write((int)0x7f7fffff); // Unk
            buffer.Write((byte)0); // Unk
            if (!p.Target.IsSimpleTarget)
            {
                buffer.Write((short)0x6B); // Buffer size from here
            }
            else
            {
                buffer.Write((short)0x66); // Buffer size from here
            }
            buffer.Write((int)p.ProjectileId); // projectile ID (hashed name)
            buffer.Write((int)0); // Second net ID
            buffer.Write((byte)0); // spellLevel
            buffer.Write((float)1.0f); // attackSpeedMod
            buffer.Write((int)p.Owner.NetId);
            buffer.Write((int)p.Owner.NetId);

            var c = p.Owner as Champion;
            if (c != null)
            {
                buffer.Write((int)c.getChampionHash());
            }
            else
            {
                buffer.Write((int)0);
            }

            buffer.Write((int)p.NetId);
            buffer.Write((float)p.Target.X);
            buffer.Write((float)Game.Map.NavGrid.GetHeightAtLocation(p.Target.X, p.Target.Y));
            buffer.Write((float)p.Target.Y);
            buffer.Write((float)p.Target.X);
            buffer.Write((float)Game.Map.NavGrid.GetHeightAtLocation(p.Target.X, p.Target.Y) + 100.0f);
            buffer.Write((float)p.Target.Y);
            if (!p.Target.IsSimpleTarget)
            {
                buffer.Write((byte)0x01); // numTargets
                buffer.Write((p.Target as AttackableUnit).NetId);
                buffer.Write((byte)0); // hitResult
            }
            else
            {
                buffer.Write((byte)0); // numTargets
            }
            buffer.Write((float)1.0f); // designerCastTime -- Doesn't seem to matter
            buffer.Write((int)0); // extraTimeForCast -- Doesn't seem to matter
            buffer.Write((float)1.0f); // designerTotalTime -- Doesn't seem to matter
            buffer.Write((float)0.0f); // cooldown -- Doesn't seem to matter
            buffer.Write((float)0.0f); // startCastTime -- Doesn't seem to matter
            buffer.Write((byte)0x00); // flags?
            buffer.Write((byte)0x30); // slot?
            buffer.Write((float)0.0f); // manaCost?
            buffer.Write((float)p.X);
            buffer.Write((float)p.GetZ());
            buffer.Write((float)p.Y);
            buffer.Write((int)0); // Unk
            buffer.Write((int)0); // Unk
        }
    }
}