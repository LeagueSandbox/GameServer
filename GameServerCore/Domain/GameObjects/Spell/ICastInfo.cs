﻿using GameServerCore.Domain.GameObjects;
using System.Collections.Generic;
using System.Numerics;

namespace GameServerCore.Domain.GameObjects.Spell
{
    public interface ICastInfo
    {
        uint SpellHash { get; set; }
        uint SpellNetID { get; set; }
        byte SpellLevel { get; set; }
        float AttackSpeedModifier { get; set; }
        IObjAiBase Owner { get; set; }
        uint SpellChainOwnerNetID { get; set; } // TODO: Figure out what this is used for
        uint PackageHash { get; set; }
        uint MissileNetID { get; set; }
        Vector3 TargetPosition { get; set; }
        Vector3 TargetPositionEnd { get; set; }

        List<ICastTarget> Targets { get; set; }

        float DesignerCastTime { get; set; }
        float ExtraCastTime { get; set; }
        float DesignerTotalTime { get; set; }
        float Cooldown { get; set; }
        float StartCastTime { get; set; }

        bool IsAutoAttack { get; set; }
        bool UseAttackCastTime { get; set; }
        bool UseAttackCastDelay { get; set; }
        bool IsSecondAutoAttack { get; set; }
        bool IsForceCastingOrChannel { get; set; }
        bool IsOverrideCastPosition { get; set; }
        bool IsClickCasted { get; set; }

        byte SpellSlot { get; set; }
        float ManaCost { get; set; }
        Vector3 SpellCastLaunchPosition { get; set; }
        int AmmoUsed { get; set; }
        float AmmoRechargeTime { get; set; }
    }
}