using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalEngine
{
    public static class InputListener
    {
        private static readonly Queue<InputAction> mInputQueue;
        public static int Actions => mInputQueue.Count;

        internal static void Accept(InputAction action)
        {
            mInputQueue.Enqueue(action);
        }

        internal static void Update()
        {
            InputStatus.Update(mInputQueue);
            Gui.Input(mInputQueue);
        }

        static InputListener()
        {
            mInputQueue = new Queue<InputAction>();
        }

        public static InputAction Get(bool remove = false)
        {
            if (remove) return mInputQueue.Dequeue();

            return mInputQueue.Peek();
        }

        public static void Clear()
        {
            mInputQueue.Clear();
        }

        public static void Execute(InputSolver solver)
        {
            //execute input solver without remove input actions
            foreach (var inputAction in mInputQueue)
            { 
                solver.Execute(inputAction);
            }
        }

        public static void Execute(InputCombine solver) => Execute(solver.Peek());
    }
}