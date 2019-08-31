using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GalEngine
{
    public static class Utility
    {
        public static void Dispose<T>(ref T Object) where T : class, IDisposable
        {
            if (Object == null) return;

            Object.Dispose();
            Object = null;
        }

        public static int SizeOf<T>()
        {
            return Marshal.SizeOf<T>();
        }

        public static T Max<T>(T left, T right) where T : IComparable<T>
        {
            if (left.CompareTo(right) < 0) return right;
            return left;
        }

        public static T Min<T>(T left, T right) where T : IComparable<T>
        {
            if (left.CompareTo(right) > 0) return right;
            return left;
        }
    
        public static T Clamp<T>(T value, T min, T max) where T : IComparable<T>
        {
            return Min(Max(value, min), max);
        }

        public static float Center(float max, float min)
        {
            return (max - min) * 0.5f;
        }

        public static void Assert(bool condition)
        {
#if DEBUG
            if (!condition) throw new Exception();
#endif
        }

        public static void Assert(bool condition, Exception exception)
        {
#if DEBUG
            if (!condition) throw exception;
#endif
        }
    }
}
