using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalEngine
{
    public class GuiButtonColorGroup
    {
        public Colorf Text { get; set; }
        public Colorf Background { get; set; }
    }

    public class GuiButtonStyle : GuiStyle
    {
        public GuiButtonColorGroup Color { get; }
        public GuiButtonColorGroup Cover { get; }

        public GuiButtonStyle()
        {
            Color = new GuiButtonColorGroup()
            {
                Background = new Colorf(0, 0, 0, 0),
                Text = new Colorf(0, 0, 0, 1)
            };

            Cover = new GuiButtonColorGroup()
            {
                Background = new Colorf(1f, 1f, 0, 0.1f),
                Text = new Colorf(0, 0, 0, 1)
            };
        }
    }

    public class GuiButton : GuiRectangleElement
    {
        private Text mTextBuffer;

        public FontClass FontClass { get; set; }
        public string Content { get; set; }
        public int FontSize { get; set; }

        public GuiButtonStyle Style { get; }

        protected internal override void Update(float delta)
        {
            var font = FontClass.Fonts(FontSize);

            //do not need to change the text buffer, because we do not change any propertry
            if (mTextBuffer != null && mTextBuffer.Content == Content && mTextBuffer.Font == font) return;

            Utility.Dispose(ref mTextBuffer);

            mTextBuffer = new Text(Content, font, new Size());
        }

        protected internal override void Draw(GuiRender render)
        {
            var colorGroup = Contain(Gui.Position) ? Style.Cover : Style.Color;
            var position = new Point2f(
                (Size.Width - mTextBuffer.Size.Width) * 0.5f,
                (Size.Height - mTextBuffer.Size.Height) * 0.5f);

            render.FillRectangle(
                new Rectanglef(0, 0, Size.Width, Size.Height),
                colorGroup.Background);
            render.DrawText(position, mTextBuffer, colorGroup.Text);
        }

        public GuiButton(string content, int fontSize, Size size) : 
            this(content, GuiFactory.FontClass, fontSize, size)
        {

        }

        public GuiButton(string content, FontClass fontClass, int fontSize, Size size)
        {
            Content = content;
            FontClass = fontClass;
            FontSize = fontSize;
            Size = size;

            Style = new GuiButtonStyle();

            Dragable = false;
            Readable = false;

            mTextBuffer = null;
        }
    }
}
