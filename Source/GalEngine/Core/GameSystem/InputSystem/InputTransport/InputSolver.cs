using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalEngine
{
    
    public class InputSolver
    {
        public Dictionary<string, ButtonInputActionSolvers> ButtonInputAction { get; }
        public Dictionary<string, AxisInputActionSolvers> AxisInputAction { get; }
        public Dictionary<string, CharInputActionSolvers> CharInputAction { get; }

        public List<InputMapped> InputMappeds { get; }

        public InputSolver()
        {
            ButtonInputAction = new Dictionary<string, ButtonInputActionSolvers>();
            AxisInputAction = new Dictionary<string, AxisInputActionSolvers>();
            CharInputAction = new Dictionary<string, CharInputActionSolvers>();
            InputMappeds = new List<InputMapped>();
        }

        public InputSolver(InputSolver other) : base() =>
            Override(other);

        public void Override(InputSolver other)
        {
            foreach (var action in other.ButtonInputAction)
                ButtonInputAction[action.Key] = action.Value;
            foreach (var action in other.AxisInputAction)
                AxisInputAction[action.Key] = action.Value;
            foreach (var action in other.CharInputAction)
                CharInputAction[action.Key] = action.Value;

            InputMappeds.InsertRange(0, other.InputMappeds);
        }

        public string MappedInput(string input)
        {
            foreach (var inputMapped in InputMappeds)
            {
                if (inputMapped.IsMapped(input)) return inputMapped.MappedInput(input);
            }
            
            return input;
        }

        public void Execute(InputAction action)
        {
            var alias = MappedInput(action.Name);

            switch (action)
            {            
                case ButtonInputAction button:
                    if (ButtonInputAction.ContainsKey(alias)) 
                        ButtonInputAction[alias]?.ForEach(button); break;
                case AxisInputAction axis:
                    if (AxisInputAction.ContainsKey(alias))
                        AxisInputAction[MappedInput(axis.Name)]?.ForEach(axis); break;
                case CharInputAction charInput:
                    if (CharInputAction.ContainsKey(alias))
                        CharInputAction[MappedInput(charInput.Name)]?.ForEach(charInput); break;
                default: throw new Exception("Invalid Input Action.");
            }
        }
    }
}
