using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketArgs;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SpawnProjectile : BasePacket
    {
        public SpawnProjectile(SpawnProjectileArgs args)
            : base(PacketCmd.PKT_S2C_SpawnProjectile, args.Projectile.UnitNetId)
        {
            buffer.Write((float)args.Projectile.X);
            buffer.Write((float)args.Projectile.Z + 100.0f);
            buffer.Write((float)args.Projectile.Y);
            buffer.Write((float)args.Projectile.X);
            buffer.Write((float)args.Projectile.Z);
            buffer.Write((float)args.Projectile.Y);
            buffer.Write((float)-0.992436f); // Rotation X
            buffer.Write((int)0); // Rotation Z
            buffer.Write((float)-0.122766f); // Rotation Y
            buffer.Write((float)-1984.871338f); // Unk
            buffer.Write((float)-166.666656f); // Unk
            buffer.Write((float)-245.531418f); // Unk
            buffer.Write((float)args.Projectile.X);
            buffer.Write((float)args.Projectile.Z + 100.0f);
            buffer.Write((float)args.Projectile.Y);
            buffer.Write((float)args.Target.X);
            buffer.Write((float)args.Target.Z);
            buffer.Write((float)args.Target.Y);
            buffer.Write((float)args.Projectile.X);
            buffer.Write((float)args.Projectile.Z);
            buffer.Write((float)args.Projectile.Y);
            buffer.Write((int)0); // Unk ((float)castDelay ?)
            buffer.Write((float)args.MoveSpeed); // Projectile speed
            buffer.Write((int)0); // Unk
            buffer.Write((int)0); // Unk
            buffer.Write((int)0x7f7fffff); // Unk
            buffer.Write((byte)0); // Unk
            if (!args.TargetIsSimple)
            {
                buffer.Write((short)0x6B); // Buffer size from here
            }
            else
            {
                buffer.Write((short)0x66); // Buffer size from here
            }
            buffer.Write((int)args.ProjectileHash); // projectile ID (hashed name)
            buffer.Write((int)0); // Second net ID
            buffer.Write((byte)0); // spellLevel
            buffer.Write((float)1.0f); // attackSpeedMod
            buffer.Write((uint)args.ProjectileOwner.UnitNetId);
            buffer.Write((uint)args.ProjectileOwner.UnitNetId);
            buffer.Write((int)args.ProjectileOwner.UnitHash);
            buffer.Write((int)args.Projectile.UnitNetId);
            buffer.Write((float)args.Target.X);
            buffer.Write((float)args.Target.Z);
            buffer.Write((float)args.Target.Y);
            buffer.Write((float)args.Target.X);
            buffer.Write((float)args.Target.Z + 100.0f);
            buffer.Write((float)args.Target.Y);
            if (!args.TargetIsSimple)
            {
                buffer.Write((byte)0x01); // numTargets
                buffer.Write(args.Target.UnitNetId);
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
            buffer.Write((float)args.Projectile.X);
            buffer.Write((float)args.Projectile.Z);
            buffer.Write((float)args.Projectile.Y);
            buffer.Write((int)0); // Unk
            buffer.Write((int)0); // Unk
        }
    }
}