﻿using GameServerCore.Content;
using System.Numerics;

namespace GameServerCore
{
    /// <summary>
    /// Class containing coordinate conversions usually used for packets.
    /// Most coordinates understandable by the League clients are oriented around an origin which is at the center of the map,
    /// however, LeagueSandbox has its origin at the bottom left corner of the map (center of blue fountain), so a conversion is needed.
    /// </summary>
    public class MovementVector
    {
        public short X;
        public short Y;

        public MovementVector(short x, short y)
        {
            X = x;
            Y = y;
        }

        public MovementVector(IGame game, float x, float y)
        {
            X = FormatCoordinate(x, game.Map.NavigationGrid.MiddleOfMap.Y);
            Y = FormatCoordinate(y, game.Map.NavigationGrid.MiddleOfMap.X);
        }

        /// <summary>
        /// Converts the given coordinate by changing its origin to the given origin.
        /// </summary>
        /// <param name="coordinate">Coordinate to convert.</param>
        /// <param name="origin">New origin for the coordinate.</param>
        /// <returns>Converted coordinate.</returns>
        public static short FormatCoordinate(float coordinate, float origin)
        {
            return (short)((coordinate - origin) / 2f);
        }

        /// <summary>
        /// Converts the given X coordinate to one which originates at the center of the map.
        /// </summary>
        /// <param name="navGrid">NavGrid of the current map.</param>
        /// <param name="value">Coordinate to convert.</param>
        /// <returns>Converted coordinate.</returns>
        public static short TargetXToNormalFormat(INavigationGrid navGrid, float value)
        {
            return FormatCoordinate(value, navGrid.MiddleOfMap.X);
        }

        /// <summary>
        /// Converts the given X coordinate to one which originates at the center of the map.
        /// </summary>
        /// <param name="game">Current Game instance.</param>
        /// <param name="value">Coordinate to convert.</param>
        /// <returns>Converted coordinate.</returns>
        public static short TargetXToNormalFormat(IGame game, float value)
        {
            return TargetXToNormalFormat(game.Map.NavigationGrid, value);
        }

        /// <summary>
        /// Converts the given Y coordinate to one which originates at the center of the map.
        /// </summary>
        /// <param name="navGrid">NavGrid of the current map.</param>
        /// <param name="value">Coordinate to convert.</param>
        /// <returns>Converted coordinate.</returns>
        public static short TargetYToNormalFormat(INavigationGrid navGrid, float value)
        {
            return FormatCoordinate(value, navGrid.MiddleOfMap.Y);
        }

        /// <summary>
        /// Converts the given Y coordinate to one which originates at the center of the map.
        /// </summary>
        /// <param name="game">Current Game instance.</param>
        /// <param name="value">Coordinate to convert.</param>
        /// <returns>Converted coordinate.</returns>
        public static short TargetYToNormalFormat(IGame game, float value)
        {
            return TargetYToNormalFormat(game.Map.NavigationGrid, value);
        }
    }
}
