using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalEngine
{
    public struct Rectangle
    {
        public int Left { get; set; }
        public int Top { get; set; }
        public int Right { get; set; }
        public int Bottom { get; set; }

        public Rectangle(int left = 0, int top = 0, int right = 0, int bottom = 0)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }
    }

    public struct Rectanglef
    {
        public float Left { get; set; }
        public float Top { get; set; }
        public float Right { get; set; }
        public float Bottom { get; set; }

        public Rectanglef(float left = 0, float top = 0, float right = 0, float bottom = 0)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }
    }
}
