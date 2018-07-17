using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.GameObjects.Stats
{
    [StructLayout(LayoutKind.Explicit)]
    public class ChampionStats
    {
        [FieldOffset(0)]
        public int Assists = 0;
        [FieldOffset(8)]
        public int Kills = 0;
        [FieldOffset(16)]
        public int DoubleKills = 0;
        [FieldOffset(32)]
        public int UnrealKills = 0;
        [FieldOffset(36)]
        public float GoldEarned = 0;
        [FieldOffset(40)]
        public float GoldSpent = 0;
        [FieldOffset(84)]
        public int CurrentKillingSpree = 0;
        [FieldOffset(88)]
        public float LargestCriticalStrike = 0;
        [FieldOffset(92)]
        public int LargestKillingSpree = 0;
        [FieldOffset(96)]
        public int LargestMultiKill = 0;
        [FieldOffset(104)]
        public float LongestTimeSpentLiving = 0;
        [FieldOffset(108)]
        public float MagicDamageDealt = 0;
        [FieldOffset(112)]
        public float MagicDamageDealtToChampions = 0;
        [FieldOffset(116)]
        public float MagicDamageTaken = 0;
        [FieldOffset(120)]
        public int MinionsKilled = 0;
        [FieldOffset(126)]
        public int NeutralMinionsKilled = 0;
        [FieldOffset(130)]
        public int NeutralMinionsKilledInEnemyJungle = 0;
        [FieldOffset(134)]
        public int NeutralMinionsKilledInTeamJungle = 0;
        [FieldOffset(142)]
        public int Deaths = 0;
        [FieldOffset(146)]
        public int PentaKills = 0;
        [FieldOffset(150)]
        public float PhysicalDamageDealt = 0;
        [FieldOffset(154)]
        public float PhysicalDamageDealtToChampions = 0;
        [FieldOffset(158)]
        public float PhysicalDamageTaken = 0;
        [FieldOffset(166)]
        public int QuadraKills = 0;
        [FieldOffset(206)]
        public int TeamId = 0;
        [FieldOffset(226)]
        public float TotalDamageDealt = 0;
        [FieldOffset(230)]
        public float TotalDamageDealtToChampions = 0;
        [FieldOffset(234)]
        public float TotalDamageTaken = 0;
        [FieldOffset(238)]
        public int TotalHeal = 0;
        [FieldOffset(242)]
        public float TotalTimeCrowdControlDealt = 0;
        [FieldOffset(246)]
        public float TotalTimeSpentDead = 0;
        [FieldOffset(250)]
        public int TotalUnitsHealed = 0;
        [FieldOffset(254)]
        public int TripleKills = 0;
        [FieldOffset(258)]
        public float TrueDamageDealt = 0;
        [FieldOffset(262)]
        public float TrueDamageDealtToChampions = 0;
        [FieldOffset(266)]
        public float TrueDamageTaken = 0;
        [FieldOffset(270)]
        public int TurretsKilled = 0;
        [FieldOffset(274)]
        public int BarracksKilled = 0;
        [FieldOffset(282)]
        public int WardsKilled = 0;
        [FieldOffset(286)]
        public int WardsPlaced = 0;
        [FieldOffset(298)]
        public short Padding = 0; // sort of length (when above 0 sends malformed buffer error)

        // TODO: move to universal serializer
        // also code here is unsafe, but thats prefered then write function that simply dumps all the variables
        public static byte[] GetBytes(ChampionStats stats)
        {
            int size = Marshal.SizeOf(stats);
            byte[] arr = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(stats, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);

            return arr;
        }

    }
}
