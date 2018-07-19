using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Logic.GameObjects.Missiles;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SpawnProjectile : BasePacket
    {
        public SpawnProjectile(Game game, Projectile p)
            : base(game, PacketCmd.PKT_S2C_SPAWN_PROJECTILE, p.NetId)
        {
            var targetZ = Game.Map.NavGrid.GetHeightAtLocation(p.Target.X, p.Target.Y);

            Write(p.X);
            Write(p.GetZ() + 100.0f);
            Write(p.Y);
            Write(p.X);
            Write(p.GetZ());
            Write(p.Y);
            Write(-0.992436f); // Rotation X
            Write(0); // Rotation Z
            Write(-0.122766f); // Rotation Y
            Write(-1984.871338f); // Unk
            Write(-166.666656f); // Unk
            Write(-245.531418f); // Unk
            Write(p.X);
            Write(p.GetZ() + 100.0f);
            Write(p.Y);
            Write(p.Target.X);
            Write(Game.Map.NavGrid.GetHeightAtLocation(p.Target.X, p.Target.Y));
            Write(p.Target.Y);
            Write(p.X);
            Write(p.GetZ());
            Write(p.Y);
            Write(0); // Unk ((float)castDelay ?)
            Write(p.GetMoveSpeed()); // Projectile speed
            Write(0); // Unk
            Write(0); // Unk
            Write(0x7f7fffff); // Unk
            Write((byte)0); // Unk
            if (!p.Target.IsSimpleTarget)
            {
                Write((short)0x6B); // Buffer size from here
            }
            else
            {
                Write((short)0x66); // Buffer size from here
            }
            Write(p.ProjectileId); // projectile ID (hashed name)
            Write(0); // Second net ID
            Write((byte)0); // spellLevel
            Write(1.0f); // attackSpeedMod
            WriteNetId(p.Owner);
            WriteNetId(p.Owner);

            var c = p.Owner as Champion;
            if (c != null)
            {
                Write(c.GetChampionHash());
            }
            else
            {
                Write(0);
            }

            WriteNetId(p);
            Write(p.Target.X);
            Write(Game.Map.NavGrid.GetHeightAtLocation(p.Target.X, p.Target.Y));
            Write(p.Target.Y);
            Write(p.Target.X);
            Write(Game.Map.NavGrid.GetHeightAtLocation(p.Target.X, p.Target.Y) + 100.0f);
            Write(p.Target.Y);
            if (!p.Target.IsSimpleTarget)
            {
                Write((byte)0x01); // numTargets
                WriteNetId(p.Target as AttackableUnit);
                Write((byte)0); // hitResult
            }
            else
            {
                Write((byte)0); // numTargets
            }
            Write(1.0f); // designerCastTime -- Doesn't seem to matter
            Write(0); // extraTimeForCast -- Doesn't seem to matter
            Write(1.0f); // designerTotalTime -- Doesn't seem to matter
            Write(0.0f); // cooldown -- Doesn't seem to matter
            Write(0.0f); // startCastTime -- Doesn't seem to matter
            Write((byte)0x00); // flags?
            Write((byte)0x30); // slot?
            Write(0.0f); // manaCost?
            Write(p.X);
            Write(p.GetZ());
            Write(p.Y);
            Write(0); // Unk
            Write(0); // Unk
        }
    }
}