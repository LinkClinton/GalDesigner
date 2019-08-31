using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalEngine
{
    public class Text : IDisposable
    {
        private readonly Font mFont;
        private readonly Size mMaxSize;
        private readonly string mContent;

        private Image mImage;
        
        public Font Font => mFont;
        public Size MaxSize => mMaxSize;
        public string Content => mContent;
        public Image Image => mImage;
        public Size Size => mImage.Size;

        private static Size GenRequirementSize(string content, Font font, Size maxSize)
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
            
            //internal function: go to next line, when the width of "textContent" more than max width
            //internal function: we need to go to next line to layout the text we want to display
            void internalfunctionNextLine()
            {
                penPositionX = 0;
                penPositionY = penPositionY + baseLineDistance;
            }

            foreach (var character in content)
            {
                //if "textContent" contains '\n', we need to go next line
                if (character == '\n')
                {
                    internalfunctionNextLine();
                    internalfunctionUpdateSize(penPositionX, penPositionY);

                    continue;
                }

                var codeMetrics = font.GetCharacterCodeMetrics(character);

                //see more in "internalfunctionNextLine"
                if (penPositionX + codeMetrics.Advance >= maxSize.Width) internalfunctionNextLine();

                //accept an new chacacter, we need to update the size
                //internalfunctionUpdateSize(penPositionX + codeMetrics.Texture.Size.Width + codeMetrics.HoriBearingX, penPositionY);
                internalfunctionUpdateSize(penPositionX + codeMetrics.Advance, penPositionY);

                //go to next character in the same line
                penPositionX = penPositionX + codeMetrics.Advance;
            }

            //limit the requirement size
            requirementWidth = Math.Min(requirementWidth, maxSize.Width);
            requirementHeight = Math.Min(requirementHeight, maxSize.Height);

            return new Size(requirementWidth, requirementHeight);
        }

        private static void GenTextureFromContent(string content, Font font, Image texture)
        {
            //see more in freetype
            var sizeMetrics = font.FontFace.Size.Metrics;
            var maxCharacterHeight = sizeMetrics.Ascender - sizeMetrics.Descender;
            var baseLineDistance = sizeMetrics.Height.Value >> 6;
            var baseLine = ((sizeMetrics.Height - maxCharacterHeight) / 2 + sizeMetrics.Ascender).Value >> 6;

            var penPositionX = 0;
            var penPositionY = baseLine;

            //internal function: go to next line, when the width of "textContent" more than max width
            //internal function: we need to go to next line to layout the text we want to display
            void internalfunctionNextLine()
            {
                penPositionX = 0;
                penPositionY = penPositionY + baseLineDistance;
            }

            foreach (var character in content)
            {
                //if "textContent" contains '\n', we need to go next line
                if (character == '\n')
                {
                    internalfunctionNextLine();

                    continue;
                }

                var codeMetrics = font.GetCharacterCodeMetrics(character);

                //see more in "internalfunctionNextLine"
                if (penPositionX + codeMetrics.HoriBearingX + codeMetrics.Texture.Size.Width > texture.Size.Width)
                    internalfunctionNextLine();

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
            }
        }

        public Text(string content, Font font, Size maxSize)
        {
            mFont = font;
            mMaxSize = maxSize;
            mContent = content;

            maxSize = new Size(
                maxSize.Width == 0 ? int.MaxValue : maxSize.Width,
                maxSize.Height == 0 ? int.MaxValue : maxSize.Height);

            mImage = new Image(GenRequirementSize(mContent, mFont, maxSize), PixelFormat.Alpha8bit);

            GenTextureFromContent(mContent, mFont, mImage);
        }

        public Text(string ontent, Font font) : this(ontent, font, new Size(0, 0))
        {

        }

        public void Dispose()
        {
            Utility.Dispose(ref mImage);
        }
    }
}
