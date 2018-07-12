namespace LeagueSandbox.GameServer.Logic.GameObjects.Other
{
    public class MovementVector
    {
        public short X;
        public short Y;

        public MovementVector() { }

        public MovementVector(short x, short y)
        {
            X = x;
            Y = y;
        }

        public MovementVector(float x, float y)
        {
            X = FormatCoordinate(x, Game.Map.NavGrid.MiddleOfMap.Y);
            Y = FormatCoordinate(y, Game.Map.NavGrid.MiddleOfMap.X);
        }

        public Target ToTarget()
        {
            return new Target(2.0f * X + Game.Map.NavGrid.MiddleOfMap.X, 2.0f * Y + Game.Map.NavGrid.MiddleOfMap.Y);
        }

        public static short FormatCoordinate(float coordinate, float origin)
        {
            return (short)((coordinate - origin) / 2f);
        }

        public static short TargetXToNormalFormat(float value)
        {
            return FormatCoordinate(value, Game.Map.NavGrid.MiddleOfMap.X);
        }

        public static short TargetYToNormalFormat(float value)
        {
            return FormatCoordinate(value, Game.Map.NavGrid.MiddleOfMap.Y);
        }
    }
}
