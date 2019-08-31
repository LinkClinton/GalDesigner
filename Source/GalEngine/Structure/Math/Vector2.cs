using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalEngine
{
    public struct Vector2
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Vector2(int x = 0, int y = 0)
        {
            X = x;
            Y = y;
        }
    }

    public struct Vector2f
    {
        public float X { get; set; }
        public float Y { get; set; }

        public Vector2f(float x = 0, float y = 0)
        {
            X = x;
            Y = y;
        }
    }
}
