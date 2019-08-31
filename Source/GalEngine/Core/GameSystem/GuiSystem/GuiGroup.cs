using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalEngine
{
    public class GuiGroup : GuiElement
    {
        public List<GuiElement> Elements { get; }

        public InputMapped InputMapped { get; }

        public InputSolver InputSolver { get; }

        private void InitializeInputSolver()
        {
            void resolveInputMoveX(AxisInputAction input)
            {
                float offset = input.Offset * Gui.Canvas.Size.Width;

                if (Gui.GlobalElementStatus.DragElement != null &&
                    Gui.GlobalElementStatus.DragElement.Dragable == true)
                {
                    Gui.GlobalElementStatus.DragElement.Transform.
                        ApplyTransform(System.Numerics.Matrix4x4.CreateTranslation(offset, 0, 0));
                }

                Elements.ForEach((element) =>
                {
                    if (element.Contain(Gui.GlobalElementStatus.Position))
                        element.Input(new AxisInputAction(GuiProperty.InputMoveX, input.Offset));
                });
            }

            void resolveInputMoveY(AxisInputAction input)
            {
                float offset = input.Offset * Gui.Canvas.Size.Height;

                if (Gui.GlobalElementStatus.DragElement != null &&
                    Gui.GlobalElementStatus.DragElement.Dragable == true)
                {
                    Gui.GlobalElementStatus.DragElement.Transform.
                        ApplyTransform(System.Numerics.Matrix4x4.CreateTranslation(0, offset, 0));
                }

                Elements.ForEach((element) =>
                {
                    if (element.Contain(Gui.GlobalElementStatus.Position))
                        element.Input(new AxisInputAction(GuiProperty.InputMoveY, input.Offset));
                });
            }

            void resolveInputWheel(AxisInputAction input)
            {
                if (Gui.GlobalElementStatus.FocusElement != null)
                {
                    Gui.GlobalElementStatus.FocusElement.Input(
                        new AxisInputAction(GuiProperty.InputWheel, input.Offset));
                }
            }

            void resolveInputClick(ButtonInputAction input)
            {
                bool anyElementContain = false;

                GuiElement newElement = null;

                Elements.ForEach((element) =>
                {
                    //input click down, we need to get drag element, focus element ...
                    if (input.Status == true)
                    {
                        bool isContain = element.Contain(Gui.GlobalElementStatus.Position);

                        anyElementContain = anyElementContain | isContain;

                        //if we are draging gui element, we do not need to update the drag or focus element.
                        if (isContain == true && Gui.GlobalElementStatus.DragElement == null)
                        {
                            newElement = element;
                        }
                    }
                });

                if (newElement != null)
                {
                    Gui.GlobalElementStatus.DragElement = newElement;
                    Gui.GlobalElementStatus.FocusElement = newElement;

                    newElement.Input(new ButtonInputAction(GuiProperty.InputClick, input.Status));
                }

                ///update the drag and focus element.
                ///if the input click is down and there are no elements contain the cursor.
                ///we need to release the focus gui element.
                ///if the input click is up, we need to release the drag gui element.
                if (input.Status == true)
                {
                    if (anyElementContain == false)
                        Gui.GlobalElementStatus.FocusElement = null;
                }
                else
                {
                    Gui.GlobalElementStatus.DragElement?.Input(new ButtonInputAction(GuiProperty.InputClick, input.Status));
                    Gui.GlobalElementStatus.DragElement = null;
                }
            }

            void resolveInputText(CharInputAction input)
            {
                if (Gui.GlobalElementStatus.FocusElement != null &&
                    Gui.GlobalElementStatus.FocusElement.Readable == true)
                {
                    Gui.GlobalElementStatus.FocusElement.Input(input);
                }
            }

            void resolveInputKey(ButtonInputAction input)
            {
                if (Gui.GlobalElementStatus.FocusElement != null)
                {
                    Gui.GlobalElementStatus.FocusElement.Input(input);
                }
            }

            InputSolver.AxisInputAction.Add(GuiProperty.InputMoveX, new AxisInputActionSolvers());
            InputSolver.AxisInputAction.Add(GuiProperty.InputMoveY, new AxisInputActionSolvers());
            InputSolver.AxisInputAction.Add(GuiProperty.InputWheel, new AxisInputActionSolvers());

            InputSolver.AxisInputAction[GuiProperty.InputMoveX].Solvers.Add(resolveInputMoveX);
            InputSolver.AxisInputAction[GuiProperty.InputMoveY].Solvers.Add(resolveInputMoveY);
            InputSolver.AxisInputAction[GuiProperty.InputWheel].Solvers.Add(resolveInputWheel);

            InputSolver.ButtonInputAction.Add(GuiProperty.InputClick, new ButtonInputActionSolvers());
            InputSolver.ButtonInputAction.Add(GuiProperty.InputKey, new ButtonInputActionSolvers());

            InputSolver.CharInputAction.Add(GuiProperty.InputText, new CharInputActionSolvers());

            InputSolver.ButtonInputAction[GuiProperty.InputClick].Solvers.Add(resolveInputClick);
            InputSolver.ButtonInputAction[GuiProperty.InputKey].Solvers.Add(resolveInputKey);

            InputSolver.CharInputAction[GuiProperty.InputText].Solvers.Add(resolveInputText);

            InputSolver.InputMappeds.Add(InputMapped);
            InputSolver.InputMappeds.Add(Gui.InputMapped);
        }

        protected internal override void Draw(GuiRender render) 
            => Elements.ForEach((element) =>
            {
                render.PushTransform(element.Transform);

                element.Draw(render);

                render.PopTransform();
            });

        protected internal override void Update(float delta)
            => Elements.ForEach((element) => element.Update(delta));

        protected internal override void Input(InputAction action)
        {
            //for char input action, we need to skip the mapped test
            if (action.Type == InputType.Char)
                InputSolver.CharInputAction[GuiProperty.InputText].ForEach(action as CharInputAction);

            var alias = InputMapped.IsMapped(action.Name) ?
                InputMapped.MappedInput(action.Name) : Gui.InputMapped.MappedInput(action.Name);

            if (!GuiProperty.IsGuiInput(alias)) return;

            InputSolver.Execute(action);
        }

        public override bool Contain(Point2f point)
        {
            foreach (var element in Elements)
                if (element.Contain(point)) return true;
            return false;
        }

        public string Name { get; }

        public GuiGroup(string name)
        {
            Name = name;
            Elements = new List<GuiElement>();
            InputMapped = new InputMapped();
            InputSolver = new InputSolver();

            InitializeInputSolver();
        }
    }
}
