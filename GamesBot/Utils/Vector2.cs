using SixLabors.ImageSharp;

namespace GamesBot
{
    public class Vector2
    {
        public Vector2()
        {
            x = 0f;
            y = 0f;
        }

        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public float x;
        public float y;

        public Point ToPoint() => new((int)x, (int)y);
        public PointF ToPointF() => new(x, y);

        public static readonly Vector2 zero = new();
        public static readonly Vector2 one = new(1, 1);
    }
}
