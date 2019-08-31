using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalEngine
{
    public class MouseMoveEvent : BaseEvent
    {
        public Point2 Position { get; }

        public MouseMoveEvent(DateTime time, Point2 position) : base(time)
        {
            Position = position;
        }
    }

    public class MouseClickEvent : MouseMoveEvent
    {
        public bool IsDown { get; }
        public MouseButton Button { get; }

        public MouseClickEvent(DateTime time, Point2 position, MouseButton button,
            bool isDown) : base(time, position)
        {
            Button = button;
            IsDown = isDown;
        }
    }

    public class MouseWheelEvent : MouseMoveEvent
    {
        public int Offset { get; }

        public MouseWheelEvent(DateTime time, Point2 position, int offset) : base(time, position)
        {
            Offset = offset;
        }
    }

    public delegate void MouseMoveEventHandler(object sender, MouseMoveEvent eventArg);
    public delegate void MouseClickEventHandler(object sender, MouseClickEvent eventArg);
    public delegate void MouseWheelEventHandler(object sender, MouseWheelEvent eventArg);
}
