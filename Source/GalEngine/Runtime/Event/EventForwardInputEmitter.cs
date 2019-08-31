using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalEngine
{
    class EventForwardInputEmitter : InputEmitter
    {
        private Point2 mMousePosition;
        private EngineWindow mWindow;

        public EventForwardInputEmitter(EngineWindow window)
        {
            mWindow = window;
        }

        public void Forward(MouseMoveEvent eventArg)
        {
            float xOffset = (float)(eventArg.Position.X - mMousePosition.X) / mWindow.Size.Width;
            float yOffset = (float)(eventArg.Position.Y - mMousePosition.Y) / mWindow.Size.Height;
            
            mMousePosition = eventArg.Position;

            Forward(new AxisInputAction(InputProperty.MouseX, xOffset));
            Forward(new AxisInputAction(InputProperty.MouseY, yOffset));
        }

        public void Forward(MouseClickEvent eventArg)
        {
            var buttonName = "";

            switch (eventArg.Button)
            {
                case MouseButton.Left: buttonName = InputProperty.LeftButton; break;
                case MouseButton.Middle: buttonName = InputProperty.MiddleButton; break;
                case MouseButton.Right: buttonName = InputProperty.RightButton; break;
                default: throw new Exception("Invalid Mouse Button.");
            }

            Forward(new ButtonInputAction(buttonName, eventArg.IsDown));
        }

        public void Forward(MouseWheelEvent eventArg)
        {
            //forward mouse wheel event
            //notice: the range of offset is from -inf to inf
            Forward(new AxisInputAction(InputProperty.MouseWheel, eventArg.Offset / 120.0f));
        }

        public void Forward(KeyBoardEvent eventArg)
        {
            Forward(new ButtonInputAction(InputProperty.KeyCodeMapped[eventArg.KeyCode], eventArg.IsDown));
        }

        public void Forward(CharEvent eventArg)
        {
            Forward(new CharInputAction(eventArg.Char.ToString()));
        }

        public void Forward(BaseEvent eventArg)
        {
            switch (eventArg)
            {
                case MouseClickEvent mouseClick: Forward(mouseClick); break;
                case MouseWheelEvent mouseWheel: Forward(mouseWheel); break;
                case MouseMoveEvent mouseMove: Forward(mouseMove); break;
                case KeyBoardEvent keyBoard: Forward(keyBoard); break;
                case CharEvent charInput: Forward(charInput); break;
                default: break;
            }
        }
    }
}
