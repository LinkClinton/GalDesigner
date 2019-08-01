using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalEngine
{
    public struct Size
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public bool Empty => Width == 0 || Height == 0;

        public Size(int width = 0, int height = 0)
        {
            Width = width;
            Height = height;
        }

        public static bool operator ==(Size left, Size right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Size left, Size right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            return obj is Size size &&
                   EqualityComparer<int>.Default.Equals(Width, size.Width) &&
                   EqualityComparer<int>.Default.Equals(Height, size.Height);
        }

        public override int GetHashCode()
        {
            var hashCode = 859600377;
            hashCode = hashCode * -1521134295 + EqualityComparer<int>.Default.GetHashCode(Width);
            hashCode = hashCode * -1521134295 + EqualityComparer<int>.Default.GetHashCode(Height);
            return hashCode;
        }

        public override string ToString()
        {
            return "Width = " + Width + ", Height = " + Height;
        }
    }

    public struct Sizef
    {
        public float Width { get; set; }
        public float Height { get; set; }

        public Sizef(float width = 0, float height = 0)
        {
            Width = width;
            Height = height;
        }

        public static bool operator == (Sizef left, Sizef right)
        {
            return left.Equals(right);
        }

        public static bool operator != (Sizef left, Sizef right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            return obj is Size size &&
                   EqualityComparer<float>.Default.Equals(Width, size.Width) &&
                   EqualityComparer<float>.Default.Equals(Height, size.Height);
        }

        public override int GetHashCode()
        {
            var hashCode = 859600377;
            hashCode = hashCode * -1521134295 + EqualityComparer<float>.Default.GetHashCode(Width);
            hashCode = hashCode * -1521134295 + EqualityComparer<float>.Default.GetHashCode(Height);
            return hashCode;
        }
    }
}
