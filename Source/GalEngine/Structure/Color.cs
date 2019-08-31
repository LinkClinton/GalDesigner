using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalEngine
{
    public struct Colorf
    {
        public float Red { get; set; }
        public float Green { get; set; }
        public float Blue { get; set; }
        public float Alpha { get; set; }

        public Colorf(float red = 0, float green = 0, float blue = 0, float alpha = 1.0f)
        {
            Red = red; Green = green; Blue = blue; Alpha = alpha;
        }
    }
}
