using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalEngine
{
    struct GlobalGuiElementStatus
    {
        public GuiElement DragElement;
        public GuiElement FocusElement;
        public Point2f Position;
    }

    public static class Gui
    {
        private static GuiRender mRender;

        internal static GlobalGuiElementStatus GlobalElementStatus;

        public static Dictionary<string, GuiGroup> Groups { get; }

        public static Image Canvas { get; private set; }

        public static InputMapped InputMapped { get; }

        public static bool AutoSize { get; set; }

        public static Point2f Position => GlobalElementStatus.Position;

        internal static void Initialize()
        {
            mRender = new GuiRender(GameSystems.GpuDevice);

            ///auto size, if true means when window size is changed, the canvas size is also changed.
            ///false means we do not change the size when window size changed.
            AutoSize = true;

            ///resize the canvas size
            ReSizeCanvas(GameSystems.EngineWindow.Size);

            GameSystems.EngineWindow.OnSizeChangeEvent += (sender, eventArg) =>
            {
                if (AutoSize == false) return;

                ReSizeCanvas(eventArg.After);
            };

            ///add input mapped
            ///axis input mapped
            InputMapped.Mapped(InputProperty.MouseX, GuiProperty.InputMoveX);
            InputMapped.Mapped(InputProperty.MouseY, GuiProperty.InputMoveY);
            InputMapped.Mapped(InputProperty.MouseWheel, GuiProperty.InputWheel);
            ///button input mapped
            InputMapped.Mapped(InputProperty.LeftButton, GuiProperty.InputClick);
            ///mapped the keycode to input key
            foreach (var keycode in InputProperty.KeyCodeMapped)
                InputMapped.Mapped(keycode.Value, GuiProperty.InputKey);

            LogEmitter.Apply(LogLevel.Information, "[Initialize Gui System Finish] from [Gui]");
        }

        internal static void Draw()
        {
            mRender.BeginDraw(Canvas);
            mRender.Clear(Canvas, new Colorf(1, 1, 1, 1));

            foreach (var group in Groups) group.Value.Draw(mRender);

            mRender.EndDraw();
        }

        internal static void Update(float delta)
        {
            foreach (var group in Groups) group.Value.Update(delta);
        }

        internal static void Input(Queue<InputAction> actions)
        {
            InputSolver tempSolver = new InputSolver();
            tempSolver.AxisInputAction.Add(GuiProperty.InputMoveX, new AxisInputActionSolvers());
            tempSolver.AxisInputAction.Add(GuiProperty.InputMoveY, new AxisInputActionSolvers());

            tempSolver.AxisInputAction[GuiProperty.InputMoveX].Solvers.Add((action) =>
                GlobalElementStatus.Position.X += action.Offset * Canvas.Size.Width
            );

            tempSolver.AxisInputAction[GuiProperty.InputMoveY].Solvers.Add((action) =>
                GlobalElementStatus.Position.Y += action.Offset * Canvas.Size.Height
            );

            tempSolver.InputMappeds.Add(InputMapped);

            foreach (var action in actions)
            {
                tempSolver.Execute(action);

                foreach (var group in Groups)
                {
                    group.Value.Input(action);
                }
            }
        }

        internal static void Present(PresentRender render)
        {
            render.Mask(Canvas, new Rectangle(
                left: 0, top: 0,
                right: GameSystems.EngineWindow.Size.Width,
                bottom: GameSystems.EngineWindow.Size.Height));
        }

        static Gui()
        {
            Groups = new Dictionary<string, GuiGroup>();
            InputMapped = new InputMapped();
        }

        public static void Add(GuiGroup group) => Groups.Add(group.Name, group);
        public static void Remove(string name) => Groups.Remove(name);
        public static void Remove(GuiGroup group) => Groups.Remove(group.Name);

        public static void ReSizeCanvas(Size size)
        {
            //when size is not changed or size is 0,
            //we do not change the size of gui
            if (Canvas?.Size == size || size.Empty == true) return;

            //update the position and dispose the canvas
            if (Canvas != null)
            {
                //update the position, because the canvas size is changed
                GlobalElementStatus.Position = new Point2f(
                    GlobalElementStatus.Position.X / Canvas.Size.Width * size.Width,
                    GlobalElementStatus.Position.Y / Canvas.Size.Height * size.Height);

                Canvas.Dispose();
            }

            Canvas = new Image(size, PixelFormat.RedBlueGreenAlpha8bit);
        }
    }
}
