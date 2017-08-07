using LeagueSandbox.GameServer.Core.Logic;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class MovementVector
    {
        public short x;
        public short y;
        private Game _game = Program.ResolveDependency<Game>();

        public MovementVector() { }

        public MovementVector(short x, short y)
        {
            this.x = x;
            this.y = y;
        }

        public MovementVector(float x, float y)
        {
            this.x = FormatCoordinate(x, _game.Map.AIMesh.getHeight() / 2);
            this.y = FormatCoordinate(y, _game.Map.AIMesh.getWidth() / 2);
        }

        public Target ToTarget()
        {
            return new Target(2.0f * x + _game.Map.AIMesh.getWidth() / 2, 2.0f * y + _game.Map.AIMesh.getHeight() / 2);
        }

        public static short FormatCoordinate(float coordinate, float origin)
        {
            return (short)((coordinate - origin) / 2f);
        }

        public static short TargetXToNormalFormat(float value)
        {
            var game = Program.ResolveDependency<Game>();
            return FormatCoordinate(value, game.Map.AIMesh.getWidth() / 2);
        }

        public static short TargetYToNormalFormat(float value)
        {
            var game = Program.ResolveDependency<Game>();
            return FormatCoordinate(value, game.Map.AIMesh.getHeight() / 2);
        }
    }
}
