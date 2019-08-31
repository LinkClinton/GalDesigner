using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpFont;
using GalEngine.Runtime.Graphics;

namespace GalEngine
{
    public class FontClass : IDisposable
    {
        private static readonly Library mLibrary = new Library();
        private static readonly FontClass mUbuntuMono = new FontClass("UbuntuMono-R", Properties.Resources.UbuntuMono_R);

        private Dictionary<int, Font> mFonts;

        private readonly GpuDevice mGpuDevice;

        private readonly byte[] mFontData;

        public string Name { get; }

        public static FontClass UbuntuMono => mUbuntuMono;

        internal void FreeCache(int size) => mFonts.Remove(size);

        public FontClass(string name, byte[] data) :
            this(name, data, GameSystems.GpuDevice)
        {

        }

        public FontClass(string name, byte[] data, GpuDevice device)
        {
            Name = name;
            mFontData = data;
            mGpuDevice = device;

            mFonts = new Dictionary<int, Font>();
        }

        public Font Fonts(int size)
        {
            if (mFonts.ContainsKey(size) == false)
                mFonts.Add(size, new Font(Name, new Face(mLibrary, mFontData, 0), size, this));

            return mFonts[size];
        }

        public void FreeCache()
        {
            foreach (var font in mFonts) font.Value.Dispose();

            mFonts.Clear();
        }

        public void Dispose()
        {
            FreeCache();
        }
    }
}
