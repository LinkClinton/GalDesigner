using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalEngine
{
    public enum InputType
    {
        Button,
        Axis,
        Char
    }

    public class InputAction
    {
        public string Name { get; }
        public InputType Type { get; }

        public InputAction(string name, InputType type)
        {
            Name = name;
            Type = type;
        }
    }
}
