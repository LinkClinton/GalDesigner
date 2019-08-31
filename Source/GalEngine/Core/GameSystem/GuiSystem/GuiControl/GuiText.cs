using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalEngine
{
    public class GuiText : GuiRectangleElement
    {
        private Text mTextBuffer;

        public FontClass FontClass { get; set; }
        public Colorf Color { get; set; }
        public string Content{ get; set; }
        public int FontSize { get; set; }

        protected internal override void Update(float delta)
        {
            var font = FontClass.Fonts(FontSize);

            //do not need to change the text buffer, because we do not change any propertry
            if (mTextBuffer != null && mTextBuffer.Content == Content && mTextBuffer.Font == font) return;

            Utility.Dispose(ref mTextBuffer);

            mTextBuffer = new Text(Content, font, new Size());

            Size = mTextBuffer.Size;
        }

        protected internal override void Draw(GuiRender render)
        {
            render.DrawText(new Point2f(0, 0), mTextBuffer, Color);
        }

        public GuiText(string content, int fontSize) : 
            this(content, GuiFactory.FontClass, fontSize, new Colorf(0, 0, 0, 1))
        {

        }

        public GuiText(string content, FontClass fontClass, int fontSize, Colorf color)
        {
            Content = content;
            FontClass = fontClass;
            FontSize = fontSize;
            Color = color;

            Dragable = false;
            Readable = false;

            mTextBuffer = null;
        }
    }
}
