﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalEngine
{
    public static class EnumerationConvert
    {
        public static ConsoleColor ToConsoleColor(LogColor logColor)
        {
            return (ConsoleColor)logColor;
        }
    }
}
