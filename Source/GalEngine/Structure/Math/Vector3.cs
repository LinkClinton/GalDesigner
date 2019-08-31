using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalEngine
{
    public struct Vector3
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public Vector3(int x = 0, int y = 0, int z = 0)
        {
            X = x; Y = y;Z = z;
        }
    }

    public struct Vector3f
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Vector3f(float x = 0, float y = 0, float z = 0)
        {
            X = x; Y = y; Z = z;
        }
    }
}
