using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalEngine
{
    public class CharEvent : BaseEvent
    {
        public char Char { get; }

        public CharEvent(DateTime time, char character) : base(time)
        {
            Char = character;
        }
    }

    public delegate void CharEventHandler(object sender, CharEvent eventArg);
}
