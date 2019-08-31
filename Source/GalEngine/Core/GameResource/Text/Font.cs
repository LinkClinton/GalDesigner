using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpFont;
using GalEngine.Runtime.Graphics;

namespace GalEngine
{
    public class CharacterCodeMetrics : IDisposable
    {
        private Image mTexture;

        public Image Texture { get => mTexture; internal set { mTexture = value; } }

        public int Advance { get; internal set; }
        public int HoriBearingX { get; internal set; }
        public int HoriBearingY { get; internal set; }

        ~CharacterCodeMetrics() => Dispose();

        public void Dispose()
        {
            Utility.Dispose(ref mTexture);
        }
    }

    public class Font : IDisposable
    {
        private Face mFace;

        private readonly FontClass mFontClass;
        private readonly Dictionary<char, CharacterCodeMetrics> mCharacterIndex;

        internal Face FontFace => mFace;
        
        public int Size { get; }

        public string ClassName { get; }

        internal Font(string className, Face face, int size, FontClass fontClass)
        {
            mFace = face;
            Size = size;
            ClassName = className;
            
            //set font size and encoding
            mFace.SetPixelSizes(0, (uint)size);
            mFace.SelectCharmap(SharpFont.Encoding.Unicode);

            mCharacterIndex = new Dictionary<char, CharacterCodeMetrics>();

            mFontClass = fontClass;
        }

        ~Font() => Dispose();

        public void FreeCache()
        {
            //dispose the CharacterCodeMetrics that cache in the font
            foreach (var characterCode in mCharacterIndex) characterCode.Value.Dispose();

            mCharacterIndex.Clear();
        }

        public CharacterCodeMetrics GetCharacterCodeMetrics(char character)
        {
            //search it in cache, if it is found return it
            //if we do not find, we create it and cache it
            if (mCharacterIndex.ContainsKey(character) == true) return mCharacterIndex[character];

            //find index(see more in freetype)
            var index = mFace.GetCharIndex(character);

            //nonsupport character, we return space
            if (index == 0)
            {
                index = mFace.GetCharIndex(' ');

                LogEmitter.Apply(LogLevel.Warning, "There are some nonsupport character used in [{0}] font.", ClassName);
            }

            //load glyph
            mFace.LoadGlyph(index, LoadFlags.Default, LoadTarget.Normal);

            //test the format of glyph, if it is not bitmap, we convert its format to bitmap
            if (mFace.Glyph.Format != GlyphFormat.Bitmap) mFace.Glyph.RenderGlyph(RenderMode.Normal);

            //create metrices
            var codeMetrics = new CharacterCodeMetrics();

            codeMetrics.Advance = (mFace.Glyph.Advance.X.Value >> 6);
            codeMetrics.HoriBearingX = (mFace.Glyph.Metrics.HorizontalBearingX.Value >> 6);
            codeMetrics.HoriBearingY = (mFace.Glyph.Metrics.HorizontalBearingY.Value >> 6);
            codeMetrics.Texture = mFace.Glyph.Bitmap.Buffer != IntPtr.Zero ? new Image(
                new Size(mFace.Glyph.Bitmap.Width, mFace.Glyph.Bitmap.Rows),
                PixelFormat.Alpha8bit, mFace.Glyph.Bitmap.BufferData) : 
                new Image(new Size(0,0), PixelFormat.Alpha8bit);

            mCharacterIndex.Add(character, codeMetrics);

            return codeMetrics;
        }

        public void Dispose()
        {
            //dispose the CharacterCodeMetrics that cache in the font
            FreeCache();

            //dispose the font class
            Utility.Dispose(ref mFace);

            //remove the reference in font class
            mFontClass.FreeCache(Size);
        }
    }
}
