using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Logic.Packets.PacketArgs.DTO;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketArgs
{
    public struct SpawnProjectileArgs
    {
        public UnitAtLocation Projectile { get; }
        public UnitAtLocation Target { get; }
        public bool TargetIsSimple { get; }
        public float MoveSpeed { get; }
        public int ProjectileHash { get; }
        public ChampionAtLocation ProjectileOwner { get; }

        public SpawnProjectileArgs(UnitAtLocation projectile, UnitAtLocation target, bool targetIsSimple, float moveSpeed, int projectileHash, ChampionAtLocation projectileOwner)
        {
            Projectile = projectile;
            Target = target;
            TargetIsSimple = targetIsSimple;
            MoveSpeed = moveSpeed;
            ProjectileHash = projectileHash;
            ProjectileOwner = projectileOwner;
        }
    }
}
