using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalEngine
{ 
    public class InputCombine
    {
        private readonly Stack<InputSolver> mInputSolvers;

        public InputCombine()
        {
            mInputSolvers = new Stack<InputSolver>();
        }

        public int Push(InputSolver solver)
        {
            var newSolver = mInputSolvers.Count != 0 ? new InputSolver(mInputSolvers.Peek()) : new InputSolver();

            newSolver.Override(solver);

            mInputSolvers.Push(newSolver);

            return mInputSolvers.Count - 1;
        }

        public InputSolver Pop()
            => mInputSolvers.Pop();

        public InputSolver Peek()
            => mInputSolvers.Peek();
    }
}
