using GameServerCore.Content;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using System.Collections.Generic;
using System.Numerics;

namespace LeagueSandbox.GameServer.GameObjects.Spell
{
    public class CastInfo : ICastInfo
    {
        public uint SpellHash { get; set; }
        public uint SpellNetID { get; set; }
        public byte SpellLevel { get; set; }
        public float AttackSpeedModifier { get; set; } = 1.0f;
        public IObjAiBase Owner { get; set; }
        public uint SpellChainOwnerNetID { get; set; } // TODO: Figure out what this is used for
        public uint PackageHash { get; set; }
        public uint MissileNetID { get; set; }
        public Vector3 TargetPosition { get; set; }
        public Vector3 TargetPositionEnd { get; set; }

        public List<ICastTarget> Targets { get; set; }

        public float DesignerCastTime { get; set; }
        public float ExtraCastTime { get; set; }
        public float DesignerTotalTime { get; set; }
        public float Cooldown { get; set; }
        public float StartCastTime { get; set; }

        public bool IsAutoAttack { get; set; } = false;
        public bool UseAttackCastTime { get; set; } = false;
        public bool UseAttackCastDelay { get; set; } = false;
        public bool IsSecondAutoAttack { get; set; } = false;
        public bool IsForceCastingOrChannel { get; set; } = false;
        public bool IsOverrideCastPosition { get; set; } = false;
        public bool IsClickCasted { get; set; } = false;

        public byte SpellSlot { get; set; }
        public float ManaCost { get; set; }
        public Vector3 SpellCastLaunchPosition { get; set; }
        public int AmmoUsed { get; set; }
        public float AmmoRechargeTime { get; set; }
    }
}