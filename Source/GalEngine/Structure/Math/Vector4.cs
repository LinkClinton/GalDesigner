using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalEngine
{
    public struct Vector4
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public int W { get; set; }

        public Vector4(int x = 0, int y = 0, int z = 0, int w = 0)
        {
            X = x; Y = y; Z = z; W = w;
        }
    }

    public struct Vector4f
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float W { get; set; }

        public Vector4f(float x = 0, float y = 0, float z = 0, float w = 0)
        {
            X = x; Y = y; Z = z; W = w;
        }
    }
}
