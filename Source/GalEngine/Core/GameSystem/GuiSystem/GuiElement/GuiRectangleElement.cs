using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalEngine
{
    public abstract class GuiRectangleElement : GuiElement
    {
        public Size Size { get; set; }

        public override bool Contain(Point2f point)
        {
            var newPoint = Transform.Invert().TransformTo(point);

            if (newPoint.X < 0 || newPoint.X > Size.Width) return false;
            if (newPoint.Y < 0 || newPoint.Y > Size.Height) return false;

            return true;
        }
    }
}
