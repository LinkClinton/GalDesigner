using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalEngine
{
    
    public delegate void AxisInputActionSolver(AxisInputAction action);

    public class AxisInputAction : InputAction
    {
        public float Offset { get; }

        public AxisInputAction(string name, float offset) : base(name, InputType.Axis)
        {
            Offset = offset;
        }
    }

    public class AxisInputActionSolvers
    {
        public List<AxisInputActionSolver> Solvers { get; }

        public AxisInputActionSolvers()
        {
            Solvers = new List<AxisInputActionSolver>();
        }

        public void ForEach(AxisInputAction action)
        {
            Solvers.ForEach((solver) => solver.Invoke(action));
        }
    }
}
