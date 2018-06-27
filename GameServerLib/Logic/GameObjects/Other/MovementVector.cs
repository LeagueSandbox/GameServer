using LeagueSandbox.GameServer.Core.Logic;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class MovementVector
    {
        public short X;
        public short Y;
        private static Game _game = Program.ResolveDependency<Game>();

        public MovementVector() { }

        public MovementVector(short x, short y)
        {
            X = x;
            Y = y;
        }

        public MovementVector(float x, float y)
        {
            X = FormatCoordinate(x, _game.Map.NavGrid.MiddleOfMap.Y);
            Y = FormatCoordinate(y, _game.Map.NavGrid.MiddleOfMap.X);
        }

        public Target ToTarget()
        {
            return new Target(2.0f * X + _game.Map.NavGrid.MiddleOfMap.X, 2.0f * Y + _game.Map.NavGrid.MiddleOfMap.Y);
        }

        public static short FormatCoordinate(float coordinate, float origin)
        {
            return (short)((coordinate - origin) / 2f);
        }

        public static short TargetXToNormalFormat(float value)
        {
            return FormatCoordinate(value, _game.Map.NavGrid.MiddleOfMap.X);
        }

        public static short TargetYToNormalFormat(float value)
        {
            return FormatCoordinate(value, _game.Map.NavGrid.MiddleOfMap.Y);
        }
    }
}
