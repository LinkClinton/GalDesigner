using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalEngine
{
    public class RowText : IDisposable
    {
        private readonly Font mFont;
        private readonly string mContent;
        private readonly List<int> mCharacterPostLocation;

        private Image mImage;

        public Font Font => mFont;
        public string Content => mContent;
        public Image Image => mImage;
        public Size Size => mImage.Size;

        private static Size GenRequirementSize(string content, Font font)
        {
            //see more in freetype
            var sizeMetrices = font.FontFace.Size.Metrics;
            var baseLineDistance = sizeMetrices.Height.Value >> 6;

            //pen position, in other words, the position of current character
            var penPositionX = 0;
            var penPositionY = baseLineDistance;

            var requirementWidth = 0;
            var requirementHeight = 0;

            //internal function: update requirement size(width and height)
            void internalfunctionUpdateSize(int width, int height)
            {
                //when we has gone to next line in this time, the "penPositionX" maybe less than requirement width
                //but at the last line, "penPositionX" maybe more than requirement width
                //so we need to use "max" function to update requirement width
                requirementWidth = Math.Max(requirementWidth, width);
                requirementHeight = Math.Max(requirementHeight, height);
            }

            foreach (var character in content)
            {
                var codeMetrics = font.GetCharacterCodeMetrics(character);

                //accept an new chacacter, we need to update the size
                internalfunctionUpdateSize(penPositionX + codeMetrics.Advance, penPositionY);

                //go to next character in the same line
                penPositionX = penPositionX + codeMetrics.Advance;
            }

            return new Size(requirementWidth, requirementHeight);
        }

        private static void GenTextureFromContent(string content, Font font, Image texture, ref List<int> characterPostLocation)
        {
            //clear character post location
            characterPostLocation.Clear();

            //see more in freetype
            var sizeMetrics = font.FontFace.Size.Metrics;
            var maxCharacterHeight = sizeMetrics.Ascender - sizeMetrics.Descender;
            var baseLine = ((sizeMetrics.Height - maxCharacterHeight) / 2 + sizeMetrics.Ascender).Value >> 6;

            var penPositionX = 0;
            var penPositionY = baseLine;

            foreach (var character in content)
            {
                var codeMetrics = font.GetCharacterCodeMetrics(character);

                //it is possible that the character texture may out of the text texture
                //we need to clip the out part(only copy the in part)
                var sourceTextureWidth = codeMetrics.Texture.Size.Width;
                var sourceTextureHeight = Math.Min(
                    codeMetrics.Texture.Size.Height,
                    codeMetrics.HoriBearingY + texture.Size.Height - penPositionY);

                //no part of character texture are in the text texture
                //we can finish our work
                if (sourceTextureHeight < 0) return;

                //copy character texture to text texture 
                //the text texture is the texture we will display
                texture.CopyFromImage(new Point2(
                    penPositionX + codeMetrics.HoriBearingX,
                    penPositionY - codeMetrics.HoriBearingY),
                    codeMetrics.Texture,
                    new Rectangle(0, 0, sourceTextureWidth, sourceTextureHeight));

                //next character
                penPositionX = penPositionX + codeMetrics.Advance;

                //set the post location for current characater
                characterPostLocation.Add(penPositionX);
            }
        }

        public RowText(string content, Font font)
        {
            mFont = font;
            mContent = content;

            mImage = new Image(GenRequirementSize(mContent, mFont), PixelFormat.Alpha8bit);
            mCharacterPostLocation = new List<int>();

            GenTextureFromContent(mContent, mFont, mImage, ref mCharacterPostLocation);
        }

        ~RowText() => Dispose();

        public int GetCharacterPostLocation(int location)
        {
            return mCharacterPostLocation[location];
        }

        public SegmentRange<int> GetCharactersLocationRange(SegmentRange<int> locationRange)
        {
            return new SegmentRange<int>(
                locationRange.Start == 0 ? 0 : 
                mCharacterPostLocation[locationRange.Start - 1],
                mCharacterPostLocation[locationRange.End]);
        }

        public void Dispose()
        {
            Utility.Dispose(ref mImage);
        }
    }
}
