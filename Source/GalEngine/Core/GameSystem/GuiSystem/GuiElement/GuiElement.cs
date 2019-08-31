using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalEngine
{
    public class GuiElement
    {
        public Transform Transform { get; set; }

        public bool Dragable { get; set; }

        public bool Readable { get; set; }

        protected internal virtual void Draw(GuiRender render) { }

        protected internal virtual void Input(InputAction action) { }

        protected internal virtual void Update(float delta) { }

        public GuiElement()
        {
            Transform = new Transform();

            Dragable = true;
            Readable = true;
        }

        public virtual bool Contain(Point2f point) { return false; }
    }
}
