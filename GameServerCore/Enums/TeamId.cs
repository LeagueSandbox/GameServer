using System;
using System.Runtime.CompilerServices;

namespace GameServerCore.Enums
{
    public enum TeamId: ushort
    {
        TEAM_BLUE = 0x64, //100
        TEAM_PURPLE = 0xC8, //200
        TEAM_NEUTRAL = 0x12C //300
    }
/*
    001100100 ^ 1101100 = 000001000
    011001000 ^ 1101100 = 010100100
    100101100 ^ 1101100 = 101000000
*/
    [Flags]
    public enum TeamIdFlags: ushort
    {
        NONE = 0,
        TEAM_BLUE = 100 ^ 108,
        TEAM_PURPLE = 200 ^ 108,
        TEAM_NEUTRAL = 300 ^ 108,
        TEAMS_BLUE_AND_PURPLE = (100 ^ 108) | (200 ^ 108),
        TEAMS_BLUE_AND_NEUTRAL = (100 ^ 108) | (300 ^ 108),
        TEAMS_PURPLE_AND_NEUTRAL = (200 ^ 108) | (300 ^ 108),
        TEAMS_ALL = (100 ^ 108) | (200 ^ 108) | (300 ^ 108)
    }
    public static class TeamIdFlagsExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasTeam(this TeamIdFlags teams, TeamId team)
        {
            TeamIdFlags t = (TeamIdFlags)((ushort)team ^ 108);
            return (teams & t) == t;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetTeam(ref this TeamIdFlags teams, TeamId team, bool value)
        {
            TeamIdFlags t = (TeamIdFlags)((ushort)team ^ 108);
            teams = value ? (teams | t) : (teams & ~t);
        }
    }
}
