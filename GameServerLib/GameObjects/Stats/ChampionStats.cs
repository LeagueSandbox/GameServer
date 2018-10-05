using System;
using System.Runtime.InteropServices;
using GameServerCore.Domain.GameObjects;

namespace LeagueSandbox.GameServer.GameObjects.Stats
{
    // deviates from the style guide
    // see discussion here:https://github.com/LeagueSandbox/GameServer/pull/583
    [StructLayout(LayoutKind.Explicit)]
    public class ChampionStats : IChampionStats
    {
        [FieldOffset(0)]
        public int _Assists;
        [FieldOffset(8)]
        public int _Kills;
        [FieldOffset(16)]
        public int _DoubleKills;
        [FieldOffset(32)]
        public int _UnrealKills;
        [FieldOffset(36)]
        public float _GoldEarned;
        [FieldOffset(40)]
        public float _GoldSpent;
        [FieldOffset(84)]
        public int _CurrentKillingSpree;
        [FieldOffset(88)]
        public float _LargestCriticalStrike;
        [FieldOffset(92)]
        public int _LargestKillingSpree;
        [FieldOffset(96)]
        public int _LargestMultiKill;
        [FieldOffset(104)]
        public float _LongestTimeSpentLiving;
        [FieldOffset(108)]
        public float _MagicDamageDealt;
        [FieldOffset(112)]
        public float _MagicDamageDealtToChampions;
        [FieldOffset(116)]
        public float _MagicDamageTaken;
        [FieldOffset(120)]
        public int _MinionsKilled;
        [FieldOffset(126)]
        public int _NeutralMinionsKilled;
        [FieldOffset(130)]
        public int _NeutralMinionsKilledInEnemyJungle;
        [FieldOffset(134)]
        public int _NeutralMinionsKilledInTeamJungle;
        [FieldOffset(142)]
        public int _Deaths;
        [FieldOffset(146)]
        public int _PentaKills;
        [FieldOffset(150)]
        public float _PhysicalDamageDealt;
        [FieldOffset(154)]
        public float _PhysicalDamageDealtToChampions;
        [FieldOffset(158)]
        public float _PhysicalDamageTaken;
        [FieldOffset(166)]
        public int _QuadraKills;
        [FieldOffset(206)]
        public int _TeamId;
        [FieldOffset(226)]
        public float _TotalDamageDealt;
        [FieldOffset(230)]
        public float _TotalDamageDealtToChampions;
        [FieldOffset(234)]
        public float _TotalDamageTaken;
        [FieldOffset(238)]
        public int _TotalHeal;
        [FieldOffset(242)]
        public float _TotalTimeCrowdControlDealt;
        [FieldOffset(246)]
        public float _TotalTimeSpentDead;
        [FieldOffset(250)]
        public int _TotalUnitsHealed;
        [FieldOffset(254)]
        public int _TripleKills;
        [FieldOffset(258)]
        public float _TrueDamageDealt;
        [FieldOffset(262)]
        public float _TrueDamageDealtToChampions;
        [FieldOffset(266)]
        public float _TrueDamageTaken;
        [FieldOffset(270)]
        public int _TurretsKilled;
        [FieldOffset(274)]
        public int _BarracksKilled;
        [FieldOffset(282)]
        public int _WardsKilled;
        [FieldOffset(286)]
        public int _WardsPlaced;
        [FieldOffset(298)]
        // sort of length (when above 0 sends malformed buffer error)
        public short Padding;

