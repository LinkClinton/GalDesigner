using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalEngine
{
    public delegate void CharInputActionSolver(CharInputAction action);

    public class CharInputAction : InputAction
    {
        public CharInputAction(string name) : base(name, InputType.Char)
        {

        }
    }

    public class CharInputActionSolvers
    {
        public List<CharInputActionSolver> Solvers { get; }

        public CharInputActionSolvers()
        {
            Solvers = new List<CharInputActionSolver>();
        }

        public void ForEach(CharInputAction action)
        {
            Solvers.ForEach((solver) => solver.Invoke(action));
        }
    }
}
