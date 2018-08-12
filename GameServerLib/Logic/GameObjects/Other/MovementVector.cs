using LeagueSandbox.GameServer.Logic.Content;

namespace LeagueSandbox.GameServer.Logic.GameObjects.Other
{
    public class MovementVector
    {
        public short X;
        public short Y;

        public MovementVector(Game game, short x, short y)
        {
            X = x;
            Y = y;
        }

        public MovementVector(Game game, float x, float y)
        {
            X = FormatCoordinate(x, game.Map.NavGrid.MiddleOfMap.Y);
            Y = FormatCoordinate(y, game.Map.NavGrid.MiddleOfMap.X);
        }

        public Target ToTarget(Game game)
        {
            return new Target(2.0f * X + game.Map.NavGrid.MiddleOfMap.X, 2.0f * Y + game.Map.NavGrid.MiddleOfMap.Y);
        }

        public static short FormatCoordinate(float coordinate, float origin)
        {
            return (short)((coordinate - origin) / 2f);
        }

        public static short TargetXToNormalFormat(Game game, float value)
        {
            return TargetXToNormalFormat(game.Map.NavGrid, value);
        }

        public static short TargetYToNormalFormat(Game game, float value)
        {
            return TargetYToNormalFormat(game.Map.NavGrid, value);
        }

        public static short TargetXToNormalFormat(NavGrid navGrid, float value)
        {
            return FormatCoordinate(value, navGrid.MiddleOfMap.X);
        }

        public static short TargetYToNormalFormat(NavGrid navGrid, float value)
        {
            return FormatCoordinate(value, navGrid.MiddleOfMap.Y);
        }
    }
}