        public int Assists { get => _Assists;
            set => _Assists = value;
        }
        public int Kills { get => _Kills;
            set => _Kills = value;
        }
        public int DoubleKills { get => _DoubleKills;
            set => _DoubleKills = value;
        }
        public int UnrealKills { get => _UnrealKills;
            set => _UnrealKills = value;
        }
        public float GoldEarned { get => _GoldEarned;
            set => _GoldEarned = value;
        }
        public float GoldSpent { get => _GoldSpent;
            set => _GoldSpent = value;
        }
        public int CurrentKillingSpree { get => _CurrentKillingSpree;
            set => _CurrentKillingSpree = value;
        }
        public float LargestCriticalStrike { get => _LargestCriticalStrike;
            set => _LargestCriticalStrike = value;
        }
        public int LargestKillingSpree { get => _LargestKillingSpree;
            set => _LargestKillingSpree = value;
        }
        public int LargestMultiKill { get => _LargestMultiKill;
            set => _LargestMultiKill = value;
        }
        public float LongestTimeSpentLiving { get => _LongestTimeSpentLiving;
            set => _LongestTimeSpentLiving = value;
        }
        public float MagicDamageDealt { get => _MagicDamageDealt;
            set => _MagicDamageDealt = value;
        }
        public float MagicDamageDealtToChampions { get => _MagicDamageDealtToChampions;
            set => _MagicDamageDealtToChampions = value;
        }
        public float MagicDamageTaken { get => _MagicDamageTaken;
            set => _MagicDamageTaken = value;
        }
        public int MinionsKilled { get => _MinionsKilled;
            set => _MinionsKilled = value;
        }
        public int NeutralMinionsKilled { get => _NeutralMinionsKilled;
            set => _NeutralMinionsKilled = value;
        }
        public int NeutralMinionsKilledInEnemyJungle { get => _NeutralMinionsKilledInEnemyJungle;
            set => _NeutralMinionsKilledInEnemyJungle = value;
        }
        public int NeutralMinionsKilledInTeamJungle { get => _NeutralMinionsKilledInTeamJungle;
            set => _NeutralMinionsKilledInTeamJungle = value;
        }
        public int Deaths { get => _Deaths;
            set => _Deaths = value;
        }
        public int PentaKills { get => _PentaKills;
            set => _PentaKills = value;
        }
        public float PhysicalDamageDealt { get => _PhysicalDamageDealt;
            set => _PhysicalDamageDealt = value;
        }
        public float PhysicalDamageDealtToChampions { get => _PhysicalDamageDealtToChampions;
            set => _PhysicalDamageDealtToChampions = value;
        }
        public float PhysicalDamageTaken { get => _PhysicalDamageTaken;
            set => _PhysicalDamageTaken = value;
        }
        public int QuadraKills { get => _QuadraKills;
            set => _QuadraKills = value;
        }
        public int TeamId { get => _TeamId;
            set => _TeamId = value;
        }
        public float TotalDamageDealt { get => _TotalDamageDealt;
            set => _TotalDamageDealt = value;
        }
        public float TotalDamageDealtToChampions { get => _TotalDamageDealtToChampions;
            set => _TotalDamageDealtToChampions = value;
        }
        public float TotalDamageTaken { get => _TotalDamageTaken;
            set => _TotalDamageTaken = value;
        }
        public int TotalHeal { get => _TotalHeal;
            set => _TotalHeal = value;
        }
        public float TotalTimeCrowdControlDealt { get => _TotalTimeCrowdControlDealt;
            set => _TotalTimeCrowdControlDealt = value;
        }
        public float TotalTimeSpentDead { get => _TotalTimeSpentDead;
            set => _TotalTimeSpentDead = value;
        }
        public int TotalUnitsHealed { get => _TotalUnitsHealed;
            set => _TotalUnitsHealed = value;
        }
        public int TripleKills { get => _TripleKills;
            set => _TripleKills = value;
        }
        public float TrueDamageDealt { get => _TrueDamageDealt;
            set => _TrueDamageDealt = value;
        }
        public float TrueDamageDealtToChampions { get => _TrueDamageDealtToChampions;
            set => _TrueDamageDealtToChampions = value;
        }
        public float TrueDamageTaken { get => _TrueDamageTaken;
            set => _TrueDamageTaken = value;
        }
        public int TurretsKilled { get => _TurretsKilled;
            set => _TurretsKilled = value;
        }
        public int BarracksKilled { get => _BarracksKilled;
            set => _BarracksKilled = value;
        }
        public int WardsKilled { get => _WardsKilled;
            set => _WardsKilled = value;
        }
        public int WardsPlaced { get => _WardsPlaced;
            set => _WardsPlaced = value;
        }

        // TODO: move to universal serializer
        // also code here is unsafe, but thats prefered than just
        // write a function that simply dumps all the variables
        public static byte[] GetBytes(IChampionStats stats)
        {
            int size = Marshal.SizeOf(stats);
            byte[] arr = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(stats, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);

            return arr;
        }

        public byte[] GetBytes()
        {
            return GetBytes(this);
        }
    }
}
