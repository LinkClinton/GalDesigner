using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalEngine
{
    public struct Point2
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Point2(int x, int y)
        {
            X = x; Y = y;
        }
    }

    public struct Point2f
    {
        public float X { get; set; }
        public float Y { get; set; }

        public Point2f(float x, float y)
        {
            X = x; Y = y;
        }

        public static Point2f operator +(Point2f left, Vector2f right)
        {
            return new Point2f(
                left.X + right.X,
                left.Y + right.Y);
        }
    }
}
