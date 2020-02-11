using GameServerCore.Content;

namespace GameServerCore
{
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

        public static short FormatCoordinate(float coordinate, float origin)
        {
            return (short)((coordinate - origin) / 2f);
        }

        public static short TargetXToNormalFormat(IGame game, float value)
        {
            return TargetXToNormalFormat(game.Map.NavigationGrid, value);
        }

        public static short TargetYToNormalFormat(IGame game, float value)
        {
            return TargetYToNormalFormat(game.Map.NavigationGrid, value);
        }

        public static short TargetXToNormalFormat(INavigationGrid navGrid, float value)
        {
            return FormatCoordinate(value, navGrid.MiddleOfMap.X);
        }

        public static short TargetYToNormalFormat(INavigationGrid navGrid, float value)
        {
            return FormatCoordinate(value, navGrid.MiddleOfMap.Y);
        }
    }
}
