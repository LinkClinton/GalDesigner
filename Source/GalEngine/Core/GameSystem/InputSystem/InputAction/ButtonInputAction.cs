using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalEngine
{
    public delegate void ButtonInputActionSolver(ButtonInputAction action);

    public class ButtonInputAction : InputAction
    {
        public bool Status { get; }

        public ButtonInputAction(string name, bool status) : base(name, InputType.Button)
        {
            Status = status;
        }
    }

    public class ButtonInputActionSolvers
    {
        public List<ButtonInputActionSolver> Solvers { get; }

        public ButtonInputActionSolvers()
        {
            Solvers = new List<ButtonInputActionSolver>();
        }

        public void ForEach(ButtonInputAction action)
        {
            Solvers.ForEach((solver) => solver.Invoke(action));
        }
    }
}
