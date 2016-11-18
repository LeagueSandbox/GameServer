using LeagueSandbox.GameServer.Core.Logic;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class MovementVector
    {
        public short x;
        public short y;
        private static Game _game = Program.ResolveDependency<Game>();

        public MovementVector() { }

        public MovementVector(short x, short y)
        {
            this.x = x;
            this.y = y;
        }

        public MovementVector(float x, float y)
        {
            this.x = FormatCoordinate(x, _game.Map.GetHeight() / 2);
            this.y = FormatCoordinate(y, _game.Map.GetWidth() / 2);
        }

        public Target ToTarget()
        {
            return new Target(2.0f * x + _game.Map.GetWidth() / 2, 2.0f * y + _game.Map.GetHeight() / 2);
        }

        public static short FormatCoordinate(float coordinate, float origin)
        {
            return (short)((coordinate - origin) / 2f);
        }

        public static short TargetXToNormalFormat(float value)
        {
            return FormatCoordinate(value, _game.Map.GetWidth() / 2);
        }

        public static short TargetYToNormalFormat(float value)
        {
            return FormatCoordinate(value, _game.Map.GetHeight() / 2);
        }
    }
}
