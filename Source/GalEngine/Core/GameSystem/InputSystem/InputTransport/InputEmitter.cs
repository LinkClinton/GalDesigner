﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalEngine
{
    public class InputEmitter
    {
        public void Accept(InputAction action)
        {
            InputListener.Accept(action);
        }
    }
}
